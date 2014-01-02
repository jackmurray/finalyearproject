using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LibCommon
{
    public class ControllerState
    {
        public StreamMode Mode { get; set; }
        public bool EncryptionEnabled { get; set; }
        public bool AuthenticationEnabled { get; set; }
    }

    public enum StreamMode
    {
        File,
        Loopback
    }
}
