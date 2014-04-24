using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibConfig;
using LibSecurity;
using LibService;

namespace UnitTest
{
    class TestUtil
    {
        public static SslServer GetSSLServer()
        {
            var key = GetKey();
            var cert = GetCert(key);
            TrustedKeys.Add(cert.ToDotNetPublicCert()); //make sure we trust ourselves
            SslServer s = new SslServer(cert.ToDotNetCert(key));
            s.Listen(10451);

            return s;
        }

        public static CertManager GetCert(KeyManager key)
        {
            var cert = global::LibSecurity.CertManager.GetCert(key);
            return cert;
        }

        public static SslClient GetSSLClient()
        {
            var key = GetKey();
            var cert = GetCert(key);
            SslClient c = new SslClient(cert.ToDotNetCert(key));
            c.Connect(new System.Net.IPEndPoint(IPAddress.Loopback, 10451));

            return c;
        }

        public static KeyManager GetKey()
        {
            var key = KeyManager.GetKey();
            return key;
        }
    }
}
