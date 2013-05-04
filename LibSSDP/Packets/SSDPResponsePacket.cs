using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibSecurity;

namespace LibSSDP
{
    public class SSDPResponsePacket : SSDPPacket
    {
        internal SSDPResponsePacket()
        {
            Location = LibUtil.Util.GetOurControlURL(false).ToString();
            Method = LibSSDP.Method.Respond;
        }

        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("ST: {0}\r\n", ServiceType);
            return sb.ToString();
        }

        internal static SSDPResponsePacket Build(KeyManager key)
        {
            string fingerprint = key.GetFingerprint();
            SSDPResponsePacket packet = new SSDPResponsePacket()
            {
                    fingerprint = fingerprint
            };
            packet.Signature = packet.GetSignature(key);
            return packet;
        }

        public void Send(IPEndPoint ep)
        {

            byte[] data = this.Serialize();
            new UdpClient().Send(data, data.Length, ep);
        }
    }
}
