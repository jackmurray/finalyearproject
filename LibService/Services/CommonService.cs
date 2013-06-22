using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class CommonService : IService
    {
        public const byte SERVICE_ID = 0x00;

        public const byte GET_VERSION = 0x00;

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
                    response = new ServiceMessage(SERVICE_ID, GET_VERSION, new byte[] {0x01});
                    break;
                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return response;
        }
    }
}
