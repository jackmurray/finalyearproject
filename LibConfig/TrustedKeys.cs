using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.IO;

namespace LibConfig
{
    public static class TrustedKeys
    {
        private static List<string> KeyList = new List<string>();
        private static string Path = System.IO.Path.Combine(Config.GetPath(Config.CRYPTO_PATH), "trustedKeys");

        public static void Load(XmlNodeList data)
        {
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
            return KeyList.Contains(fingerprint);
        }

        public static List<string> GetAllKeys()
        {
            return new List<string>(KeyList);
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
    }
}
