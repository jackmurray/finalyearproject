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

        public int BitRate;
        public int Frequency;

        protected AudioFileReader(Stream s)
        {
            _s = s;
        }

        public abstract void Parse();
        public abstract bool CheckMagic();

        protected byte[] Read(int numBytes)
        {
            byte[] ret = new byte[numBytes];
            _s.Read(ret, 0, numBytes);
            return ret;
        }

        public void Reset()
        {
            _s.Position = 0;
        }

        protected bool CheckBytes(byte[] expect)
        {
            byte[] read = Read(expect.Length);
            this.SkipBack(expect.Length);
            if (expect.SequenceEqual(read))
                return true;
            else return false;
        }

        public void Skip(int bytes)
        {
            _s.Seek(bytes, SeekOrigin.Current);
        }

        public void SkipBack(int bytes)
        {
            _s.Seek(-bytes, SeekOrigin.Current);
        }
    }
}
