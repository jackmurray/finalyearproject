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
    public class SslServer
    {
        private X509Certificate2 _cert;

        public SslServer(X509Certificate2 cert)
        {
            _cert = cert;
        }

        public void Listen(int port)
        {
            var sock = new TcpListener(IPAddress.Any, port);
            sock.Start();
            SslStream ssl = new SslStream(sock.AcceptTcpClient().GetStream(), false, (a, b, c, d) => true);
            ssl.AuthenticateAsServer(_cert, true, System.Security.Authentication.SslProtocols.Tls, false);
            Console.WriteLine("Accepted SSL connection.");
            ssl.WriteByte(0xff);
            ssl.Flush();
            ssl.Close();
        }
    }
}
