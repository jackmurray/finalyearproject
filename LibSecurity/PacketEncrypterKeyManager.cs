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

        private byte[] nextKey, nextNonce;

        public const int KEY_LENGTH = 16;
        public const int NONCE_LENGTH = 8; //hardcoded to AES128 values.

        private LibTrace.Trace Log = LibTrace.Trace.GetInstance("LibSecurity");

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

            Log.Verbose("PEKM: Creating new instance with specified key/nonce.");
        }

        public byte[] GenerateNewKey()
        {
            Key = CryptRandom.GetBytes(KEY_LENGTH);
            Log.Verbose("PEKM: Generated new key");
            return Key;
        }

        public byte[] GenerateNewNonce()
        {
            Nonce = CryptRandom.GetBytes(NONCE_LENGTH);
            Log.Verbose("PEKM: Generated new nonce");
            return Nonce;
        }

        public void SetNextKey(byte[] key, byte[] nonce)
        {
            this.nextKey = key;
            this.nextNonce = nonce;
            Log.Verbose("PEKM: setting next key");
        }

        public void UseNextKey()
        {
            if (nextKey == null || nextNonce == null)
            {
                Log.Error("PEKM: Was asked to switch to next key but one has not been set!");
                return;
            }
            this.Key = nextKey;
            this.Nonce = nextNonce;
            Log.Verbose("PEKM: switching to next key");
            nextKey = null;
            nextNonce = null;
        }
    }
}
