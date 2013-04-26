using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace LibSSDP
{
    public class SSDPPacket
    {
        public static IPEndPoint RemoteEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900); //Standard SSDP address.
        public static IPEndPoint LocalEndpoint = new IPEndPoint(IPAddress.Any, 1900); //Standard SSDP address.
        private const string NT = "urn:multicastspeakers:speaker";
        public Guid guid;
        public string Nts, Location;
        public Method Method;

        private SSDPPacket()
        {
            
        }

        public void Send(UdpClient c)
        {
            byte[] data = Serialize();
            c.Send(data, data.Length, RemoteEndpoint);
        }

        private byte[] Serialize()
        {
            StringBuilder packet = new StringBuilder();
            packet.AppendFormat("{0} * HTTP/1.1\r\n", Method.ToPacketString());
            packet.AppendFormat("USN: uuid:{0}\r\n", guid); //Eventually we'll use the cert thumbprint as the ID.
            packet.AppendFormat("NTS: {0}\r\n", Nts);
            packet.AppendFormat("NT: {0}\r\n", NT);
            if (Location != null)
                packet.AppendFormat("Location: {0}\r\n", Location);
            return Encoding.ASCII.GetBytes(packet.ToString());
        }

        private static string GetOurControlURL()
        {
            IPAddress ourip =
                Dns.GetHostEntry(Dns.GetHostName())
                   .AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            return String.Format("http://{0}:10451/Control.svc", ourip);
        }

        public static SSDPPacket BuildAnnouncePacket(Guid g)
        {
            return new SSDPPacket()
                {
                    Method = Method.Announce,
                    guid = g,
                    Nts = "ssdp:alive",
                    Location = GetOurControlURL()
                };
        }

        public static SSDPPacket Parse(string s)
        {
            SSDPPacket p = new SSDPPacket();
            string[] lines = s.Split(new string[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                string method = lines[0].Split(new char[] { ' ' })[0];
                p.Method = Util.Lookup(method);
                bool checkedNT = false;
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    int endOfHeaderName = line.IndexOf(':');
                    string headername = line.Substring(0, endOfHeaderName);
                    string headerval = line.Substring(endOfHeaderName+1);
                    if (headerval[0] == ' ')
                        headerval = headerval.Substring(1); //Eat a leading space.

                    switch (headername) //We'll just ignore any headers we don't understand.
                    {
                        case "USN":
                            p.guid = Guid.Parse(headerval.Replace("uuid:", ""));
                            break;
                        case "NTS":
                            p.Nts = headerval;
                            break;
                        case "NT":
                            if (headerval == SSDPPacket.NT)
                                checkedNT = true;
                            break;
                        case "Location":
                            p.Location = headerval;
                            break;
                    }
                }

                if (checkedNT != true)
                    return null;

                return p;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("SSDPacket.Parse() failed: " + ex.Message);
                throw;
            }
        }
    }

    //Make sure to update the extension methods.
    public enum Method
    {
        Announce,
        Search
    };
}
