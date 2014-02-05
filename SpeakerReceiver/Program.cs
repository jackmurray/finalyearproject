using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibAudio;
using LibCommon;
using LibSecurity;
using LibService;
using LibTrace;
using LibConfig;
using LibTransport;
using LibUtil;
using SDL2;


namespace SpeakerReceiver
{
    class Program
    {
        private static Trace Log;
        private static StreamReceiver r;
        private static KeyManager key;
        private static CertManager cert;
        private static Controller controller;

        static uint len;
        static IntPtr bufloc;
        static int pos = 0;
        static SDL.SDL_AudioSpec want;

        private static void Main(string[] args)
        {
            if (args.Length > 0)
                ProcessConfigOverride(args);
            Setup();
            if (args.Length > 0)
                ProcessArgs(args);
            
            Log.Information("Build Flavour: " + GetBuildFlavour());
            foreach (AssemblyName n in Util.GetReferencedAssemblies())
                Log.Verbose(n.Name + "-" + n.Version);
            
            Log.Information("Platform: " + (Config.IsRunningOnMono ? "Mono" : "MS.NET"));
            try
            {
                key = KeyManager.GetKey();
                cert = CertManager.GetCert(key);
            }
            catch (Exception ex)
            {
                Log.Critical("Failed to load key/cert. Exiting. " + ex.Message);
                Exit();
            }
            
            new LibSSDP.SSDPService(key, cert).Start();

            LibService.ServiceRegistration.Register(new LibService.CommonService());
            LibService.ServiceRegistration.Register(new LibService.PairingService());
            LibService.ServiceRegistration.Register(new LibService.TransportService(Handler_TransportService_JoinGroup, Handler_TransportService_SetEncryptionKey, Handler_TransportService_SetControllerAddress, Handler_SetSigningKey));
            LibService.ServiceRegistration.Start(cert.ToDotNetCert(key), 10451);
            Console.WriteLine("SpeakerReceiver ready.");
            Console.ReadLine();

            Cleanup();
        }

        private static void Setup()
        {
            Config.CreateItems();
            Trace.Initialised = true;
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

        private static void ProcessConfigOverride(string[] args)
        {
            if (args[0] == "--config")
            {
                Config.CONFIG_FILENAME = args[1];
                Console.WriteLine("Overriding config to " + args[1]);
            }
        }

        public static void Handler_TransportService_JoinGroup(IPAddress ip)
        {
            if (r != null)
                r.Stop();
            r = new StreamReceiver(new RTPInputStream(new IPEndPoint(ip, 10452)));

            //wait the configured amount of time because things like VLC take a while to start before you can start writing data to them.
            int startupWait = Config.GetInt(Config.PLAYER_STARTUP_WAIT);
            if (startupWait > 0)
            {
                Log.Verbose("Waiting " + startupWait + "ms for player to start.");
                Thread.Sleep(startupWait);
            }


            r.OnKeyRotatePacketReceived += Handler_OnRotateKeyPacketReceived;
            r.Start();
        }

        public static void Handler_TransportService_SetEncryptionKey(byte[] key, byte[] nonce)
        {
            r.SetEncryptionKey(new PacketEncrypterKeyManager(key, nonce));
        }

        public static void Handler_OnRotateKeyPacketReceived()
        {
            Log.Verbose("SR asked for a new key, guess we'd better go get one...");
            new Thread(RotateKeyFetchThreadProc).Start();
        }

        private static void RotateKeyFetchThreadProc()
        {
            SslClient ssl = controller.GetSsl(cert, key);
            KeyServiceClient kc = ssl.GetClient<KeyServiceClient>();
            try
            {
                var newkey = kc.GetNextKey();
                r.DeliverNewKey(newkey.Item1, newkey.Item2);
                Log.Verbose("New key delivered.");
            }
            catch (Exception)
            {
                Log.Critical("Key was rotated and we were unable to get the new one! Quitting.");
                Environment.Exit(1);
            }
        }

        public static void Handler_TransportService_SetControllerAddress(IPAddress ip, ushort port, X509Certificate cert)
        {
            controller = new Controller(new IPEndPoint(ip, port), cert);
            Log.Verbose("Your controller today will be " + controller + " please enjoy your stream.");
        }

        public static void Handler_SetSigningKey(Verifier v)
        {
            r.SetVerifier(v);
        }

        private static void myCallback(IntPtr i, IntPtr j, int k)
        {
            byte[] temp = new byte[k];
            Marshal.Copy(bufloc + pos, temp, 0, k);
            Marshal.Copy(temp, 0, j, k);
            pos += k;
        }
    }
}
