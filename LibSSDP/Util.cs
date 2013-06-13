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
            UdpClient c = new UdpClient(AddressFamily.InterNetwork);
            c.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));
            return c;
        }

        public static UdpClient GetListener(IPEndPoint ep)
        {
            var client = GetClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); //Windows's SSDP thing already has a socket here, so we need this option so we can open one too.
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(SSDPPacket.LocalEndpoint);
            return client;
        }

        public static string ToPacketString(this Method m)
        {
            switch (m)
            {
                case Method.Announce:
                    return "NOTIFY * HTTP/1.1";
                case Method.Search:
                    return "M-SEARCH * HTTP/1.1";
                case Method.Respond:
                    return "HTTP/1.1 200 OK";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Method Lookup(string s)
        {
            switch (s)
            {
                case "NOTIFY * HTTP/1.1":
                    return Method.Announce;
                case "M-SEARCH * HTTP/1.1":
                    return Method.Search;
                case "HTTP/1.1 200 OK":
                    return Method.Respond;
                default:
                    throw new ArgumentOutOfRangeException();
            } 
        }
    }
}
