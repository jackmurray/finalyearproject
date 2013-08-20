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
        public const byte SERVICE_ID = 0x00;
        private string _name = "CommonService";

        public const byte GET_VERSION = 0x00;

        private Version _version;

        public CommonService(Version version)
        {
            _version = version;
        }

        public bool CanHandleMessage(ServiceMessage message)
        {
            if (message.serviceID != SERVICE_ID)
                return false;
            if (message.operationID != GET_VERSION)
                return false;

            return true;
        }

        public ServiceMessage HandleMessage(ServiceMessage message)
        {
            ServiceMessage response = null;

            switch (message.operationID)
            {
                case GET_VERSION:
                    string encodedVersion = JsonConvert.SerializeObject(_version);

                    response = new ServiceMessage(SERVICE_ID, GET_VERSION, encodedVersion);
                    break;
                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return response;
        }

        public string Name {
            get { return _name; }
        }
    }
}
