using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using LibSecurity;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;

namespace LibService
{
    public class TransportServiceClient : ServiceClient
    {
        public TransportServiceClient(SslStream s)
            : base(s)
        {
        }

        public void JoinGroup(string addr)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(addr, out ip))
                throw new ArgumentException("Failed to parse IP.");
            if (!LibUtil.Util.IsMulticastAddress(ip))
                throw new ArgumentException("Not a valid multicast IP.");

            ServiceMessage m = new ServiceMessage("TransportService", "JoinGroup", addr); //send the string, since IPAddress objects don't like being serialized.
            Call(m);
        }

        public void SetEncryptionKey(byte[] key, byte[] nonce)
        {
            var t = new Tuple<byte[], byte[]>(key, nonce);
            ServiceMessage m = new ServiceMessage("TransportService", "SetEncryptionKey", JsonConvert.SerializeObject(t));
            Call(m);
        }

        public void SetControllerAddress(IPEndPoint ep)
        {
            var t = new Tuple<string, ushort>(ep.Address.ToString(), (ushort) ep.Port);
            ServiceMessage m = new ServiceMessage("TransportService", "SetControllerAddress", JsonConvert.SerializeObject(t));
            Call(m);
        }

        public void SetSigningKey(KeyManager key)
        {
            //We have to split the key up into modulus and exponent because a BigInteger won't serialise properly automatically.
            string n = (key.Public as RsaKeyParameters).Modulus.ToString();
            string e = (key.Public as RsaKeyParameters).Exponent.ToString();

            Tuple<string, string> pubkey = new Tuple<string, string>(n, e);
            Call(new ServiceMessage("TransportService", "SetSigningKey", JsonConvert.SerializeObject(pubkey)));
        }
    }
}
