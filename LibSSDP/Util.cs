using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LibSSDP
{
    static class Util
    {
        public static UdpClient GetClient()
        {
            UdpClient c = new UdpClient();
            c.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));
            return c;
        }

        public static string ToPacketString(this Method m)
        {
            switch (m)
            {
                case Method.Announce:
                    return "NOTIFY";
                case Method.Search:
                    return "M-SEARCH";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Method Lookup(string s)
        {
            switch (s)
            {
                case "NOTIFY":
                    return Method.Announce;
                case "M-SEARCH":
                    return Method.Search;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        }
    }
}
