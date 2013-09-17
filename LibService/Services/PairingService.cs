using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSecurity;

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

        public override ServiceMessageResponse HandleMessage(ServiceMessage m)
        {
            ServiceMessageResponse resp = null;

            switch (m.operationID)
            {
                case "Pair":
                    break;

                case "GetPairingChallenge":
                    ChallengeResponse cr = new ChallengeResponse();
                    GeneratedChallenges.Add(cr.ChallengeBytes);
                    resp = new ServiceMessageResponse(LibUtil.Util.BytesToHexString(cr.ChallengeBytes),
                                                      HttpResponseCode.OK);
                    break;

                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return resp;
        }
    }
}
