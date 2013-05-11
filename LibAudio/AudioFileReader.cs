using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public abstract class AudioFileReader
    {
        protected Stream _s;

        protected AudioFileReader(Stream s)
        {
            _s = s;
        }

        public abstract void Parse();

        protected byte[] Read(int numBytes)
        {
            byte[] ret = new byte[numBytes];
            _s.Read(ret, 0, numBytes);
            return ret;
        }

        protected void Reset()
        {
            _s.Position = 0;
        }

        protected bool CheckMagic(byte[] magic)
        {
            byte[] read = Read(magic.Length);
            if (magic.SequenceEqual(read))
                return true;
            else return false;
        }
    }
}
