using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;

namespace LibSecurity
{
    public class Hasher
    {
        public static byte[] Hash(IDigest algorithm, byte[] data)
        {
            algorithm.BlockUpdate(data, 0, data.Length);
            byte[] hash = new byte[algorithm.GetDigestSize()];
            algorithm.DoFinal(hash, 0);
            return hash;
        }

        public static string HashBase64(IDigest algorithm, byte[] data)
        {
            return LibUtil.Util.BytesToBase64String(Hash(algorithm, data));
        }

        public static string HashHex(IDigest algorithm, byte[] data)
        {
            return LibUtil.Util.BytesToHexString(Hash(algorithm, data));
        }
    }
}
