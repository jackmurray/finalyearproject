using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibAudio;
using LibCommon;
using LibConfig;
using LibSSDP;
using LibSecurity;
using LibService;
using LibTransport;
using LibUtil;
using Trace = LibTrace.Trace;

namespace SpeakerController
{
    public partial class Form1 : Form
    {
        private KeyManager key;
        private CertManager cert;
        private List<Receiver> Receivers = new List<Receiver>(); //list of all receivers discovered by SSDP
        private Trace Log;
        private SSDPClient ssdpc;
        private IAudioFormat audio;
        private RTPOutputStream stream;
        private bool firstRun = true;
        private ActiveReceiverManager activeReceiverManager;
        private LoopbackWavCapture loopback;
        private CircularStream circbuf = new CircularStream();
        private ControllerState state = new ControllerState() {Mode = StreamMode.File};

        private PacketEncrypterKeyManager pekm;
        private KeyManager rtpsignkey;

        public bool ShouldAdvanceLog { get { return !chkLogPause.Checked; } }

        public Form1()
        {
            InitializeComponent();
            Text = string.Format("{0}-{1}", GetBuildVersion(), GetBuildFlavour());
        }

        public static string GetBuildFlavour()
        {
#if TEST
            return "TEST";
#elif DEBUG
            return "DEBUG";
#else
            return "RELEASE";
#endif
        }

        private static string GetBuildVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        private void btnDiscover_Click(object sender, EventArgs e)
        {
            DoDiscovery();
        }

        private void DoDiscovery()
        {
            lstDevicesAvail.Items.Clear();
            Receivers.Clear();
            ssdpc = new SSDPClient();
            ssdpc.OnResponsePacketReceived += c_OnResponsePacketReceived;
            ssdpc.OnAnnouncePacketReceived += c_OnResponsePacketReceived;
            ssdpc.StartDiscovery();
        }

        //this is called by the receiver thread, hence the Invoke().
        void c_OnResponsePacketReceived(object sender, ResponsePacketReceivedArgs args)
        {
            string val = string.Format("{0} - {1}", args.Packet.friendlyName, args.Source.ToString());
            Color c;
            if (!TrustedKeys.Contains(args.Packet.fingerprint))
            {
                c = Color.Black;
                Log.Information("Processed SSDP response from untrusted device.");
            }
            else
            {
                Verifier v = new Verifier(TrustedKeys.Get(args.Packet.fingerprint));
                bool result = v.Verify(args.StrippedPacket, Util.Base64ToByteArray(args.Packet.signature));
                if (result)
                {
                    TimeSpan diff = DateTime.Now.ToUniversalTime() - args.Packet.Date;
                    if (Math.Abs(diff.TotalSeconds) > Config.GetInt(Config.MAX_TIMEDIFF))
                    {
                        c = Color.Red;
                        Log.Critical("!!!Time check failed on SSDP packet. Diff was " + diff.TotalSeconds + " seconds.!!!");
                    }
                    else
                    {
                        c = Color.Green;
                        Log.Information("Processed correctly signed SSDP response from trusted device.");
                    }
                }
                else
                {
                    c = Color.Red;
                    Log.Critical("!!!Incorrectly signed SSDP response from trusted device. This could be a result of a malicious entity attempting to impersonate a device.!!!");
                }
            }
            BeginInvoke((Action)(() =>
             {
                 ListViewItem item = new ListViewItem(val) {ForeColor = c};
                 lstDevicesAvail.Items.Add(item);
                 var ep = new IPEndPoint(args.Source, args.Packet.Location);
                 string f = args.Packet.fingerprint;
                 var r = TrustedKeys.Contains(f) ? new Receiver(ep, TrustedKeys.Get(f)) : new Receiver(ep, f);
                 Receivers.Add(r);
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Receiver r = Receivers[lstDevicesAvail.SelectedIndices[0]];
            SslClient ssl = r.GetSsl(cert, key);

            CommonServiceClient client = ssl.GetClient<CommonServiceClient>();

            try
            {
                var response = client.GetVersions();
                StringBuilder sb = new StringBuilder();
                foreach (var kvp in response)
                    sb.AppendLine(kvp.Key + " = " + kvp.Value);

                MessageBox.Show(sb.ToString());
            }
            catch (ServiceException ex)
            {
                MessageBox.Show(ex.Message + ": " + ex.Code, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ssl.Close();
            }
        }

        private void Setup()
        {
            LibTrace.Trace.ExtraListeners.Add(new DataGridTraceListener(this, dGridTrace));
            LibTrace.Trace.Initialised = true;
            Log = Trace.GetInstance("SpeakerController");
            Config.CreateItems();
            Config.LoadTrustedKeys();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Setup();
            this.pekm = new PacketEncrypterKeyManager();
            key = KeyManager.GetKey();
            cert = CertManager.GetCert(key);

            ServiceRegistration.Register(new KeyService(this.pekm, this.IsClientValid));
            ServiceRegistration.Start(cert.ToDotNetCert(key), 10452);


            string name = "Unnamed Device";
            if (!Config.Exists(Config.DEVICE_FRIENDLY_NAME))
                SetFriendlyName(name);
            else
                name = Config.Get(Config.DEVICE_FRIENDLY_NAME);

            txtFriendlyName.Text = name;

            DoDiscovery();

            cmbLogLevel.SelectedItem = Config.Get(Config.TRACE_LEVEL).ToString();
            chkEnableEncrypt.Checked = Config.GetFlag(Config.ENABLE_ENCRYPTION);
            chkEnableAuth.Checked = Config.GetFlag(Config.ENABLE_AUTHENTICATION);
            this.activeReceiverManager = new ActiveReceiverManager(lstDevicesActive);
            UpdateButtonState();
        }

        private void SetFriendlyName(string name)
        {
            Config.Set(Config.DEVICE_FRIENDLY_NAME, name);
        }

        private void btnSaveFriendlyName_Click(object sender, EventArgs e)
        {
            SetFriendlyName(txtFriendlyName.Text);
        }

        private void btnGetCert_Click(object sender, EventArgs e)
        {
            Receiver r = Receivers[lstDevicesAvail.SelectedIndices[0]];
            SslClient ssl = r.GetSsl(cert, key);

            PairingServiceClient c = ssl.GetClient<PairingServiceClient>();
            byte[] challenge = c.GetChallengeBytes();
            Log.Verbose("Got challenge " + Util.BytesToHexString(challenge));
            var cr = new ChallengeResponse(cert.Fingerprint, challenge); //sign the challenge with our fingerprint and the key
            byte[] sig = cr.Sign(Config.Get(Config.PAIRING_KEY));
            bool res;
            try
            {
                res = c.Pair(challenge, sig);
                if (res)
                    MessageBox.Show("Pairing succeeded.");
                else
                    MessageBox.Show("Pairing failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ServiceException ex)
            {
                MessageBox.Show("Pairing failed with error code " + ex.Code + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ssdpc != null) ssdpc.Stop();
            if (stream != null) stream.Stop();
            if (loopback != null && loopback.Playing) loopback.Stop();
            ServiceRegistration.Stop();
        }

        private void btnListPairedDevices_Click(object sender, EventArgs e)
        {
            new frmPairedDevices().ShowDialog();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK)
            {
                FileStream s = File.OpenRead(ofd.FileName);
                IAudioFormat r = SupportedAudio.FindReaderForFile(new AudioFileReader(s));
                /*if (r != null)
                {
                    MessageBox.Show(r.BitRate.ToString());
                    MessageBox.Show(r.Frequency.ToString());
                    MessageBox.Show((r as MP3Format).BytesPerFrame.ToString());
                    float i = 0;
                    int count = 0;
                    while (!r.EndOfFile())
                    {
                        i += r.GetDataForTime(0.01f).Item1;
                        count++;
                    }
                    MessageBox.Show("Asked for " + count*0.01f + " seconds, got " + i);
                }*/
                this.audio = r;
            }
        }

        private void btnJoinGroup_Click(object sender, EventArgs e)
        {
            Receiver r = Receivers[lstDevicesAvail.SelectedIndices[0]];
            SslClient ssl = r.GetSsl(cert, key);

            TransportServiceClient tclient = ssl.GetClient<TransportServiceClient>();
            CommonServiceClient commonclient = ssl.GetClient<CommonServiceClient>();

            try
            {
                //check remote party has same or higher versions for all components.
                var remoteversions = commonclient.GetVersions();
                foreach (var kvp in Util.GetComponentVersions())
                {
                    if (!remoteversions.Keys.Contains(kvp.Key))
                        throw new Exception("Remote party did not specify a version for " + kvp.Key);

                    if (remoteversions[kvp.Key] < kvp.Value)
                        throw new Exception("Remote party specified version " + remoteversions[kvp.Key] +
                                            " for component " + kvp.Key + ". Need version " + kvp.Value + " or higher.");

                    Log.Verbose("[Version Check] [" + kvp.Key + "] Remote: " + remoteversions[kvp.Key] + " Local: " +
                                kvp.Value);
                }

                var remoteconfig = commonclient.GetConfigState();
                if (!remoteconfig.Equals(ConfigState.GetConfigState()))
                {
                    Log.Error("Local config state: " + ConfigState.GetConfigState());
                    Log.Error("Remote config state: " + remoteconfig);
                    throw new Exception("Remote config state didn't match ours!");
                }
                else
                    Log.Verbose("Config state check passed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to add receiver to group: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.Error("Unable to add receiver to group: " + ex.Message);
                return;
            }

            tclient.JoinGroup(txtGroupAddr.Text);

            if (Config.GetFlag(Config.ENABLE_ENCRYPTION))
                tclient.SetEncryptionKey(this.pekm.Key, this.pekm.Nonce);

            if (Config.GetFlag(Config.ENABLE_AUTHENTICATION))
            {
                if (rtpsignkey == null)
                    rtpsignkey = KeyManager.CreateTemporaryKey();
                tclient.SetSigningKey(this.rtpsignkey);
            }

            IPAddress ourIP = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            tclient.SetControllerAddress(new IPEndPoint(ourIP, 10452));

            activeReceiverManager.Add(r);

            if (stream != null && stream.State == OutputStreamState.Started)
                stream.SendSync();

            btnStream.Enabled = true;
            btnStreamTestSound.Enabled = true;

            Log.Verbose("Added " + r);
        }

        private void btnStream_Click(object sender, EventArgs e)
        {
            //For the first run, each receiver will have had the key delivered already.
            //For each run after that, we generate a new key and push it to all the active receivers.
            if (!firstRun)
            {
                this.pekm.SetNextKey(this.pekm.GenerateNewKey(), this.pekm.GenerateNewNonce());
                this.pekm.UseNextKey();

                if (Config.GetFlag(Config.ENABLE_ENCRYPTION))
                {
                    foreach (Receiver r in activeReceiverManager)
                    {
                        try
                        {
                            SslClient ssl = r.GetSsl(cert, key);
                            TransportServiceClient tclient = ssl.GetClient<TransportServiceClient>();
                            tclient.SetEncryptionKey(this.pekm.Key, this.pekm.Nonce);
                            Log.Information("Delivered new key to " + r);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Failed to deliver key to " + r + ". Exception was: " + ex.Message);
                        }
                    }
                }
            }

            this.stream = new RTPOutputStream(new IPEndPoint(IPAddress.Parse(txtGroupAddr.Text), 10452));

            if (Config.GetFlag(Config.ENABLE_AUTHENTICATION))
                this.stream.EnableSigning(Signer.Create(rtpsignkey));
            if (Config.GetFlag(Config.ENABLE_ENCRYPTION))
                this.stream.EnableEncryption(pekm);
            
            stream.Stream(audio);
            firstRun = false;
        }

        private void btnStreamTestSound_Click(object sender, EventArgs e)
        {
            FileStream s = File.OpenRead("test.mp3");
            IAudioFormat r = SupportedAudio.FindReaderForFile(new AudioFileReader(s));
            this.audio = r;
            btnStream_Click(this, null);
        }

        private void cmbLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            SourceLevels l = (SourceLevels)Enum.Parse(typeof (SourceLevels), cmbLogLevel.SelectedItem as string);
            Trace.SetLevel(l);
            Config.Set(Config.TRACE_LEVEL, cmbLogLevel.SelectedItem as string);
        }

        private void chkEnableEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            Config.Set(Config.ENABLE_ENCRYPTION, (chkEnableEncrypt.Checked ? bool.TrueString : bool.FalseString));
            state.EncryptionEnabled = chkEnableEncrypt.Checked;
            UpdateButtonState();
        }

        private void chkEnableAuth_CheckedChanged(object sender, EventArgs e)
        {
            Config.Set(Config.ENABLE_AUTHENTICATION, (chkEnableAuth.Checked ? bool.TrueString : bool.FalseString));
            state.AuthenticationEnabled = chkEnableAuth.Checked;
            UpdateButtonState();
        }

        private void btnRotateKey_Click(object sender, EventArgs e)
        {
            this.stream.RotateKey();
        }

        private void btnEjectDevice_Click(object sender, EventArgs e)
        {
            Receiver r = activeReceiverManager[lstDevicesActive.SelectedIndices[0]];
            Log.Information("Removing " + r + " from active stream.");
            activeReceiverManager.RemoveAt(lstDevicesActive.SelectedIndices[0]);
            if (this.stream != null)
                this.stream.RotateKey();
        }

        public bool IsClientValid(X509Certificate client)
        {
            return activeReceiverManager.SingleOrDefault(r => r.Fingerprint == client.GetCertHashString()) != null;
        }

        private void lstDevicesAvail_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDevicesAvail.SelectedIndices.Count > 0)
                state.AvailableDeviceSelected = true;
            else
                state.AvailableDeviceSelected = false;

            UpdateButtonState();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (this.stream != null)
                this.stream.Stop(state.Mode == StreamMode.File); //seeking to the start only makes sense for File mode, not Loopback
            if (state.Mode == StreamMode.Loopback)
                loopback.Stop();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (this.stream != null)
            {
                if (stream.State == OutputStreamState.Started)
                    this.stream.Pause();
                else if (stream.State == OutputStreamState.Paused)
                    this.stream.Resume();
            }
        }

        private void btnLoopback_Click(object sender, EventArgs e)
        {
            this.loopback = new LoopbackWavCapture(circbuf);
        }

        private void btnPlaySamples_Click(object sender, EventArgs e)
        {
            this.audio = SupportedAudio.FindReaderForFile(new AudioStreamReader(circbuf));
            btnStream_Click(this, null);
        }

        private void UpdateButtonState()
        {
            if (!state.EncryptionEnabled)
            {
                state.AuthenticationEnabled = false;
                chkEnableAuth.Enabled = false; //auth requires encryption. can't toggle the chkbox otherwise we'd call this method again infinitely.

                btnRotateKey.Enabled = false; //can't rotate without enc.
                btnEjectDevice.Enabled = false; //can't eject devices without enc/rotation
            }
            else
            {
                chkEnableAuth.Enabled = true;
                btnRotateKey.Enabled = true;
                btnEjectDevice.Enabled = true;
            }

            if (state.AvailableDeviceSelected)
            {
                btnJoinGroup.Enabled = true;
                btnTrustDevice.Enabled = true;
                button1.Enabled = true;
            }
            else
            {
                btnJoinGroup.Enabled = false;
                btnTrustDevice.Enabled = false;
                button1.Enabled = false;
            }

            if (state.Mode == StreamMode.File)
            {
                grpFileStream.Enabled = true;
                grpLoopbackStream.Enabled = false;
            }
            else
            {
                grpFileStream.Enabled = false;
                grpLoopbackStream.Enabled = true;
            }
        }

        private void radioFile_CheckedChanged(object sender, EventArgs e)
        {
            state.Mode = radioFile.Checked ? StreamMode.File : StreamMode.Loopback;
            UpdateButtonState();
        }
    }

    public class DataGridTraceListener : TraceListener
    {
        private Form1 _f;
        private DataGridView _dgrid;

        public DataGridTraceListener(Form1 f, DataGridView dgrid)
        {
            _f = f;
            _dgrid = dgrid;
        }

        public void PrintMessage(string source, TraceEventType eventType, int id, string message)
        {
            /*
             * Use BeginInvoke() to queue up the message for adding to the dgrid. Previously we used Invoke which
             * caused deadlocks.
             * 
             * The GUI thread could be busy performing some operation that would eventually try and send a log message.
             * Meanwhile, another thread performed an operation which sent a log message. This thread would then lock the .NET
             * framework's global tracing lock (this could probably have been turned off instead, but this is a better solution)
             * and proceed to try and call Invoke() to have the GUI thread update the datagrid. The Invoke() call would block because
             * it's synchronous, and the GUI thread was currently busy. Eventually, the GUI thread would try and send his log message
             * and cause a deadlock. The worker thread would hold the tracing lock and be unable to release it because it couldn't call Invoke(),
             * and the GUI thread would unable to process the Invoke() since it was waiting for the tracing lock.
             * 
             * BeginInvoke() allows the worker thread's logging call to return straight away and release the tracing lock, then the GUI thread
             * can acquire it, queue up his log message and then go service both of them.
             * */
            _f.BeginInvoke((Action)(() =>
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(_dgrid);
                    row.Cells[0].Value = eventType.ToString();
                    row.Cells[1].Value = source;
                    row.Cells[2].Value = id;
                    row.Cells[3].Value = message;
                    _dgrid.Rows.Add(row);
                    if (_f.ShouldAdvanceLog)
                        _dgrid.FirstDisplayedScrollingRowIndex = _dgrid.Rows.Count - 1;
                }));
        }

        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            PrintMessage(source, eventType, id, message);
        }
    }
}
