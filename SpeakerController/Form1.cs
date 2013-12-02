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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibAudio;
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
        private List<IPEndPoint> Receivers = new List<IPEndPoint>();
        private Trace Log;
        private SSDPClient ssdpc;
        private IAudioFormat audio;
        private RTPOutputStream stream;

        private PacketEncrypterKeyManager pekm = new PacketEncrypterKeyManager();

        public bool ShouldAdvanceLog { get { return !chkLogPause.Checked; } }

        public Form1()
        {
            InitializeComponent();
            Text = string.Format("{0}-{1}", GetBuildVersion(), GetBuildFlavour());
        }

        public static string GetBuildFlavour()
        {
#if DEBUG
            return "DEBUG";
#elif NONDEBUG
            return "NONDEBUG";
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
            lstDevices.Items.Clear();
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
                 lstDevices.Items.Add(item);
                 Receivers.Add(new IPEndPoint(args.Source, args.Packet.Location));
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPEndPoint ep = Receivers[lstDevices.SelectedIndices[0]];
            SslClient ssl = new SslClient(cert.ToDotNetCert(key));
            ssl.Connect(ep);

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
            Log = Trace.GetInstance("SpeakerController");
            Config.CreateItems();
            Config.LoadTrustedKeys();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Setup();
            key = KeyManager.GetKey();
            cert = CertManager.GetCert(key);

            ServiceRegistration.Register(new KeyService(this.pekm));
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
            IPEndPoint ep = Receivers[lstDevices.SelectedIndices[0]];
            SslClient ssl = new SslClient(cert.ToDotNetCert(key));
            ssl.Connect(ep);

            PairingServiceClient c = ssl.GetClient<PairingServiceClient>();
            byte[] challenge = c.GetChallengeBytes();
            Log.Verbose("Got challenge " + Util.BytesToHexString(challenge));
            var cr = new ChallengeResponse(challenge);
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
                Stream s = File.OpenRead(ofd.FileName);
                IAudioFormat r = SupportedAudio.FindReaderForFile(s);
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
            IPEndPoint ep = Receivers[lstDevices.SelectedIndices[0]];
            SslClient ssl = new SslClient(cert.ToDotNetCert(key));
            ssl.Connect(ep);

            TransportServiceClient tclient = ssl.GetClient<TransportServiceClient>();
            CommonServiceClient commonclient = ssl.GetClient<CommonServiceClient>();

            //check remote party has same or higher versions for all components.
            var remoteversions = commonclient.GetVersions();
            foreach (var kvp in Util.GetComponentVersions())
            {
                if (!remoteversions.Keys.Contains(kvp.Key))
                    throw new Exception("Remote party did not specify a version for " + kvp.Key);

                if (remoteversions[kvp.Key] < kvp.Value)
                    throw new Exception("Remote party specified version " + remoteversions[kvp.Key] + " for component " + kvp.Key + ". Need version " + kvp.Value + " or higher.");

                Log.Verbose("[Version Check] ["+kvp.Key+"] Remote: " + remoteversions[kvp.Key] + " Local: " + kvp.Value);
            }

            if (Config.GetFlag(Config.ENABLE_ENCRYPTION))
                tclient.JoinGroupEncrypted(txtGroupAddr.Text, this.pekm.Key, this.pekm.Nonce);
            else
                tclient.JoinGroup(txtGroupAddr.Text);
        }

        private void btnStream_Click(object sender, EventArgs e)
        {
            if (Config.GetFlag(Config.ENABLE_AUTHENTICATION))
                this.stream = new RTPOutputStream(new IPEndPoint(IPAddress.Parse(txtGroupAddr.Text), 10452), Config.GetFlag(Config.ENABLE_ENCRYPTION), pekm, Signer.Create(key));
            else
                this.stream = new RTPOutputStream(new IPEndPoint(IPAddress.Parse(txtGroupAddr.Text), 10452), Config.GetFlag(Config.ENABLE_ENCRYPTION), pekm);
            
            stream.Stream(audio);
        }

        private void btnStreamTestSound_Click(object sender, EventArgs e)
        {
            Stream s = File.OpenRead("test.mp3");
            IAudioFormat r = SupportedAudio.FindReaderForFile(s);
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
        }

        private void chkEnableAuth_CheckedChanged(object sender, EventArgs e)
        {
            Config.Set(Config.ENABLE_AUTHENTICATION, (chkEnableAuth.Checked ? bool.TrueString : bool.FalseString));
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
