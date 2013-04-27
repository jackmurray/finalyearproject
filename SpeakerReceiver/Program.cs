using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LibSecurity;
using LibTrace;


namespace SpeakerReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace Log = new Trace("SpeakerReceiver");

            if (System.IO.File.Exists("key.txt"))
            {
                Console.WriteLine("Not generating key as we already have one.");
                try
                {
                    KeyManager rsa = KeyManager.LoadKeyFromFile("key.txt");
                }
                catch (CryptographicException ex)
                {
                    Log.Critical("Failed to load key: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Generating key...");
                KeyManager.Create().WriteKeyToFile("key.txt");
            }

            Log.Close();
        }
    }
}
