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

        public static Dictionary<string, string> HashOIDMap = new Dictionary<string, string>
            {
                {"MD5", "1.2.840.113549.2.5"},
                {"SHA1", "1.3.14.3.2.26"},
                {"SHA256", "2.16.840.1.101.3.4.2.1"},
                {"SHA384", "2.16.840.1.101.3.4.2.2"},
                {"SHA512", "2.16.840.1.101.3.4.2.3"}
            };

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
