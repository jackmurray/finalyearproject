using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LibSecurity
{
    public class SslClient
    {
        private X509Certificate2 _cert;
        private SslStream _stream;

        public SslClient(X509Certificate2 cert)
        {
            _cert = cert;
        }

        public void Connect(IPEndPoint ep)
        {
            var sock = new TcpClient();
            sock.Connect(ep);
            _stream = new SslStream(sock.GetStream(), false, Validate);
            _stream.AuthenticateAsClient("d3cb375c-b626-4b24-bc3f-f022b12b3f2f", new X509Certificate2Collection(_cert), System.Security.Authentication.SslProtocols.Tls, false);
        }

        private bool Validate(object sender, X509Certificate certificate, X509Chain chain,
                              SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public int GetVal()
        {
            return _stream.ReadByte();
        }
    }
}
