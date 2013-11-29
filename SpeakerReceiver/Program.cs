﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LibSecurity;
using LibTrace;
using LibConfig;
using LibTransport;
using LibUtil;


namespace SpeakerReceiver
{
    class Program
    {
        private static Trace Log;
        private static StreamReceiver r;

        private static void Main(string[] args)
        {
            Setup();
            if (args.Length > 0)
                ProcessArgs(args);
            Console.WriteLine("Build Flavour: " + GetBuildFlavour());
            foreach (Assembly a in Util.GetLoadedAssemblies())
                Log.Verbose(a.GetName().Name + "-" + a.GetName().Version);
            
            Console.WriteLine("Platform: " + (Config.IsRunningOnMono ? "Mono" : "MS.NET"));
            
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
            LibService.ServiceRegistration.Register(new LibService.PairingService());
            LibService.ServiceRegistration.Register(new LibService.TransportService(Handler_TransportService_JoinGroup, Handler_TransportService_JoinGroupEncrypted));
            LibService.ServiceRegistration.Start(cert.ToDotNetCert(key), 10451);
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

        private static void ProcessArgs(string[] args)
        {
            if (args[0] == "--list-pairings")
            {
                var keys = TrustedKeys.GetAllKeys();
                for (int i = 0; i < keys.Count; i++)
                    Console.WriteLine(i + ": " + keys[i]);
                Environment.Exit(0);
            }
            if (args[0] == "--delete-pairing")
            {
                if (args.Length != 2)
                    throw new ArgumentException("Must provide cert index to delete");
                int i = int.Parse(args[1]);
                TrustedKeys.Remove(i);
                Environment.Exit(0);
            }
        }

        public static void Handler_TransportService_JoinGroup(IPAddress ip)
        {
            if (r != null)
                r.Stop();
            r = new StreamReceiver(new RTPInputStream(new IPEndPoint(ip, 10452), Config.GetFlag(Config.ENABLE_ENCRYPTION)));
            r.Start();
        }

        public static void Handler_TransportService_JoinGroupEncrypted(IPAddress ip, byte[] key, byte[] nonce)
        {
            if (r != null)
                r.Stop();
            r = new StreamReceiver(new RTPInputStream(new IPEndPoint(ip, 10452), Config.GetFlag(Config.ENABLE_ENCRYPTION), key, nonce));
            r.Start();
        }
    }
}
