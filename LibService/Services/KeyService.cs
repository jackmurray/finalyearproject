﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LibSecurity;
using Newtonsoft.Json;

namespace LibService
{
    public class KeyService : ServiceBase
    {
        private PacketEncrypterKeyManager pekm;

        public KeyService(PacketEncrypterKeyManager pekm)
        {
            Name = "KeyService";
            Operations = new List<string>() { "GetNextKey" };
            this.pekm = pekm;
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage m, X509Certificate remoteParty)
        {
            switch (m.operationID)
            {
                case "GetNextKey":
                    //TODO: Check if calling client is still authorized for the stream.
                    if (pekm.NextKey == null || pekm.NextNonce == null)
                        return Error("Unable to provide new key/nonce - they have not been set yet.");
                    return Success(JsonConvert.SerializeObject(new Tuple<byte[], byte[]>(pekm.NextKey, pekm.NextNonce)));
                    break;
                default:
                    throw new ArgumentException("Invalid message received.");
            }
        }
    }
}
