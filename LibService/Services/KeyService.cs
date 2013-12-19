using System;
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
        private ClientValidationCallback isClientValid;

        public delegate bool ClientValidationCallback(X509Certificate client);

        public KeyService(PacketEncrypterKeyManager pekm, ClientValidationCallback cvc)
        {
            Name = "KeyService";
            Operations = new List<string>() { "GetNextKey" };
            this.pekm = pekm;
            this.isClientValid = cvc;
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage m, X509Certificate remoteParty)
        {
            switch (m.operationID)
            {
                case "GetNextKey":
                    if (pekm.NextKey == null || pekm.NextNonce == null)
                        return Error("Unable to provide new key/nonce - they have not been set yet.");
                    if (!isClientValid(remoteParty))
                        return new ServiceMessageResponse("No longer authorized.", HttpResponseCode.ACCESS_DENIED);
                    else return Success(JsonConvert.SerializeObject(new Tuple<byte[], byte[]>(pekm.NextKey, pekm.NextNonce)));
                default:
                    throw new ArgumentException("Invalid message received.");
            }
        }
    }
}
