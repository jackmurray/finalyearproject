using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
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
                Console.WriteLine("Key loaded.");
                cert = CertManager.GetCert(key);
                Console.WriteLine(cert.Subject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load key/cert. Exiting. " + ex.Message);
                Exit();
            }
            
            new LibSSDP.SSDPService(key, cert).Start();

            Uri uri = Util.GetOurControlURL(false); //Get URI that we're going to run on.

            ServiceHost host = new ServiceHost(typeof (ReceiverService), uri);
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior
            {
                    HttpGetEnabled = true
            };
            host.Description.Behaviors.Add(smb);
            
            //App EP
            ServiceEndpoint appEP = host.AddServiceEndpoint(typeof (IReceiverService), new BasicHttpBinding(), uri);
#if !DEBUG
            appEP.Behaviors.Add(new ServerRequestProcessor(new LibSecurity.WebServiceProtector(true)));
#endif
            //MEX EP
            host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                                    MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
            
            host.Open();
            Console.WriteLine("Service started on " + uri);
            Console.ReadLine();
            host.Close();

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


        private static string GetBuildFlavour()
        {
#if DEBUG
            return "DEBUG";
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
