using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LibUtil;

namespace LibConfig
{
    public static class Config
    {
        public static string CONFIG_FILENAME = "config.xml";
        public static string TRUSTED_KEYS_FILENAME = "trustedKeys.xml";

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
        public const string MAX_TIMEDIFF = "maxTimeDifference"; //Max difference in milliseconds between timestamp sent in SSDP response and the clock on the controller that receives it. The RPi takes a while (1700-2100 ms) to compute the signature, so set this to no less than 2500ms.
        public const string STREAM_BUFFER_TIME = "streamBufferTime"; //The number of milliseconds of audio data to buffer.
        public const string PLAYER_EXECUTABLE = "playerExecutable";
        public const string PLAYER_ARGUMENTS = "playerArgs";
        public const string ENABLE_ENCRYPTION = "enableEncryption";
        public const string ENABLE_AUTHENTICATION = "enableAuthentication";
        public const string ROTATE_KEY_TIME = "rotateKeyTime"; //The number of milliseconds in the future a RotateKey packet should be scheduled for, measured from the time it's generated.
        public const string IP_ADDRESS = "ipAddr";
        public const string MAX_STREAM_ERROR = "maxStreamError"; //number of milliseconds of stream drift we allow before we apply compensation. controller only.

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

                if (n.InnerText != value) //don't bother saving if nothing changed.
                {
                    n.InnerText = value;

                    xml.Save(Util.ResolvePath(CONFIG_FILENAME));
                }
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
            return Boolean.Parse(Get(key));
        }

        public static int GetInt(string key)
        {
            return Int32.Parse(Get(key));
        }

        /// <summary>
        /// Gets a config key, and interprets it's value as a path.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetPath(string key)
        {
            string p = Get(key);
            return Util.ResolvePath(p);
        }

        public static XmlNode GetRawNode(string key)
        {
            lock (configLock)
            {
                XmlNode n = xml.SelectSingleNode("/config/" + key);
                if (n == null)
                    throw new ConfigException("Missing config key: " + key);
                return n;
            }
        }
        
        /// <summary>
        /// Load config data. Will be called automatically if needed, but can be done manually if required.
        /// </summary>
        public static void LoadConfig()
        {
            lock (configLock)
            {
                xml = new XmlDocument();
                try
                {
                    var p = Util.ResolvePath(CONFIG_FILENAME);
                    xml.Load(p);
                    Console.WriteLine("Loaded config from " + p);
                }
                catch (Exception ex)
                {
                    throw new ConfigException("Unable to load config: " + ex.Message);
                }
                IsLoaded = true;

                SanityCheck();
            }
        }

        public static SourceLevels GetTraceLevel()
        {
            string raw = Get(TRACE_LEVEL);
            SourceLevels result;
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
            keysxml.Load(Util.ResolvePath(GetPath(CRYPTO_PATH), TRUSTED_KEYS_FILENAME));

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
            keysxml.Save(Util.ResolvePath(GetPath(CRYPTO_PATH), TRUSTED_KEYS_FILENAME));
        }

        private static void SanityCheck()
        {
            if (GetFlag(ENABLE_AUTHENTICATION) && !GetFlag(ENABLE_ENCRYPTION))
                throw new ConfigException("Authentication requires encryption to be enabled.");
        }

        public static void CreateItems()
        {
            Util.MkDir(GetPath(LOG_PATH));
            Util.MkDir(Util.ResolvePath(Get(Config.CRYPTO_PATH), "trustedKeys")); //can't use getpath here because we need to join with the dir name
            if (!File.Exists(Util.ResolvePath(Get(Config.CRYPTO_PATH), TRUSTED_KEYS_FILENAME)))
                SaveTrustedKeys(); //If the file didn't exist then we can call this and it'll make it for us.
        }
    }

    public class ConfigException : Exception
    {
        public ConfigException(string message) : base(message)
        {
            
        }
    }
}
