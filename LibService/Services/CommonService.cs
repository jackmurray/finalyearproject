using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil;
using Newtonsoft.Json;

namespace LibService
{
    public class CommonService : IService
    {
        public static string Name = "CommonService";
        private List<string> Operations = new List<string>() {"GetVersion"};
        private Version _version;

        public CommonService(Version version)
        {
            _version = version;
        }

        public bool CanHandleMessage(ServiceMessage message)
        {
            if (message.serviceID != CommonService.Name)
                return false;
            if (!Operations.Contains(message.operationID))
                return false;

            return true;
        }

        public ServiceMessage HandleMessage(ServiceMessage message)
        {
            ServiceMessage response = null;

            switch (message.operationID)
            {
                case "GetVersion":
                    string encodedVersion = JsonConvert.SerializeObject(_version);

                    response = new ServiceMessage(CommonService.Name, "GetVersion", encodedVersion);
                    break;
                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return response;
        }
    }
}
