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
        public const string CONFIG_FILENAME = "config.xml";
        public const string TRUSTED_KEYS_FILENAME = "trustedKeys.xml";

        private static bool IsLoaded = false;
        private static XmlDocument xml;
        private static readonly object configLock = new object();

        //List of config keys.
        public const string LOG_PATH = "logPath"; //Path for logfiles.
        public const string CRYPTO_PATH = "cryptoPath"; //Path to store cryptographic material in.
        public const string GEN_PKCS12_CERT = "generatePKCS12Cert";
        public const string WRITE_TRACE_TO_CONSOLE = "writeTraceToConsole";
        public const string DEVICE_FRIENDLY_NAME = "deviceFriendlyName";
        public const string TRACE_LEVEL = "traceLevel";
        public const string PAIRING_KEY = "pairingKey"; //This must be the same on all devices.
        public const string MAX_TIMEDIFF = "maxTimeDifference";
        public const string STREAM_BUFFER_TIME = "streamBufferTime"; //The number of seconds of audio data to pre-buffer.
        public const string PLAYER_EXECUTABLE = "playerExecutable";
        public const string PLAYER_ARGUMENTS = "playerArgs";
        public const string ENABLE_ENCRYPTION = "enableEncryption";
        public const string ENABLE_AUTHENTICATION = "enableAuthentication";

        public static string Get(string key)
        {
            lock (configLock) //we don't wanna accidentally try and read something while we're updating it.
            {
                if (!IsLoaded)
                    LoadConfig();

                try
                {
                    return GetRawNode(key).InnerText;
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

        public static int GetInt(string key)
        {
            return int.Parse(Get(key));
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

        public static XmlNode GetRawNode(string key)
        {
            XmlNode n = xml.SelectSingleNode("/config/" + key);
            if (n == null)
                throw new ConfigException("Missing config key: " + key);
            return n;
        }
        
        /// <summary>
        /// Load config data. Will be called automatically if needed, but can be done manually if required.
        /// </summary>
        public static void LoadConfig()
        {
            if (IsLoaded)
                return;
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

            Config.SanityCheck();
        }

        public static System.Diagnostics.SourceLevels GetTraceLevel()
        {
            string raw = Get(TRACE_LEVEL);
            System.Diagnostics.SourceLevels result;
            if (Enum.TryParse(raw, out result))
                return result;
            else
                throw new ConfigException("Unable to parse '" + raw + "' as a valid SourceLevel.");
        }

        /// <summary>
        /// Must be called explicitly - it's not done automatically like normal config stuff.
        /// </summary>
        public static void LoadTrustedKeys()
        {
            XmlDocument keysxml = new XmlDocument();
            keysxml.Load(System.IO.Path.Combine(GetPath(CRYPTO_PATH), TRUSTED_KEYS_FILENAME));

            XmlNode root = keysxml.SelectSingleNode("/keys");
            TrustedKeys.Load(root.ChildNodes);
        }

        public static void SaveTrustedKeys()
        {
            XmlDocument keysxml = new XmlDocument();
            XmlNode root = keysxml.CreateNode(XmlNodeType.Element, "keys", "");
            foreach (string key in TrustedKeys.GetAllKeys())
            {
                XmlNode n = keysxml.CreateNode(XmlNodeType.Element, "key", "");
                n.InnerText = key;
                root.AppendChild(n);
            }
            keysxml.AppendChild(root);
            keysxml.Save(System.IO.Path.Combine(GetPath(CRYPTO_PATH), TRUSTED_KEYS_FILENAME));
        }

        private static void SanityCheck()
        {
            if (Config.GetFlag(ENABLE_AUTHENTICATION) && !Config.GetFlag(ENABLE_ENCRYPTION))
                throw new ConfigException("Authentication requires encryption to be enabled.");
        }
    }

    public class ConfigException : Exception
    {
        public ConfigException(string message) : base(message)
        {
            
        }
    }
}
