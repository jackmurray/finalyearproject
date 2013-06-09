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
        private const int KEY_LENGTH = 2048;
        private static Trace Log = Trace.GetInstance("LibSecurity");
        private AsymmetricCipherKeyPair rsa;
        public AsymmetricKeyParameter Public { get { return rsa.Public; } }
        public AsymmetricKeyParameter Private { get { return rsa.Private; } }

        private KeyManager(AsymmetricCipherKeyPair rsa)
        {
            this.rsa = rsa;
        }

        public static KeyManager Create()
        {
            Log.Verbose("Creating new RSA key...");
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();
            g.Init(new KeyGenerationParameters(new SecureRandom(), KEY_LENGTH));
            AsymmetricCipherKeyPair key = g.GenerateKeyPair();
            Log.Verbose("Generated new RSA key.");
            return new KeyManager(key);
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
            string privateKeyFile = System.IO.Path.Combine(Config.Get(Config.CRYPTO_PATH), "private.key");
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
                    key = KeyManager.Create();
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
