using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace LibSecurity
{
    public static class RsaGenerator
    {
        public static AsymmetricCipherKeyPair Generate(int length, int publicExponent = 65537)
        {
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();
            int certainty;

            /* These values are taken from http://csrc.nist.gov/publications/nistpubs/800-57/sp800-57-Part1-revised2_Mar08-2007.pdf
             * The certainty represents the probability (1 - 2^-x) that the RSA primes are actually prime.
             * The certainty should be chosen to be the same size as the 'bits of security' provided by a modulus of a given length.
             * This is because you can assume that the weakest link in the cryptosystem will be broken first, so if you have say certainty=128
             * but bitsofsecurity=64 then the modulus will be broken first.
             */
            if (length <= 1024) certainty = 80;
            else if (length <= 2048) certainty = 112;
            else if (length <= 3072) certainty = 128;
            else certainty = 192;
            g.Init(new RsaKeyGenerationParameters(new BigInteger(publicExponent.ToString()), new SecureRandom(), length, certainty));
            return g.GenerateKeyPair();
        }
    }
}
