using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibConfig
{
    public class ConfigState
    {
        public ConfigStateFlags Flags { get; set; }

        public ConfigState(ConfigStateFlags f) //needs to be public so the JSON lib can create an instance
        {
            this.Flags = f;
        }

        public static ConfigState GetConfigState()
        {
            ConfigStateFlags st = new ConfigStateFlags();
            if (Config.GetFlag(Config.ENABLE_ENCRYPTION))
                st |= ConfigStateFlags.EncryptionEnabled;
            if (Config.GetFlag(Config.ENABLE_AUTHENTICATION))
                st |= ConfigStateFlags.AuthenticationEnabled;

            return new ConfigState(st);
        }

        public bool Equals(ConfigState s)
        {
            return this.Flags == s.Flags;
        }

        public override string ToString()
        {
            return "ConfigState {Flags=" + Flags + "}";
        }
    }

    [Flags]
    public enum ConfigStateFlags
    {
        EncryptionEnabled = 1,
        AuthenticationEnabled = 2
    }
}
