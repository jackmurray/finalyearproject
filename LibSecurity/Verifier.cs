using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.X509;
using X509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;

namespace LibSecurity
{
    public class Verifier
    {
        protected ISigner _signer;

        protected Verifier()
        {
            _signer = new RsaDigestSigner(new Sha256Digest());
        }

        public Verifier(X509Certificate cert) : this()
        {
            _signer.Init(false, new X509CertificateParser().ReadCertificate(cert.GetRawCertData()).GetPublicKey());
        }

        public Verifier(AsymmetricKeyParameter kp) : this()
        {
            _signer.Init(false, kp);
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            //LibTrace.Trace.GetInstance("LibSecurity").Verbose("Verifying data: " + LibUtil.Util.BytesToHexString(data));
            _signer.Reset();
            _signer.BlockUpdate(data, 0, data.Length);
            return _signer.VerifySignature(signature);
        }
    }
}
