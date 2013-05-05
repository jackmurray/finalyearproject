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

            Uri uri = Util.GetOurControlURL(false); //Get URI that we're going to run on.
            ServiceHost host = new ServiceHost(typeof (ReceiverService), uri);
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior
            {
                    HttpGetEnabled = true
            };
            host.Description.Behaviors.Add(smb);
            
            //App EP
            host.AddServiceEndpoint(typeof (IReceiverService), new BasicHttpBinding(), uri);
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
    }
}
