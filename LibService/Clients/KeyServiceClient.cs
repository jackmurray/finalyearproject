using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using Newtonsoft.Json;

namespace LibService
{
    public class KeyServiceClient : ServiceClient
    {
        public KeyServiceClient(SslStream s) : base(s)
        {
        }

        public Tuple<byte[], byte[]> GetNextKey()
        {
            ServiceMessage m = new ServiceMessage("KeyService", "GetNextKey", null);
            ServiceMessageResponse response = Call(m);
            return JsonConvert.DeserializeObject<Tuple<byte[], byte[]>>(response.Data);
        }
    }
}
