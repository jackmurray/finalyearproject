using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

            p = new Process
                {
                    StartInfo =
                        {
                            FileName = "mpg123",
                            Arguments = "-",
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
