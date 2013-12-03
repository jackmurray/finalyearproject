using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using Newtonsoft.Json;

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
    }
}
