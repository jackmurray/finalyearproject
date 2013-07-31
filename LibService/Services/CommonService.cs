using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil;

namespace LibService
{
    public class CommonService : IService
    {
        public const byte SERVICE_ID = 0x00;

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
                    byte[] major = Util.Encode(_version.Major);
                    byte[] minor = Util.Encode(_version.Minor);
                    byte[] build = Util.Encode(_version.Build);
                    byte[] revision = Util.Encode(_version.Revision);
                    byte[] encodedVersion = new byte[4*4];
                    major.CopyTo(encodedVersion, 0);
                    minor.CopyTo(encodedVersion, 4);
                    build.CopyTo(encodedVersion, 8);
                    revision.CopyTo(encodedVersion, 12);

                    response = new ServiceMessage(SERVICE_ID, GET_VERSION, encodedVersion);
                    break;
                default:
                    throw new ArgumentException("Invalid message received.");
            }

            return response;
        }
    }
}
