using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibConfig
{
    public static class Config
    {
        private const string CONFIG_FILENAME = "config.xml";
        private static bool IsLoaded = false;
        private static XmlDocument xml;
        private static readonly object configLock = new object();

        //List of config keys.
        public const string LOG_PATH = "logPath"; //Path for logfiles.
        public const string CRYPTO_PATH = "cryptoPath"; //Path to store cryptographic material in.
        public const string GEN_PKCS12_CERT = "generatePKCS12Cert";
        public const string WRITE_TRACE_TO_CONSOLE = "writeTraceToConsole";
        public const string DEVICE_FRIENDLY_NAME = "deviceFriendlyName";

        public static string Get(string key)
        {
            lock (configLock) //we don't wanna accidentally try and read something while we're updating it.
            {
                if (!IsLoaded)
                    LoadConfig();

                try
                {
                    XmlNode n = xml.SelectSingleNode("/config/" + key);
                    if (n == null)
                        throw new ConfigException("Missing config key: " + key);
                    return n.InnerText;
                }
                catch (Exception ex)
                {
                    throw new ConfigException("Unable to read config value: " + ex.Message);
                }
            }
        }

        public static void Set(string key, string value)
        {
            lock (configLock)
            {
                XmlNode n = xml.SelectSingleNode("/config/" + key);
                if (n == null)
                {
                    n = xml.CreateNode(XmlNodeType.Element, key, "");
                    XmlNode root = xml.SelectSingleNode("/config");
                    root.AppendChild(n);
                }

                n.InnerText = value;

                xml.Save(CONFIG_FILENAME);
            }
        }

        public static bool Exists(string key)
        {
            lock (configLock)
            {
                try
                {
                    XmlNode n = xml.SelectSingleNode("/config/" + key);
                    return n != null;
                }
                catch (Exception ex)
                {
                    throw new ConfigException("Unable to read config value: " + ex.Message);
                }
            }
        }

        public static bool GetFlag(string key)
        {
            return bool.Parse(Get(key));
        }

        /// <summary>
        /// Gets a config key, and interprets it's value as a path.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetPath(string key)
        {
            string p = Get(key);
            return System.IO.Path.GetFullPath(p);
        }
        
        private static void LoadConfig()
        {
            xml = new XmlDocument();
            try
            {
                xml.Load(CONFIG_FILENAME);
            }
            catch (Exception ex)
            {
                throw new ConfigException("Unable to load config: " + ex.Message);
            }
            IsLoaded = true;
        }
    }

    public class ConfigException : Exception
    {
        public ConfigException(string message) : base(message)
        {
            
        }
    }
}
