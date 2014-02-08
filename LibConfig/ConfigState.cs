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

        /// <summary>
        /// Check if the given remote ConfigState is compatible with this instance.
        /// </summary>
        /// <param name="s">Remote party's ConfigState</param>
        /// <returns></returns>
        public bool CanAccept(ConfigState s)
        {
            //if we have encryption and they don't, then fail.
            if (Flags.HasFlag(ConfigStateFlags.EncryptionEnabled) && !s.Flags.HasFlag(ConfigStateFlags.EncryptionEnabled))
                return false;

            //if we have auth and they don't, then fail
            if (Flags.HasFlag(ConfigStateFlags.AuthenticationEnabled) && !s.Flags.HasFlag(ConfigStateFlags.AuthenticationEnabled))
                return false;

            return true;
        }
    }

    [Flags]
    public enum ConfigStateFlags
    {
        EncryptionEnabled = 1,
        AuthenticationEnabled = 2
    }
}
