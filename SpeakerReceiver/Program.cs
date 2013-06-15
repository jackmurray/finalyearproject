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
            Console.WriteLine("{0}-{1}", GetBuildVersion(), GetBuildFlavour());
            Setup();
            KeyManager key = null;
            CertManager cert = null;
            try
            {
                key = KeyManager.GetKey();
                cert = CertManager.GetCert(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load key/cert. Exiting. " + ex.Message);
                Exit();
            }
            
            new LibSSDP.SSDPService(key, cert).Start();

            LibService.ServiceRegistration.Register(new LibService.CommonService());
            new SslServer(cert.ToDotNetCert(key)).Listen(10451);
            Console.ReadLine();

            Cleanup();
        }

        private static void Setup()
        {
            Util.CreateItems();
            Log = Trace.GetInstance("SpeakerReceiver"); //Do this after we create the log dir.
            Config.LoadTrustedKeys();
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


        private static string GetBuildFlavour()
        {
#if DEBUG
            return "DEBUG";
#elif NONDEBUG
            return "NONDEBUG";
#else
            return "RELEASE";
#endif
        }

        private static string GetBuildVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}
