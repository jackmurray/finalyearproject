using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LibSecurity;
using LibTrace;
using LibConfig;
using LibUtil;


namespace SpeakerReceiver
{
    class Program
    {
        private static Trace Log;

        private static void Main(string[] args)
        {
            Setup();
            KeyManager key = null;
            try
            {
                key = KeyManager.GetKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load key. Exiting. " + ex.Message);
                Exit();
            }

            Console.WriteLine("Key fingerprint: " + key.GetFingerprint());
            new LibSSDP.SSDPService(key).Start();

            Cleanup();
        }

        private static void Setup()
        {
            Util.CreateDirs();
            Log = new Trace("SpeakerReceiver"); //Do this after we create the log dir.
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
    }
}
