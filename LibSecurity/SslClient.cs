﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.Net.Sockets;
using LibService;

namespace LibSecurity
{
    public class SslClient : SslEndpointBase
    {
        private SslStream _stream;

        public SslClient(X509Certificate2 cert) : base(cert)
        {
            
        }

        public void Connect(IPEndPoint ep)
        {
            var sock = new TcpClient();
            sock.Connect(ep);
            _stream = new SslStream(sock.GetStream(), false, Util.ValidateServerCert, Util.SelectLocalCert);
            X509CertificateCollection cc = new X509CertificateCollection();
            cc.Add(_cert);
            _stream.AuthenticateAsClient("d3cb375c-b626-4b24-bc3f-f022b12b3f2f", cc, System.Security.Authentication.SslProtocols.Tls, false);
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
    }
}
