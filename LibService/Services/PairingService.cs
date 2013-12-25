using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LibConfig;
using LibSecurity;
using Newtonsoft.Json;

namespace LibService
{
    public class PairingService : ServiceBase
    {
        /// <summary>
        /// List of challenges that have been generated. Pairing requests are checked against this list to make sure the client
        /// replies to a valid challenge. We don't need to associate the challenges with a specific device because even if the challenge
        /// is leaked, an attacker would still need the pairing code and could just request their own challenge anyway.
        /// </summary>
        private static List<byte[]> GeneratedChallenges = new List<byte[]>();

        public PairingService()
        {
            Name = "PairingService";
            Operations = new List<string>() {"Pair", "GetPairingChallenge"};
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage m, X509Certificate remoteParty)
        {
            switch (m.operationID)
            {
                //Pair is intentionally non-verbose in it's errors returned to the client. This is to try and thwart attackers.
                //Full logging is still done on the service host side though, so legitimate users can try and debug.
                case "Pair":
                    bool valid;
                    var recv = JsonConvert.DeserializeObject<Tuple<byte[], byte[], byte[]>>(m.Data); //tuple<challenge, sig, challenge2>
                    byte[] expectedsig;
                    lock (GeneratedChallenges)
                    {
                        byte[] find = GeneratedChallenges.Find(a => a.SequenceEqual(recv.Item1));
                        if (find == null) //can't use .Contains() since arrays don't implement Equals().
                        {
                            LibTrace.Trace.GetInstance("LibService")
                                    .Error("Invalid challenge received in pairing request.");
                            return new ServiceMessageResponse("False", HttpResponseCode.ACCESS_DENIED);
                        }

                        var check = new ChallengeResponse(remoteParty.GetCertHashString(), recv.Item1); //verify that the remote party we're connected to is the one that signed the challenge
                        expectedsig = check.Sign(Config.Get(Config.PAIRING_KEY));
                        valid = expectedsig.SequenceEqual(recv.Item2);
                        GeneratedChallenges.Remove(find); //valid or not, we still remove the challenge.
                    }
                    if (valid)
                    {
                        var ourcert = CertManager.GetCert(KeyManager.GetKey());
                        TrustedKeys.Add(remoteParty); //we now trust the other end of the connection since they answered our challenge correctly.
                        byte[] response2 = new ChallengeResponse(ourcert.Fingerprint, recv.Item3).Sign(Config.Get(Config.PAIRING_KEY)); //compute r2 to send back
                        return new ServiceMessageResponse(JsonConvert.SerializeObject(response2), HttpResponseCode.OK);
                    }


                    //OK to log the expected signature, since it's no use anymore (challenge is invalidated).
                    LibTrace.Trace.GetInstance("LibService")
                            .Error("Incorrect signature in pairing request. Expected " +
                                    LibUtil.Util.BytesToHexString(expectedsig)
                                    + " got " + LibUtil.Util.BytesToHexString(recv.Item2));
                    return new ServiceMessageResponse("", HttpResponseCode.ACCESS_DENIED);
                    

                case "GetPairingChallenge":
                    lock (GeneratedChallenges)
                    {
                        ChallengeResponse cr = new ChallengeResponse(""); //empty fingerprint because we only want the random bytes out
                        GeneratedChallenges.Add(cr.ChallengeBytes);
                        return new ServiceMessageResponse(LibUtil.Util.BytesToHexString(cr.ChallengeBytes),
                                                          HttpResponseCode.OK);
                    }

                default:
                    throw new ArgumentException("Invalid message received.");
            }
        }
    }
}
