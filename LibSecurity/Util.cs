using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LibSecurity
{
    internal static class Util
    {
        internal static bool ValidateClientCert(object sender, X509Certificate certificate, X509Chain chain,
                              SslPolicyErrors sslPolicyErrors)
        {
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors; //we don't care about cert chain errors sonce we don't have a proper PKI structure.
            //if (sslPolicyErrors != SslPolicyErrors.None)
                //return false;
            LibTrace.Trace Log = LibTrace.Trace.GetInstance("LibSecurity");
            Log.Information(String.Format("Client cert: {0}", certificate == null ? "null" : certificate.Subject));
            return true;
        }

        internal static bool ValidateServerCert(object sender, X509Certificate certificate, X509Chain chain,
                              SslPolicyErrors sslPolicyErrors)
        {
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors; //we don't care about cert chain errors sonce we don't have a proper PKI structure.
            //if (sslPolicyErrors != SslPolicyErrors.None)
            //return false;
            LibTrace.Trace Log = LibTrace.Trace.GetInstance("LibSecurity");
            Log.Information(String.Format("Server cert: {0}", certificate == null ? "null" : certificate.Subject));
            return true;
        }
    }
}
