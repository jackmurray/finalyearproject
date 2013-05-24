﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LibConfig;

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

        public static void MkDir(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }

        public static string FormatDate(DateTime dt)
        {
            DateTime utc = dt.ToUniversalTime();
            return String.Format("{0:d4}-{1:d2}-{2:d2} {3:d2}:{4:d2}:{5:d2}", utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute,
                                 utc.Second);
        }

        public static void CreateDirs()
        {
            Util.MkDir(Config.Get(Config.LOG_PATH));
            Util.MkDir(Config.Get(Config.CRYPTO_PATH));
            Util.MkDir(System.IO.Path.Combine(Config.Get(Config.CRYPTO_PATH), "trustedKeys"));
        }

        public static Uri GetOurControlURL(bool isController)
        {
            IPAddress ourip =
                Dns.GetHostEntry(Dns.GetHostName())
                   .AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            string svcname = isController ? "Controller" : "Receiver";
            return new Uri(String.Format("http://{0}:10451/{1}.svc", ourip, svcname));
        }
    }
}
