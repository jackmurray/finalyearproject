using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibSecurity;

namespace LibSSDP
{
    public class SSDPAnnouncePacket : SSDPSignedPacket
    {
        internal SSDPAnnouncePacket()
        {
            Setup();
        }

        internal SSDPAnnouncePacket(KeyManager key, CertManager cert) : base(key, cert)
        {
            Setup();
        }

        private void Setup()
        {
            Location = 10451;
            Method = LibSSDP.Method.Announce;
        }

        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetSpecificHeaders());
            sb.Append("NTS: ssdp:alive\r\n");
            sb.AppendFormat("NT: {0}\r\n", ServiceType);

            return sb.ToString();
        }
    }
}
