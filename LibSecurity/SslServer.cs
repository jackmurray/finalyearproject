using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LibSecurity
{
    public class SslServer
    {
        private X509Certificate2 _cert;

        /// <summary>
        /// This class should be instantiated on a new thread. It will block to accept tcp connections.
        /// When it receives a connection, it fires off another thread to deal with it.
        /// </summary>
        /// <param name="cert"></param>
        public SslServer(X509Certificate2 cert)
        {
            _cert = cert;
        }

        public void Listen(int port)
        {
            var sock = new TcpListener(IPAddress.Any, port);
            sock.Start();
            while (true)
            {
                TcpClient c = sock.AcceptTcpClient();
                new Thread(() => ConnectionHandler(c)).Start();
            }
        }

        private void ConnectionHandler(TcpClient c)
        {
            SslStream ssl = new SslStream(c.GetStream(), false, Util.ValidateClientCert);
            ssl.AuthenticateAsServer(_cert, true, System.Security.Authentication.SslProtocols.Tls, false);
            Console.WriteLine("Accepted SSL connection.");
            ssl.WriteByte(0xff);
            ssl.Flush();
            ssl.Close();
        }
    }
}
