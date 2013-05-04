using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeakerReceiver
{
    class ReceiverService : IReceiverService
    {
        public int GetVersion()
        {
            return 1;
        }
    }
}
