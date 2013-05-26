using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using LibSSDP;
using LibSecurity;
using LibUtil;

namespace SpeakerController
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Text = string.Format("{0}-{1}", GetBuildVersion(), GetBuildFlavour());

            Uri uri = Util.GetOurControlURL(true); //Get URI that we're going to run on.

            ServiceHost host = new ServiceHost(typeof(ControllerService), uri);
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true
            };
            host.Description.Behaviors.Add(smb);

            //App EP
            ServiceEndpoint appEP = host.AddServiceEndpoint(typeof(IControllerService), new BasicHttpBinding(), uri);
#if !DEBUG //If we're a debug build, don't encrypt stuff.
            appEP.Behaviors.Add(new ServerRequestProcessor(new LibSecurity.WebServiceProtector(true)));
#endif
            //MEX EP
            host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                                    MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            host.Open();
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
            MessageBox.Show(GetClient(lstDevices.SelectedItem.ToString()).GetVersion().ToString());
        }

        private ReceiverService.ReceiverServiceClient GetClient(string uri)
        {
            var client = new ReceiverService.ReceiverServiceClient(new BasicHttpBinding(), new EndpointAddress(uri));
#if !DEBUG //If we're a debug build, don't try and decrypt stuff. This means that debug builds of the controller must be used with debug receivers too.
            client.Endpoint.EndpointBehaviors.Add(new ClientServiceProcessor(new LibSecurity.WebServiceProtector(false)));
#endif
            return client;
        }
    }
}
