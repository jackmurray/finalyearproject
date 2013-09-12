﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibConfig;
using System.IO;

namespace LibUtil
{
    public static class Util
    {
        public static string BytesToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", String.Empty).ToLower();
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
            return String.Format("{0:d4}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}", utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute,
                                 utc.Second);
        }

        public static void CreateItems()
        {
            Util.MkDir(Config.Get(Config.LOG_PATH));
            Util.MkDir(Config.Get(Config.CRYPTO_PATH));
            Util.MkDir(Path.Combine(Config.GetPath(Config.CRYPTO_PATH), "trustedKeys"));
            if (!File.Exists(Path.Combine(Config.GetPath(Config.CRYPTO_PATH), Config.TRUSTED_KEYS_FILENAME)))
                Config.SaveTrustedKeys(); //If the file didn't exist then we can call this and it'll make it for us.
        }

        public static byte[] Encode(int val)
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

        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("Lib") || a.GetName().Name.StartsWith("Speaker"));
        }
    }
}
