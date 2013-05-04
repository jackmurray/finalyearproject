using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using LibSecurity;

namespace LibSSDP
{
    public abstract class SSDPPacket
    {
        public static IPEndPoint RemoteEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900); //Standard SSDP address.
        public static IPEndPoint LocalEndpoint = new IPEndPoint(IPAddress.Any, 1900); //Standard SSDP address.
        protected const string ServiceType = "urn:multicastspeakers:speaker";
        public string fingerprint, Location, Signature;
        public Method Method;

        public void Send(UdpClient c)
        {
            byte[] data = Serialize();
            c.Send(data, data.Length, RemoteEndpoint);
        }

        protected byte[] Serialize()
        {
            StringBuilder packet = new StringBuilder();
            packet.AppendFormat("{0}\r\n", Method.ToPacketString());
            packet.AppendFormat("USN: fingerprint:{0}\r\n", fingerprint);
            packet.AppendFormat("Date: {0}\r\n", LibUtil.Util.FormatDate(DateTime.Now));
            packet.Append(GetSpecificHeaders()); //Get any specific headers for different packet types.

            if (Location != null)
                packet.AppendFormat("Location: {0}\r\n", Location);
            if (Signature != null)
                packet.AppendFormat("x-signature: {0}\r\n", Signature);

            return Encoding.ASCII.GetBytes(packet.ToString());
        }

        protected abstract string GetSpecificHeaders();

        protected byte[] GetHash()
        {
            return Hasher.Create().Hash(Serialize());
        }

        protected string GetSignature(KeyManager key)
        {
            return LibUtil.Util.BytesToBase64String(key.Sign(GetHash()));
        }

        public static SSDPPacket Parse(string s)
        {
            SSDPPacket p;
            string[] lines = s.Split(new string[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                Method m = Util.Lookup(lines[0]);
                switch (m)
                {
                    case Method.Announce:
                        p = new SSDPAnnouncePacket();
                        break;
                    case Method.Respond:
                        p = new SSDPResponsePacket();
                        break;
                    case Method.Search:
                        p = new SSDPSearchPacket();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Invalid HTTP method in received packet.");
                }

                bool checkedServiceType = false;
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
                            p.fingerprint = headerval.Replace("fingerprint:", "");
                            break;
                        case "NT": //Announce uses NT
                        case "ST": //Discovery/Reponse uses ST.
                            //We're actually more liberal here than we should be. The protocol spec dictates which packet types should use which header, but we don't care as much - so long as we get *something* it's cool.
                            if (headerval == ServiceType)
                                checkedServiceType = true;
                            break;
                        case "Location":
                            p.Location = headerval;
                            break;
                        case "x-signature":
                            p.Signature = headerval;
                            break;
                    }
                }

                if (checkedServiceType != true)
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
        Search,
        Respond
    };
}
