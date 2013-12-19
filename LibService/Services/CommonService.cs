using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LibConfig;
using LibUtil;
using Newtonsoft.Json;

namespace LibService
{
    public class CommonService : ServiceBase
    {
        public CommonService()
        {
            Name = "CommonService";
            Operations = new List<string>() { "GetVersions", "GetConfigState" };
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage message, X509Certificate remoteParty)
        {
            ServiceMessageResponse response = null;

            switch (message.operationID)
            {
                case "GetVersions":
                    string encodedData = JsonConvert.SerializeObject(Util.GetComponentVersions());

                    response = new ServiceMessageResponse(encodedData, HttpResponseCode.OK);
                    break;

                case "GetConfigState":
                    return Success(JsonConvert.SerializeObject(ConfigState.GetConfigState()));
                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return response;
        }
    }
}
