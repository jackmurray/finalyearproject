using System;
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
    public class Hasher
    {
        private static Trace Log = new Trace("LibSecurity");
        private HashAlgorithm hasher;

        private Hasher(HashAlgorithm hasher)
        {
            this.hasher = hasher;
        }

        public static Hasher Create()
        {
            string confHashAlgorithm = Config.Get(Config.HASH_ALGORITHM);
            HashAlgorithm hasher = HashAlgorithm.Create(confHashAlgorithm);
            if (hasher == null)
            {
                Log.Critical("Unable to find hash algorithm: " + confHashAlgorithm);
                throw new CryptographicException("Unable to find hash algorithm: " + confHashAlgorithm);
            }

            return new Hasher(hasher);
        }

        public byte[] Hash(byte[] data)
        {
            return hasher.ComputeHash(data);
        }

        public string HashBase64(byte[] data)
        {
            return Util.BytesToBase64String(Hash(data));
        }
    }
}
