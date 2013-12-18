using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using LibTrace;
using LibConfig;
using LibUtil;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace LibSecurity
{
    public class KeyManager
    {
        private const int KEY_LENGTH_TEMP = 512; //for RTP signing
        private const int KEY_LENGTH_PERM = 2048; //for the device's service certificate and SSDP
        private const int PUBLIC_EXPONENT_PERM = 65537; //standard value for e (F4).
        private const int PUBLIC_EXPONENT_TEMP = 3; //Use 3 (F1) as e because it's ~8 times faster than F4. **STILL SECURE**
        private static Trace Log = Trace.GetInstance("LibSecurity");
        private AsymmetricCipherKeyPair rsa;
        public AsymmetricKeyParameter Public { get { return rsa.Public; } }
        public AsymmetricKeyParameter Private { get { return rsa.Private; } }

        private KeyManager(AsymmetricCipherKeyPair rsa)
        {
            this.rsa = rsa;
        }

        public static KeyManager CreatePermenantKey()
        {
            Log.Verbose("Creating permenant RSA key...");
            return new KeyManager(RsaGenerator.Generate(KEY_LENGTH_PERM, PUBLIC_EXPONENT_PERM));
        }

        public static KeyManager CreateTemporaryKey()
        {
            Log.Verbose("Creating temp RSA key.");
            return new KeyManager(RsaGenerator.Generate(KEY_LENGTH_TEMP, PUBLIC_EXPONENT_TEMP));
        }

        public AsymmetricAlgorithm ToDotNetKey()
        {
            //Once again, StackOverflow to the rescue.
            //http://stackoverflow.com/questions/7984945/the-credentials-supplied-to-the-package-were-not-recognized-error-when-authent
            // Apparently, using DotNetUtilities to convert the private key is a little iffy. Have to do some init up front.
            RSACryptoServiceProvider tempRcsp = (RSACryptoServiceProvider)DotNetUtilities.ToRSA(rsa.Private as RsaPrivateCrtKeyParameters);
            RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(new CspParameters(1, "Microsoft Strong Cryptographic Provider", Guid.NewGuid().ToString()));
            

            rcsp.ImportCspBlob(tempRcsp.ExportCspBlob(true));

            return rcsp;
        }

        public void WriteKeyToFile(string filename, bool writePrivateKey)
        {
            StreamWriter w = new StreamWriter(filename);
            AsymmetricKeyParameter param = (writePrivateKey == true ? rsa.Private : rsa.Public);
            new PemWriter(w).WriteObject(param);
            w.Close();
            Log.Verbose("Wrote key to file " + filename);
        }

        public static KeyManager LoadKeyFromFile(string filename)
        {
            var key = (AsymmetricCipherKeyPair) new PemReader(new StreamReader(filename)).ReadObject();
            Log.Verbose("Loaded rsa key from file " + filename);
            return new KeyManager(key);
        }

        /// <summary>
        /// Loads our key from storage, or if we don't have one, create a new one.
        /// </summary>
        /// <returns></returns>
        public static KeyManager GetKey()
        {
            string privateKeyFile = LibUtil.Util.ResolvePath(Config.Get(Config.CRYPTO_PATH), "private.key");
            KeyManager key = null;

            if (System.IO.File.Exists(privateKeyFile))
            {
                Log.Information("Not generating key as we already have one.");
                try
                {
                    key = KeyManager.LoadKeyFromFile(privateKeyFile);
                }
                catch (CryptographicException ex)
                {
                    Log.Critical("Failed to load key: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    Log.Information("Generating key. This takes several minutes on the RPi.");
                    key = KeyManager.CreatePermenantKey();
                    key.WriteKeyToFile(privateKeyFile, true);
                    Log.Information("Key generated.");
                }
                catch (Exception ex)
                {
                    Log.Critical("Failed to generate key: " + ex.Message);
                    throw;
                }
            }
            return key;
        }
    }
}
