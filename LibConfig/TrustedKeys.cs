using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.IO;
using LibUtil;

namespace LibConfig
{
    public static class TrustedKeys
    {
        private static List<string> KeyList;
        private static string Path = Util.ResolvePath(Config.Get(Config.CRYPTO_PATH), "trustedKeys");

        public static void Load(XmlNodeList data)
        {
            KeyList = new List<string>();
            foreach (XmlNode n in data)
                KeyList.Add(n.InnerText);
        }

        public static void Add(X509Certificate cert)
        {
            string fingerprint = cert.GetCertHashString().ToLower();
            if (Contains(fingerprint)) //if we already have this key, no need to do anything.
                return;

            KeyList.Add(fingerprint);
            var fs = File.OpenWrite(System.IO.Path.Combine(Path, fingerprint + ".crt"));
            byte[] data = cert.GetRawCertData();
            fs.Write(data, 0, data.Length);
            fs.Close();

            Config.SaveTrustedKeys(); //save out the data. don't want to lose it if we crash.
        }

        public static bool Contains(string fingerprint)
        {
            return KeyList.Contains(fingerprint.ToLower());
        }

        public static List<string> GetAllKeys()
        {
            return new List<string>(KeyList); //We return a new list rather than our internal one so we don't leak the reference outside the class.
        }

        public static X509Certificate Get(string key)
        {
            if (!Contains(key))
                return null;

            string fingerprint = key.ToLower();
            var fs = File.OpenRead(System.IO.Path.Combine(Path, fingerprint + ".crt"));
            byte[] buf = new byte[fs.Length];
            fs.Read(buf, 0, (int)fs.Length);
            return new X509Certificate(buf);
        }

        /// <summary>
        /// Remove the key at index i in the internal array.
        /// </summary>
        /// <param name="i"></param>
        public static void Remove(int i)
        {
            KeyList.RemoveAt(i);
            Config.SaveTrustedKeys();
        }

        public static void Remove(string fingerprint)
        {
            KeyList.Remove(fingerprint);
            Config.SaveTrustedKeys();
        }
    }
}
