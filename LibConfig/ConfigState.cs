using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibConfig
{
    public class ConfigState
    {
        public ConfigStateFlags Flags { get; protected set; }

        protected ConfigState(ConfigStateFlags f)
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
    }

    [Flags]
    public enum ConfigStateFlags
    {
        EncryptionEnabled = 1,
        AuthenticationEnabled = 2
    }
}
