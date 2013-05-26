using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeakerController
{
    class ControllerService : IControllerService
    {
        public int GetVersion()
        {
            return 1;
        }

        public byte[] GetCurrentDataKey()
        {
            return new byte[] {0xDE, 0xAD, 0xBE, 0xEF};
        }
    }
}
