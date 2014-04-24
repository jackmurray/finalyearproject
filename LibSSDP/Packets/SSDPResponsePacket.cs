using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibSecurity;

namespace LibSSDP
{
    public class SSDPResponsePacket : SSDPSignedPacket
    {
        public SSDPResponsePacket()
        {
            Setup();
        }

        public SSDPResponsePacket(KeyManager key, CertManager cert)
            : base(key, cert)
        {
            Setup();
        }

        private void Setup()
        {
            Location = 10451;
            Method = LibSSDP.Method.Respond;
        }

        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetSpecificHeaders());
            sb.AppendFormat("ST: {0}\r\n", ServiceType);
            return sb.ToString();
        }

        public void Send(IPEndPoint ep)
        {

            byte[] data = this.Serialize();
            new UdpClient().Send(data, data.Length, ep);
        }
    }
}
