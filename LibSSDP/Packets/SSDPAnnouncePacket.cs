using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibSecurity;

namespace LibSSDP
{
    public class SSDPAnnouncePacket : SSDPPacket
    {
        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("NTS: ssdp:alive\r\n");
            sb.AppendFormat("NT: {0}\r\n", ServiceType);

            return sb.ToString();
        }

        protected static string GetOurControlURL()
        {
            IPAddress ourip =
                Dns.GetHostEntry(Dns.GetHostName())
                   .AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            return String.Format("http://{0}:10451/Control.svc", ourip);
        }

        public static SSDPAnnouncePacket BuildAnnouncePacket(KeyManager key)
        {
            string fingerprint = key.GetFingerprint();
            SSDPAnnouncePacket packet = new SSDPAnnouncePacket()
            {
                Method = Method.Announce,
                fingerprint = fingerprint
            };
            packet.Signature = packet.GetSignature(key);
            return packet;
        }
    }
}
