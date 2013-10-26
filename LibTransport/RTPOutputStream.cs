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

        public RTPOutputStream(IPEndPoint ep)
        {
            if (ep.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("IP endpoint must be AF_INET. IPv6 is currently not supported.");
            if ((ep.Address.GetAddressBytes()[0] & 0xF0) != 0xE0)
                throw new ArgumentException("IP address must be in the range 224.0.0.0/4 (multicast)");
        }
    }
}
