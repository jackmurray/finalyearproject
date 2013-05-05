using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;

namespace LibSecurity
{
    public class Signer
    {
        public ISigner sig { get; private set; }

        private Signer(ISigner rsa)
        {
            sig = rsa;
        }

        public static Signer Create(KeyManager key)
        {
            RsaDigestSigner rsa = new RsaDigestSigner(new Sha256Digest());
            rsa.Init(true, key.Private);
            return new Signer(rsa);
        }

        public byte[] Sign(byte[] data)
        {
            sig.BlockUpdate(data, 0, data.Length);
            byte[] signature = sig.GenerateSignature();
            sig.Reset(); //So we can re-use this signer for multiple inputs.
            return signature;
        }

        public string SignBase64(byte[] data)
        {
            return LibUtil.Util.BytesToBase64String(Sign(data));
        }
    }
}
