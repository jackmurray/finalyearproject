using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibSecurity
{
    public static class TrustManager
    {
        internal static bool ValidateAsServer(object sender, X509Certificate certificate, X509Chain chain,
                              SslPolicyErrors sslPolicyErrors)
        {
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors; //we don't care about cert chain errors sonce we don't have a proper PKI structure.
            //if (sslPolicyErrors != SslPolicyErrors.None)
                //return false;
            Console.WriteLine("Remote cert: {0}", certificate == null ? "null" : certificate.Subject);
            return true;
        }

        internal static bool ValidateAsClient(object sender, X509Certificate certificate, X509Chain chain,
                              SslPolicyErrors sslPolicyErrors)
        {
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors; //we don't care about cert chain errors sonce we don't have a proper PKI structure.
            //if (sslPolicyErrors != SslPolicyErrors.None)
            //return false;
            Console.WriteLine("Remote cert: {0}", certificate == null ? "null" : certificate.Subject);
            return true;
        }
    }
}
