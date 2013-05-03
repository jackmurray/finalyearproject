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
    }
}
