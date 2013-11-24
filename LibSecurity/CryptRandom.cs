using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LibSecurity
{
    public static class CryptRandom
    {
        private static RandomNumberGenerator rand = new RNGCryptoServiceProvider();

        public static byte[] GetBytes(int count)
        {
            lock (rand)
            {
                byte[] buf = new byte[count];
                rand.GetBytes(buf);
                return buf;
            }
        }
    }
}
