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
                Version response = client.GetVersion();
                MessageBox.Show(response.ToString());
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
            Util.CreateItems();
            Config.LoadTrustedKeys();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Setup();
            key = KeyManager.GetKey();
            cert = CertManager.GetCert(key);


            string name = "Unnamed Device";
            if (!Config.Exists(Config.DEVICE_FRIENDLY_NAME))
                SetFriendlyName(name);
            else
                name = Config.Get(Config.DEVICE_FRIENDLY_NAME);

            txtFriendlyName.Text = name;

            DoDiscovery();
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
            ssdpc.Stop();
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
                AudioFileReader r = SupportedAudio.FindReaderForFile(s);
                if (r != null)
                {
                    MessageBox.Show(r.BitRate.ToString());
                    MessageBox.Show(r.Frequency.ToString());
                    MessageBox.Show((r as MP3Format).BytesPerFrame.ToString());
                    int i = 0;
                    while (!r.EndOfFile())
                    {
                        r.GetFrame();
                        i++;
                    }
                    MessageBox.Show(i.ToString());
                }
            }
        }
    }

    public class DataGridTraceListener : TraceListener
    {
        private Form _f;
        private DataGridView _dgrid;

        public DataGridTraceListener(Form f, DataGridView dgrid)
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
