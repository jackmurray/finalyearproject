using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTrace
{
    public interface ITraceReceiver
    {
        void ReceiveTrace(string message);
    }
}
