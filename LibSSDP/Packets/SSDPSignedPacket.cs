using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSecurity;
using LibConfig;

namespace LibSSDP
{
    public abstract class SSDPSignedPacket : SSDPPacket
    {
        private KeyManager key;
        public string fingerprint, signature, friendlyName;
        public int Location;

        /// <summary>
        /// So we can construct objects when parsing packets.
        /// </summary>
        protected SSDPSignedPacket()
        {
            
        }

        protected SSDPSignedPacket(KeyManager key, CertManager cert)
        {
            this.key = key;
            fingerprint = cert.Fingerprint;
            friendlyName = Config.Get(Config.DEVICE_FRIENDLY_NAME);
        }

        protected string GetSignature(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("USN: fingerprint:{0}\r\n", fingerprint);
            sb.AppendFormat("x-signature: {0}", Signer.Create(key).SignASCIIStringToBase64(s));
            return sb.ToString();
        }

        public sealed override string ToString()
        {
            StringBuilder packet = new StringBuilder();
            packet.Append(base.ToString());
            string packetSoFar = packet.ToString();
            packet.Append(GetSignature(packetSoFar));
            return packet.ToString();
        }

        protected override void TryParseExtendedHeader(string name, string val)
        {
            switch (name)
            {
                case "USN":
                    fingerprint = val.Replace("fingerprint:", "");
                    break;
                case "x-signature":
                    signature = val;
                    break;
                case "Location":
                    Location = int.Parse(val);
                    break;
                case "x-device-name":
                    friendlyName = val;
                    break;
            }
        }

        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Location: {0}\r\n", Location);
            sb.AppendFormat("x-device-name: {0}\r\n", friendlyName);
            return sb.ToString();
        }
    }
}
