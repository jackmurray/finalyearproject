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
using LibService;

namespace LibSecurity
{
    public class SslServer : SslEndpointBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cert"></param>
        public SslServer(X509Certificate2 cert) : base(cert)
        {
            
        }

        /// <summary>
        /// Fire up the main listening thread. This call returns after starting it.
        /// </summary>
        /// <param name="port"></param>
        public void Listen(int port)
        {
            var l = new TcpListener(IPAddress.Any, port);
            var t = new Thread(() => ConnectionListener(l)) {Name = "SslServerMainListener"};
            t.Start();
        }

        /// <summary>
        /// Wait for incoming connections, and fire off a new thread to deal with each one that's received.
        /// </summary>
        /// <param name="l"></param>
        private void ConnectionListener(TcpListener l)
        {
            l.Start();
            while (true)
            {
                TcpClient c = l.AcceptTcpClient();
                var t = new Thread(() => ConnectionHandler(c)) {Name = "SslServerConnectionHandler"};
                t.Start();
            }
        }

        /// <summary>
        /// Handle a connection.
        /// </summary>
        /// <param name="c"></param>
        private void ConnectionHandler(TcpClient c)
        {
            try
            {
                SslStream ssl = new SslStream(c.GetStream(), false, Util.ValidateClientCert);
                ssl.AuthenticateAsServer(_cert, true, System.Security.Authentication.SslProtocols.Tls, false);
                LibTrace.Trace.GetInstance("LibSecurity").Information("Accepted SSL connection.");
                ServiceHandler handler = new ServiceHandler(ssl);
                int numHandled = 0;
                while (true)
                {
                    int ret = handler.HandleMessage();
                    if (ret == -1) //socket was closed. we're done here.
                    {
                        LibTrace.Trace.GetInstance("LibSecurity")
                                .Information("SslServer shutting down. Handled " + numHandled + " messages.");
                        return;
                    }
                    numHandled++;
                }
            }
            catch (Exception ex)
            {
                LibTrace.Trace.GetInstance("LibSecurity").Error("Exception in SslServer: " + ex.Message);
                c.Close();
                return;
            }
        }
    }
}
