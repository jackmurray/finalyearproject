﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using LibTrace;
using LibConfig;
using LibUtil;

namespace LibSecurity
{
    public class KeyManager
    {
        private const int KEY_LENGTH = 2048;
        private static Trace Log = new Trace("LibSecurity");
        private RSACryptoServiceProvider rsa;

        private KeyManager(RSACryptoServiceProvider rsa)
        {
            this.rsa = rsa;
            rsa.PersistKeyInCsp = false;
        }

        public static KeyManager Create()
        {
            Log.Verbose("Creating new RSA key...");
            var rsa = new RSACryptoServiceProvider(KEY_LENGTH);
            Log.Verbose("Generated new RSA key.");
            return new KeyManager(rsa);
        }

        public void WriteKeyToFile(string filename)
        {
            string xml = rsa.ToXmlString(true);
            System.IO.File.WriteAllText(filename, xml);
            Log.Verbose("Wrote key to file " + filename);
        }

        public static KeyManager LoadKeyFromFile(string filename)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(System.IO.File.ReadAllText(filename));
            Log.Verbose("Loaded rsa key from file " + filename);
            return new KeyManager(rsa);
        }

        /// <summary>
        /// Gets the SHA1 hash of the public part of this key.
        /// </summary>
        /// <returns></returns>
        public string GetFingerprint()
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] pubkey = Encoding.ASCII.GetBytes(rsa.ToXmlString(false));
            return Util.BytesToHexString(sha.ComputeHash(pubkey));
        }

        public byte[] Sign(byte[] hash)
        {
            return rsa.SignHash(hash, Hasher.HashOIDMap[Config.Get(Config.HASH_ALGORITHM)]); //Microsoft .NET will accept the algorithm name here, but mono (correctly, I suppose - IntelliSense says OID but MS.NET accepts name anyway) only accepts the OID.
        }
    }
}
