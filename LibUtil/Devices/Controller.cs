using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibUtil
{
    public class Controller : Device
    {
        public Controller(IPEndPoint ep, X509Certificate cert) : base(ep, cert)
        {
        }

        public Controller(IPEndPoint ep, string Fingerprint) : base(ep, Fingerprint)
        {
        }

        public override string ToString()
        {
            return "Controller " + Address + " " + Fingerprint;
        }
    }
}
