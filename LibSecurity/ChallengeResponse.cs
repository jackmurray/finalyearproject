using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace LibSecurity
{
    public class ChallengeResponse
    {
        public const int CHALLENGE_SIZE = 32;
        public const int ROUNDS = 100000;
        private static RandomNumberGenerator rand = new RNGCryptoServiceProvider();

        public byte[] ChallengeBytes { get; private set; }

        /// <summary>
        /// Create a new ChallengeResponse object with a random challenge value.
        /// </summary>
        public ChallengeResponse()
        {
            ChallengeBytes = new byte[CHALLENGE_SIZE];
            rand.GetBytes(ChallengeBytes);
        }

        /// <summary>
        /// Create a ChallengeResponse object with a known challenge value.
        /// </summary>
        /// <param name="challenge"></param>
        public ChallengeResponse(byte[] challenge)
        {
            if (challenge == null || challenge.Length != CHALLENGE_SIZE)
                throw new ArgumentException("Challenge must be of length " + CHALLENGE_SIZE);

            ChallengeBytes = challenge;
        }

        /// <summary>
        /// Sign a challenge using the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] Sign(byte[] key)
        {
            return CalculateSignature(key);
        }

        /// <summary>
        /// Verify the signature on a challenge with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Verify(byte[] key, byte[] signature)
        {
            return signature.SequenceEqual(CalculateSignature(key));
        }

        private byte[] CalculateSignature(byte[] key)
        {
            HMACSHA256 hash = new HMACSHA256(key);
            byte[] output = new byte[hash.HashSize/8];
            output = hash.ComputeHash(ChallengeBytes);
            for (int i = 0; i < ROUNDS; i++) //we do one initial round to mix in the challenge bytes, then repeat the hash ROUNDS times.
            {
                output = hash.ComputeHash(output);
            }

            return output;
        }
    }
}
