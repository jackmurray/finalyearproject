using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LibUtil
{
    public static class Util
    {
        public static Dictionary<string, Version> GetComponentVersions()
        {
            var ret = new Dictionary<string, Version>();
            foreach (AssemblyName n in Util.GetReferencedAssemblies())
            {
                if (n.Name == "SpeakerController" || n.Name == "SpeakerReceiver")
                    ret.Add("Core", n.Version);
                else
                    ret.Add(n.Name, n.Version);
            }
            return ret;
        }

        public static string BytesToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", String.Empty).ToLower();
        }

        public static byte[] HexStringToBytes(string s)
        {
            if (s.Length %2 != 0)
                throw new ArgumentException("String length must be a multiple of 2");

            byte[] bytes = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return bytes;
        }

        public static string BytesToBase64String(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static byte[] Base64ToByteArray(string s)
        {
            return Convert.FromBase64String(s);
        }

        public static void MkDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string FormatDate(DateTime dt)
        {
            DateTime utc = dt.ToUniversalTime();
            return String.Format("{0:d4}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}.{6:d3}", utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute,
                                 utc.Second, utc.Millisecond);
        }

        public static byte[] Encode(int val)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val));
        }

        //There are implicit conversions from uint -> long and ushort -> int, so we take to use Skip+ToArray to get only the bytes we wanted.
        public static byte[] Encode(uint val)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val)).Skip(4).ToArray();
        }

        public static byte[] Encode(ushort val)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val)).Skip(2).ToArray(); 
        }

        public static byte[] Encode(long val)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val));
        }

        public static int Decode(byte[] val)
        {
            return Decode(val, 0);
        }

        public static int Decode(byte[] val, int offset)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(val, offset));
        }

        public static uint DecodeUint(byte[] val, int offset)
        {
            return (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(val, offset));
        }

        public static ushort DecodeUshort(byte[] val, int offset)
        {
            return (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(val, offset));
        }

        public static long DecodeLong(byte[] val, int offset)
        {
            return (long) IPAddress.NetworkToHostOrder(BitConverter.ToInt64(val, offset));
        }

        public static IEnumerable<AssemblyName> GetReferencedAssemblies()
        {
            var all = new List<AssemblyName>();
            all.AddRange(Assembly.GetEntryAssembly().GetReferencedAssemblies().Where(a => a.Name.StartsWith("Lib")));
            all.Add(Assembly.GetEntryAssembly().GetName());
            return all;
        }

        public static bool IsMulticastAddress(IPAddress ip)
        {
            //mask out the top 4 bits of the first octet and check if they're 1110 (which for multicast, they must be).
            return (ip.GetAddressBytes()[0] & 0xF0) == 0xE0;
        }

        public static string ResolvePath(params string[] args)
        {
            string basedir =
                Environment.ExpandEnvironmentVariables(Environment.OSVersion.Platform == PlatformID.Win32NT
                                                           ? "%LOCALAPPDATA%"
                                                           : "%HOME%");
            string appdir = Environment.ExpandEnvironmentVariables(Environment.OSVersion.Platform == PlatformID.Win32NT
                                                           ? "mcspkr"
                                                           : ".mcspkr");
            string typedir = Util.IsController() ? "controller" : "receiver";

            var ret = new List<string>() {basedir, appdir, typedir};
            ret.AddRange(args);
            return System.IO.Path.Combine(ret.ToArray());
        }

        public static bool IsController()
        {
#if TEST
            return true; //UnitTest framework doesn't define an entry assembly so just override it to use the controller path
#endif
            return Assembly.GetEntryAssembly().GetName().Name == "SpeakerController";
        }
    }
}
