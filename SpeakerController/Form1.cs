using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibConfig;
using LibSSDP;
using LibSecurity;
using LibUtil;

namespace SpeakerController
{
    public partial class Form1 : Form
    {
        private KeyManager key;
        private CertManager cert;
        private List<IPEndPoint> Receivers = new List<IPEndPoint>();

        public Form1()
        {
            InitializeComponent();
            Text = string.Format("{0}-{1}", GetBuildVersion(), GetBuildFlavour());

            Setup();
            key = KeyManager.GetKey();
            cert = CertManager.GetCert(key);
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
            lstDevices.Items.Clear();
            Receivers.Clear();
            SSDPClient c = new SSDPClient();
            c.OnResponsePacketReceived += c_OnResponsePacketReceived;
            c.StartDiscovery();
        }

        //this is called by the receiver thread, hence the Invoke().
        void c_OnResponsePacketReceived(object sender, ResponsePacketReceivedArgs args)
        {
            string val = string.Format("{0} - {1}", args.Packet.friendlyName, args.Source.ToString());
            Invoke((Action)(() => { lstDevices.Items.Add(val);
                                      Receivers.Add(new IPEndPoint(args.Source, args.Packet.Location));
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPEndPoint ep = Receivers[lstDevices.SelectedIndex];
            SslClient ssl = new SslClient(cert.ToDotNetCert(key));
            ssl.Connect(ep);
            MessageBox.Show(ssl.GetVal().ToString());
        }

        private void Setup()
        {
            Util.CreateDirs();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string name = "Unnamed Device";
            if (!Config.Exists(Config.DEVICE_FRIENDLY_NAME))
                SetFriendlyName(name);
            else
                name = Config.Get(Config.DEVICE_FRIENDLY_NAME);

            txtFriendlyName.Text = name;
        }

        private void SetFriendlyName(string name)
        {
            Config.Set(Config.DEVICE_FRIENDLY_NAME, name);
        }

        private void btnSaveFriendlyName_Click(object sender, EventArgs e)
        {
            SetFriendlyName(txtFriendlyName.Text);
        }
    }
}
