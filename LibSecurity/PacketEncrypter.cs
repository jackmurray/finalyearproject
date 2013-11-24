using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace LibSecurity
{
    public class PacketEncrypter
    {
        private IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
        private KeyParameter key;
        private byte[] nonce;

        private void Init(byte[] key, byte[] nonce, long ctr)
        {
            this.key = new KeyParameter(key);

            int bs = cipher.GetBlockSize();
            if (nonce.Length != bs - 8) //only the top part of the IV will be the nonce. bottom 8 bytes will be counter.
                throw new ArgumentException("Nonce must be == blockSize - 8 in length");

            byte[] iv = new byte[bs];
            Array.Copy(nonce, iv, nonce.Length);
            //Once BC ingests the IV it will increment the bottom byte(s) of the array, so we put the counter into network/bigendian order
            byte[] ctrbytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ctr));
            Array.Copy(ctrbytes, 0, iv, iv.Length - 8, 8);

            cipher.Init(true, new ParametersWithIV(this.key, iv));
        }

        /// <summary>
        /// Create a new instance for encryption. Nonce will be generated with CSPRNG.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctr"></param>
        public PacketEncrypter(byte[] key, long ctr)
        {
            RandomNumberGenerator rand = new RNGCryptoServiceProvider();
            nonce = new byte[cipher.GetBlockSize() / 2]; 
            rand.GetBytes(nonce);
            this.Init(key, nonce, ctr);
        }

        /// <summary>
        /// Create a new instance for encryption or decryption with the specified nonce.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctr"></param>
        /// <param name="nonce"></param>
        public PacketEncrypter(byte[] key, long ctr, byte[] nonce)
        {
            this.Init(key, nonce, ctr);
        }

        public byte[] Encrypt(byte[] data)
        {
            int realDataLength = data.Length;
            //BC has a bug that forces you to round this up to the nearest blocksize multiple. because we're in CTR mode we only really need a buffer the same size as our data...
            int bufSize = realDataLength;
            int diff = realDataLength%cipher.GetBlockSize();
            bufSize += diff != 0 ? (cipher.GetBlockSize() - diff) : 0; 
            byte[] output = new byte[bufSize];
            int bytesLeft = data.Length;

            //will process only a whole number of blocks. there can be 0 - 15 bytes left unprocessed after this call.
            bytesLeft -= cipher.ProcessBytes(data, output, 0);

            if (bytesLeft == 0)
                return output;
            else
            {
                cipher.DoFinal(output, realDataLength - bytesLeft);
            }
            return output.Take(realDataLength).ToArray();
        }
    }
}
