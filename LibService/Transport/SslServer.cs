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
using LibTrace;

namespace LibService
{
    public class SslServer : SslEndpointBase
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private TcpListener listener;
        private Trace Log = LibTrace.Trace.GetInstance("LibService.Transport");

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
            lock (cts)
            {
                listener = new TcpListener(IPAddress.Any, port);
                var t = new Thread(() => ConnectionListener(listener, cts.Token)) {Name = "SslServerMainListener"};
                t.Start();
            }
        }

        public void Stop()
        {
            lock (cts)
            {
                cts.Cancel();
                listener.Stop();
            }
        }

        /// <summary>
        /// Wait for incoming connections, and fire off a new thread to deal with each one that's received.
        /// </summary>
        /// <param name="l"></param>
        private void ConnectionListener(TcpListener l, CancellationToken token)
        {
            l.Start();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient c = l.AcceptTcpClient();
                    var t = new Thread(() => ConnectionHandler(c)) {Name = "SslServerConnectionHandler"};
                    t.Start();
                }
            }
            catch (SocketException ex)
            {
                //We can't log that the code *was* Interrupted and we're shutting down because at this point the controller GUI
                //will have been destroyed causing the log call to fail (because it can't call BeginInvoke() on the log box).
                if (ex.SocketErrorCode != SocketError.Interrupted)
                    Log.Critical(ex.Message);
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
                SslStream ssl = new SslStream(c.GetStream(), false, ValidateClientCert);
                ssl.AuthenticateAsServer(_cert, true, System.Security.Authentication.SslProtocols.Tls, false);
                Log.Information("Accepted SSL connection.");
                ServiceHandler handler = new ServiceHandler(ssl);
                int numHandled = 0;
                while (true)
                {
                    int ret = handler.HandleMessage();
                    if (ret == -1) //socket was closed. we're done here.
                    {
                        Log.Information("SslServer shutting down. Handled " + numHandled + " messages.");
                        return;
                    }
                    numHandled++;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in SslServer: " + ex.Message);
                c.Close();
                return;
            }
        }
    }
}
