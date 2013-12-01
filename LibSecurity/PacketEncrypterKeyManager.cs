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
        }

        public byte[] GenerateNewKey()
        {
            Key = CryptRandom.GetBytes(KEY_LENGTH);
            return Key;
        }

        public byte[] GenerateNewNonce()
        {
            Nonce = CryptRandom.GetBytes(NONCE_LENGTH);
            return Nonce;
        }
    }
}
