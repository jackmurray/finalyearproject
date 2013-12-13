using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibUtil
{
    public class Receiver : Device
    {
        public Receiver(IPEndPoint ep, X509Certificate cert) : base(ep, cert)
        {
        }

        public Receiver(IPEndPoint ep, string Fingerprint) : base(ep, Fingerprint)
        {
        }

        public override string ToString()
        {
            return "Receiver " + Address + " " + Fingerprint;
        }
    }
}
