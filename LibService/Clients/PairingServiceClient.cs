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
        
        public bool Pair(byte[] challenge, byte[] sig)
        {
            try
            {
                var data = new Tuple<byte[], byte[]>(challenge, sig);
                ServiceMessage m = new ServiceMessage("PairingService", "Pair", JsonConvert.SerializeObject(data));
                ServiceMessageResponse response = Call(m);
                return bool.Parse(response.Data);
            }
            catch (ServiceException ex)
            {
                //If we get an access denied, it means that pairing failed. Call() will generate an exception because the code is != 200, so we convert
                //that back into a false return value for our caller.
                if (ex.Code == HttpResponseCode.ACCESS_DENIED)
                    return false;
                else throw;
            }
        }
    }
}
