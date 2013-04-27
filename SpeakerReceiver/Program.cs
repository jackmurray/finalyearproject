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
        static void Main(string[] args)
        {
            Trace Log = new Trace("SpeakerReceiver");
            string privateKeyFile = System.IO.Path.Combine(Config.Get(Config.CRYPTO_PATH), Config.Get(Config.DEVICE_PRIVATEKEY_FILE));

            if (!System.IO.Directory.Exists(Config.Get(Config.CRYPTO_PATH)))
                System.IO.Directory.CreateDirectory(Config.Get(Config.CRYPTO_PATH));

            if (System.IO.File.Exists(privateKeyFile))
            {
                Console.WriteLine("Not generating key as we already have one.");
                try
                {
                    KeyManager rsa = KeyManager.LoadKeyFromFile(privateKeyFile);
                }
                catch (CryptographicException ex)
                {
                    Log.Critical("Failed to load key: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Generating key...");
                KeyManager.Create().WriteKeyToFile(privateKeyFile);
            }

            Log.Close();
        }
    }
}
