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
        public const int ROUNDS = 10000; //takes about 3 seconds on the Pi. Not as many rounds as I'd like but it's a hard perf limit.

        public byte[] ChallengeBytes { get; private set; }

        /// <summary>
        /// Create a new ChallengeResponse object with a random challenge value.
        /// </summary>
        public ChallengeResponse()
        {
            ChallengeBytes = CryptRandom.GetBytes(CHALLENGE_SIZE);
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
        /// Sign a challenge using the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] Sign(string key)
        {
            return Sign(Encoding.UTF8.GetBytes(key));
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

        public bool Verify(string key, byte[] signature)
        {
            return Verify(Encoding.UTF8.GetBytes(key), signature);
        }

        private byte[] CalculateSignature(byte[] key)
        {
            LibTrace.Trace.GetInstance("LibSecurity").Verbose("Beginning HMAC signature.");
            HMACSHA256 hash = new HMACSHA256(key);
            byte[] output = new byte[hash.HashSize/8];
            output = hash.ComputeHash(ChallengeBytes);
            for (int i = 0; i < ROUNDS; i++) //we do one initial round to mix in the challenge bytes, then repeat the hash ROUNDS times.
            {
                output = hash.ComputeHash(output);
            }
            LibTrace.Trace.GetInstance("LibSecurity").Verbose("HMAC signature complete.");
            return output;
        }
    }
}
