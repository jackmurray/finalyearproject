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
using LibSSDP;
using LibSecurity;
using LibUtil;

namespace SpeakerController
{
    public partial class Form1 : Form
    {
        private KeyManager key;
        private CertManager cert;

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
            SSDPClient c = new SSDPClient();
            c.OnResponsePacketReceived += c_OnResponsePacketReceived;
            c.StartDiscovery();
        }

        //this is called by the receiver thread, hence the Invoke().
        void c_OnResponsePacketReceived(object sender, ResponsePacketReceivedArgs args)
        {
            Invoke((Action)(() => lstDevices.Items.Add(args.Packet.Location)));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(new SslClient(cert.ToDotNetCert(key)).Connect(10452).ToString());
            //MessageBox.Show(GetClient(lstDevices.SelectedItem.ToString()).GetVersion().ToString());
        }

        private void Setup()
        {
            Util.CreateDirs();
        }
    }
}
