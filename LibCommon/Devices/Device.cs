using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LibSecurity;
using LibService;

namespace LibCommon
{
    public abstract class Device
    {
        public IPEndPoint Address { get; protected set; }
        public X509Certificate Cert { get; protected set; }

        private readonly string _fingerprint;
        public string Fingerprint { get { return Cert != null ? Cert.GetCertHashString() : _fingerprint; } }

        protected Device(IPEndPoint ep, X509Certificate cert)
        {
            this.Address = ep;
            this.Cert = cert;
        }

        protected Device(IPEndPoint ep, string Fingerprint)
        {
            this.Address = ep;
            this._fingerprint = Fingerprint;
        }

        public override string ToString()
        {
            return "Device " + Address + " " + Fingerprint;
        }

        public SslClient GetSsl(CertManager cert, KeyManager key)
        {
            SslClient c = new SslClient(cert.ToDotNetCert(key));
            c.Connect(Address);
            if (!c.ValidateRemoteFingerprint(Fingerprint))
                throw new Exception("Remote certificate failed validation!");

            return c;
        }
    }
}
