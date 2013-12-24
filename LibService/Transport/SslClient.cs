using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LibService
{
    public class SslClient : SslEndpointBase
    {
        private SslStream _stream;

        public SslClient(X509Certificate2 cert) : base(cert)
        {
            
        }

        public void Connect(IPEndPoint ep)
        {
            /*
             * This retry loop is to fix an issue with (I think) .NET SSL. If you connect to a receiver running on the local machine
             * and then try to connect to an RPi, the first attempt sometimes fails with some unhelpful SSPI exception. Opening a new socket
             * and new SSL connection seems to make it work.
             * */
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var sock = new TcpClient();
                    sock.Connect(ep);
                    _stream = new SslStream(sock.GetStream(), false, ValidateServerCert, SelectLocalCert);
                    X509CertificateCollection cc = new X509CertificateCollection();
                    cc.Add(_cert);
                    _stream.AuthenticateAsClient("_", cc, System.Security.Authentication.SslProtocols.Tls, false);
                    break;
                }
                catch (Exception ex)
                {
                    LibTrace.Trace.GetInstance("LibService.Transport")
                            .Warning("Failed to open SSL connection: " + ex.Message + ". Retrying...");
                    if (i == 4) //if this is the last retry then pass the exception up since things are not going well...
                        throw;
                }
            }
        }

        public X509Certificate GetRemoteCert()
        {
            return _stream.RemoteCertificate;
        }

        public void Close()
        {
            _stream.Close();
        }

        public T GetClient<T>() where T : ServiceClient
        {
            //not sure if this is the best method. maybe something better should be done in future.
            return Activator.CreateInstance(typeof (T), _stream) as T;
        }

        public bool ValidateRemoteFingerprint(string expected)
        {
            return string.Compare(GetRemoteCert().GetCertHashString(), expected, true) == 0;
        }
    }
}
