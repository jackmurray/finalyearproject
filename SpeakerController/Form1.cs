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
using System.ServiceModel;
using LibSSDP;

namespace SpeakerController
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDiscover_Click(object sender, EventArgs e)
        {
            SSDPClient c = new SSDPClient();
            c.OnResponsePacketReceived += c_OnResponsePacketReceived;
            c.StartDiscovery();
        }

        void c_OnResponsePacketReceived(object sender, ResponsePacketReceivedArgs args)
        {
            MessageBox.Show("Got a packet for " + args.Packet.Location);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetClient("http://10.0.1.6:10451/Control.svc").GetVersion().ToString());
        }

        private ReceiverService.ReceiverServiceClient GetClient(string uri)
        {
            var client = new ReceiverService.ReceiverServiceClient(new BasicHttpBinding(), new EndpointAddress(uri));
            client.Endpoint.EndpointBehaviors.Add(new LibUtil.ClientServiceProcessor(new LibSecurity.WebServiceProtector(false)));
            return client;
        }
    }
}
