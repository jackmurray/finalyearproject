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
            Console.WriteLine("{0}-{1}", GetBuildVersion(), GetBuildFlavour());
            Console.WriteLine("Platform: " + (Config.IsRunningOnMono == true ? "Mono" : "MS.NET"));
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

            LibService.ServiceRegistration.Register(new LibService.CommonService(GetBuildVersion()));
            new SslServer(cert.ToDotNetCert(key)).Listen(10451);
            Console.ReadLine();

            Cleanup();
        }

        private static void Setup()
        {
            Config.LoadConfig(); //Manually load config.
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

        private static Version GetBuildVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        }
    }
}
