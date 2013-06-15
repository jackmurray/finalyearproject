using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public class CommonService : IService
    {
        public bool CanHandleMessage(byte[] message)
        {
            if (message[0] == 0x00)
                return true;

            return false;
        }

        public byte[] HandleMessage(byte[] message)
        {
            return new byte[] {0x01};
        }
    }
}
