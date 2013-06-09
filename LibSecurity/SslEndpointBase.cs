using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibSecurity
{
    public abstract class SslEndpointBase
    {
        protected X509Certificate2 _cert;

        protected SslEndpointBase(X509Certificate2 cert)
        {
            _cert = cert;
        }
    }
}
