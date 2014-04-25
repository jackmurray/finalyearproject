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
    public abstract class SSDPPacket
    {
        public static IPEndPoint RemoteEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900); //Standard SSDP address.
        public static IPEndPoint LocalEndpoint = new IPEndPoint(IPAddress.Any, 1900); //Standard SSDP address.
        protected const string ServiceType = "urn:multicastspeakers:speaker";
        public Method Method;
        public DateTime Date;

        public void Send(UdpClient c)
        {
            byte[] data = Serialize();
            c.Send(data, data.Length, RemoteEndpoint);
        }

        public byte[] Serialize()
        {
            return Encoding.ASCII.GetBytes(this.ToString());
        }

        protected string GetBasicHeaders()
        {
            StringBuilder packet = new StringBuilder();
            packet.AppendFormat("{0}\r\n", Method.ToPacketString());
            Date = DateTime.UtcNow;
            packet.AppendFormat("Date: {0}\r\n", LibUtil.Util.FormatDate(Date));
            

            return packet.ToString();
        }

        public override string ToString()
        {
            StringBuilder packet = new StringBuilder();
            packet.Append(GetBasicHeaders());
            packet.Append(GetSpecificHeaders()); //Get any specific headers for different packet types.

            return packet.ToString();
        }


        protected abstract string GetSpecificHeaders();
        protected abstract void TryParseExtendedHeader(string name, string val);

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
                        case "NT": //Announce uses NT
                        case "ST": //Discovery/Reponse uses ST.
                            //We're actually more liberal here than we should be. The protocol spec dictates which packet types should use which header, but we don't care as much - so long as we get *something* it's cool.
                            if (headerval == ServiceType)
                                checkedServiceType = true;
                            break;
                        case "Date":
                            p.Date = DateTime.Parse(headerval);
                            break;
                            //If we couldn't figure out the header, maybe the implementing class can.
                        default:
                            p.TryParseExtendedHeader(headername, headerval);
                            break;
                    }
                }

                if (checkedServiceType != true)
                    return null;

                return p;
            }
            catch (Exception ex)
            {
                LibTrace.Trace.GetInstance("SSDPPacket").Warning("Failed to parse packet: " + ex.Message);
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
