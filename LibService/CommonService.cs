using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class CommonService : IService
    {
        public bool CanHandleMessage(ServiceMessage message)
        {
            if (message.Data[0] == 0x00)
                return true;

            return false;
        }

        public ServiceMessage HandleMessage(ServiceMessage message)
        {
            return new ServiceMessage(new byte[] {0x01});
        }
    }
}
