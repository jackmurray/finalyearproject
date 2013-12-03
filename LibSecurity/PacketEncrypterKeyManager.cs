using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSecurity
{
    public class PacketEncrypterKeyManager
    {
        public byte[] Key { get; protected set; }
        public byte[] Nonce { get; protected set; }

        public const int KEY_LENGTH = 16;
        public const int NONCE_LENGTH = 8; //hardcoded to AES128 values.

        public PacketEncrypterKeyManager()
        {
            LibTrace.Trace.GetInstance("LibSecurity").Verbose("PEKM: Creating new instance - generation message should follow.");
            GenerateNewKey();
            GenerateNewNonce();
        }

        public PacketEncrypterKeyManager(byte[] key, byte[] nonce)
        {
            if (key.Length != PacketEncrypterKeyManager.KEY_LENGTH)
                throw new ArgumentException("Key must be " + PacketEncrypterKeyManager.KEY_LENGTH + " bytes.");

            if (nonce.Length != PacketEncrypterKeyManager.NONCE_LENGTH)
                throw new ArgumentException("Nonce must be " + PacketEncrypterKeyManager.NONCE_LENGTH + " bytes");

            this.Key = key;
            this.Nonce = nonce;

            LibTrace.Trace.GetInstance("LibSecurity").Verbose("PEKM: Creating new instance with specified key/nonce.");
        }

        public byte[] GenerateNewKey()
        {
            Key = CryptRandom.GetBytes(KEY_LENGTH);
            LibTrace.Trace.GetInstance("LibSecurity").Verbose("PEKM: Generated new key");
            return Key;
        }

        public byte[] GenerateNewNonce()
        {
            Nonce = CryptRandom.GetBytes(NONCE_LENGTH);
            LibTrace.Trace.GetInstance("LibSecurity").Verbose("PEKM: Generated new nonce");
            return Nonce;
        }
    }
}
