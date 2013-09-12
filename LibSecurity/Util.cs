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
            return ValidateGeneric(sender, certificate, chain, sslPolicyErrors, false);
        }

        internal static bool ValidateServerCert(object sender, X509Certificate certificate, X509Chain chain,
                              SslPolicyErrors sslPolicyErrors)
        {
            return ValidateGeneric(sender, certificate, chain, sslPolicyErrors, true);
        }

        internal static X509Certificate SelectLocalCert(object sender, string targetHost,
                                                        X509CertificateCollection localCertificates,
                                                        X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return localCertificates[0];
        }

        private static bool ValidateGeneric(object sender, X509Certificate certificate, X509Chain chain,
                                            SslPolicyErrors sslPolicyErrors, bool isServer)
        {
            LibTrace.Trace Log = LibTrace.Trace.GetInstance("LibSecurity");
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors; //we don't care about cert chain errors sonce we don't have a proper PKI structure.
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNameMismatch; //the name on our certs is just a GUID which we don't care about. all of our security comes from checking the
                                                                               //public key on the cert.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                Log.Error((isServer ? "Server" : "Client") + " cert validation FAILED. SslPolicyErrors was : " + sslPolicyErrors);
                return false;
            }
            if (certificate == null)
            {
                Log.Error((isServer ? "Server" : "Client") + " cert validation FAILED. Cert was null.");
                return false;
            }

            Log.Information((isServer ? "Server" : "Client") + " cert: " + certificate.Subject);
            return true;
        }
    }
}
