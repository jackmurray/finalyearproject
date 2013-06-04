﻿using System;
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

        //List of config keys.
        public const string LOG_PATH = "logPath"; //Path for logfiles.
        public const string CRYPTO_PATH = "cryptoPath"; //Path to store cryptographic material in.
        public const string GEN_PKCS12_CERT = "generatePKCS12Cert";

        public static string Get(string key)
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
