using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LibTransport
{
    public class RTPOutputStream
    {
        private UdpClient c;
        private IPEndPoint ep;

        public RTPOutputStream(IPEndPoint ep)
        {
            if (ep.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("IP endpoint must be AF_INET. IPv6 is currently not supported.");
            if (!LibUtil.Util.IsMulticastAddress(ep.Address))
                throw new ArgumentException("IP address must be in the range 224.0.0.0/4 (multicast)");

            c = new UdpClient(AddressFamily.InterNetwork);
            c.JoinMulticastGroup(ep.Address);
            this.ep = ep;
        }

        public void Send(RTPPacket p)
        {
            byte[] data = p.Serialise();
            c.Send(data, data.Length, ep);
        }
    }
}
