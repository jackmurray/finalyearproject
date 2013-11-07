using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LibConfig;

namespace LibAudio
{
    public class AudioPlayer
    {
        private Process p = null;

        public AudioPlayer()
        {
            Reset();
        }

        public void Reset()
        {
            if (p != null)
            {
                p.Kill();
            }

            string program = Config.Get(Config.PLAYER_EXECUTABLE);
            string args = Config.Exists(Config.PLAYER_ARGUMENTS) ? Config.Get(Config.PLAYER_ARGUMENTS) : "";

            p = new Process
                {
                    StartInfo =
                        {
                            FileName = program,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardInput = true
                        }
                };
            p.Start();
        }

        public void Write(byte[] data)
        {
            p.StandardInput.BaseStream.Write(data, 0, data.Length);
        }
    }
}
