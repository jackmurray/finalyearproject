﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibTrace;

namespace LibAudio
{
    public abstract class AudioFileReader
    {
        protected Stream _s;
        protected Trace _trace;

        protected AudioFileReader(Stream s)
        {
            _s = s;
            _trace = Trace.GetInstance("LibAudio");
        }

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

        public long Position {
            get { return _s.Position; }
        }

        public bool EndOfFile()
        {
            return _s.Position == _s.Length - 1;
        }
    }
}
