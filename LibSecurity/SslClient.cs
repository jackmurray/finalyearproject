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

        public SslClient(X509Certificate2 cert)
        {
            _cert = cert;
        }

        public int Connect(int port)
        {
            var sock = new TcpClient();
            sock.Connect(new IPEndPoint(IPAddress.Parse("10.0.1.6"), port));
            SslStream ssl = new SslStream(sock.GetStream(), false, (a,b,c,d) => true);
            ssl.AuthenticateAsClient("d3cb375c-b626-4b24-bc3f-f022b12b3f2f", new X509Certificate2Collection(_cert), System.Security.Authentication.SslProtocols.Tls, false);
            int read = ssl.ReadByte();
            ssl.Close();
            return read;
        }
    }
}
