using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using LibConfig;
using LibSecurity;
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
                ChallengeResponse cr = new ChallengeResponse(remoteParty.GetCertHashString()); //the signature will have the remote fingerprint in it
                var data = new Tuple<byte[], byte[], byte[]>(challenge, sig, cr.ChallengeBytes);
                ServiceMessage m = new ServiceMessage("PairingService", "Pair", JsonConvert.SerializeObject(data));
                ServiceMessageResponse response = Call(m);
                bool correct =  cr.Verify(Config.Get(Config.PAIRING_KEY), JsonConvert.DeserializeObject<byte[]>(response.Data));
                if (correct)
                {
                    TrustedKeys.Add(_s.RemoteCertificate);
                    return true;
                }
                return false;
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
