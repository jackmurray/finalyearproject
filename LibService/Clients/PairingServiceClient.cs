using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using Newtonsoft.Json;

namespace LibService
{
    public class PairingServiceClient : ServiceClient
    {
        public PairingServiceClient(SslStream s)
            : base(s)
        {
        }

        public byte[] GetChallengeBytes()
        {
            ServiceMessage m = new ServiceMessage("PairingService", "GetPairingChallenge", null);
            ServiceMessageResponse response = Call(m);
            return LibUtil.Util.HexStringToBytes(response.Data);
        }
    }
}
