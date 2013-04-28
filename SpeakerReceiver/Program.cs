using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LibSecurity;
using LibTrace;
using LibConfig;


namespace SpeakerReceiver
{
    class Program
    {
        private static Trace Log = new Trace("SpeakerReceiver");

        static void Main(string[] args)
        {
            Setup();
            KeyManager key = GetKey();
            if (key == null)
                Exit();

            Console.WriteLine("Key fingerprint: " + key.GetFingerprint());
            new LibSSDP.SSDPService(key).Start();

            Cleanup();
        }

        private static void Setup()
        {
            CreateDirs();
        }

        private static void Cleanup()
        {
            Log.Close();
        }

        private static void Exit()
        {
            Cleanup();
            Environment.Exit(1);
        }

        private static void CreateDirs()
        {
            if (!System.IO.Directory.Exists(Config.Get(Config.CRYPTO_PATH)))
                System.IO.Directory.CreateDirectory(Config.Get(Config.CRYPTO_PATH));
        }

        private static KeyManager GetKey()
        {
            string privateKeyFile = System.IO.Path.Combine(Config.Get(Config.CRYPTO_PATH), Config.Get(Config.DEVICE_PRIVATEKEY_FILE));
            KeyManager key = null;

            if (System.IO.File.Exists(privateKeyFile))
            {
                Console.WriteLine("Not generating key as we already have one.");
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
                    Console.WriteLine("Generating key. This takes several minutes on the RPi.");
                    key = KeyManager.Create();
                    key.WriteKeyToFile(privateKeyFile);
                }
                catch (Exception ex)
                {
                    Log.Critical("Failed to generate key: " + ex.Message);
                }
            }
            return key;
        }
    }
}
