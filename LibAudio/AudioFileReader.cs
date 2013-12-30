using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibAudio
{
    public class AudioFileReader : AudioReaderBase
    {
        public AudioFileReader(FileStream s) : base(s)
        {
        }

        public override bool EndOfFile()
        {
            return _s.Position >= _s.Length - 1;
        }
    }
}
