using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibService
{
    public interface IService
    {
        bool CanHandleMessage(byte[] message);
        byte[] HandleMessage(byte[] message);
    }
}
