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
            Location = GetOurControlURL();
            Method = LibSSDP.Method.Respond;
        }

        protected override string GetSpecificHeaders()
        {
            return "";
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
