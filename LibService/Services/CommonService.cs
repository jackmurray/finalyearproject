using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LibUtil;
using Newtonsoft.Json;

namespace LibService
{
    public class CommonService : ServiceBase
    {
        private Version _version;

        public CommonService(Version version)
        {
            Name = "CommonService";
            Operations = new List<string>() { "GetVersion" };
            _version = version;
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage message, X509Certificate remoteParty)
        {
            ServiceMessageResponse response = null;

            switch (message.operationID)
            {
                case "GetVersion":
                    string encodedVersion = JsonConvert.SerializeObject(_version);

                    response = new ServiceMessageResponse(encodedVersion, HttpResponseCode.OK);
                    break;
                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return response;
        }
    }
}
