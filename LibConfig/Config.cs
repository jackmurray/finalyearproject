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
        public const string LOG_PATH = "logPath";

        public static string Get(string key)
        {
            if (!IsLoaded)
                LoadConfig();

            try
            {
                XmlNode n = xml.SelectSingleNode("/config/" + key);
                return n.InnerText;
            }
            catch (Exception ex)
            {
                throw new ConfigException("Unable to read config value: " + ex.Message);
            }
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
