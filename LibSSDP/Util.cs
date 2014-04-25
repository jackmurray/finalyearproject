using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LibConfig;
using LibSecurity;
using LibTrace;

namespace LibSSDP
{
    public static class Util
    {
        public static UdpClient GetClient()
        {
            UdpClient c = new UdpClient(AddressFamily.InterNetwork);
            c.JoinMulticastGroup(IPAddress.Parse("239.255.255.250"));
            if (!Config.IsRunningOnMono) //TODO: Later, build the new mono and work out how to do this for linux.
            {
                int netIfIndex = GetNetIFIndex();
                if (netIfIndex != -1) //-1 means we couldn't find it.
                    c.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, netIfIndex);
            }
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

        private static int GetNetIFIndex()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string wantedIP = Config.Get(Config.IP_ADDRESS);
            foreach (NetworkInterface iface in nics)
            {
                var props = iface.GetIPProperties();
                if (!props.MulticastAddresses.Any())
                    continue;
                if (!iface.SupportsMulticast)
                    continue;
                if (iface.OperationalStatus != OperationalStatus.Up)
                    continue;
                var ip4 = props.GetIPv4Properties();
                if (ip4 == null)
                    continue;

                var addrs = props.UnicastAddresses;
                bool found = addrs.Any(addr => addr.Address.ToString() == wantedIP);
                if (!found) continue;
                Trace.GetInstance("LibSSDP").Verbose("IFACE Index: " + ip4.Index + ", ADDR=" + wantedIP);
                return IPAddress.HostToNetworkOrder(ip4.Index);
            }

            Trace.GetInstance("LibSSDP").Error("Unable to find preferred interface. Using default.");
            return -1;
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

        public static string StripPacket(byte[] data)
        {
            string stripped = Encoding.ASCII.GetString(data);
            int pos = stripped.IndexOf("USN: fingerprint");
            //TODO: do this properly (i.e. remove the headers and serialise the packet rather than just hacking the end of the string off).
            stripped = stripped.Remove(pos);
            return stripped;
        }

        public static bool CheckSignature(SSDPSignedPacket p, byte[] Stripped)
        {
            Verifier v = new Verifier(TrustedKeys.Get(p.fingerprint));
            bool result = v.Verify(Stripped, LibUtil.Util.Base64ToByteArray(p.signature));
            return result;
        }
    }
}
