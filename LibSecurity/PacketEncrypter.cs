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
        private bool mode;

        /// <summary>
        /// Automatically called by the constructor to set up the cipher. Can be called at any time to reset the state, and change the counter.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctr"></param>
        /// <param name="nonce"></param>
        /// <param name="forEncryption"></param>
        public void Init(PacketEncrypterKeyManager pekm, long ctr, bool forEncryption)
        {
            var kp = new KeyParameter(pekm.Key);

            int bs = cipher.GetBlockSize();
            

            byte[] iv = new byte[bs];
            Array.Copy(pekm.Nonce, iv, pekm.Nonce.Length);
            //Once BC ingests the IV it will increment the bottom byte(s) of the array, so we put the counter into network/bigendian order
            byte[] ctrbytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ctr));
            Array.Copy(ctrbytes, 0, iv, iv.Length - 8, 8);

            cipher.Init(forEncryption, new ParametersWithIV(kp, iv));
            mode = forEncryption;
        }

        /// <summary>
        /// Create a new instance for encryption or decryption with the specified nonce.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctr"></param>
        /// <param name="nonce"></param>
        public PacketEncrypter(PacketEncrypterKeyManager pekm, long ctr, bool forEncryption)
        {
            this.Init(pekm, ctr, forEncryption);
        }

        public int GetBlockSize()
        {
            return cipher.GetBlockSize();
        }

        /// <summary>
        /// Performs the AES-CTR transform. The actual operation for encryption and decryption is the same (a block of data, either cipher or plain text is XOR'd with the result of AESEncrypt(counter)).
        /// Because of the nature of this algorithm we can therefore use the exact same code for both.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] Transform(byte[] data)
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

        public byte[] Encrypt(byte[] data)
        {
            if (mode != true) throw new InvalidOperationException("This instance was initialised for decryption only!");
            return Transform(data);
        }

        public byte[] Decrypt(byte[] data)
        {
            if (mode != false) throw new InvalidOperationException("This instance was initialised for encryption only!");
            return Transform(data);
        }
    }
}
