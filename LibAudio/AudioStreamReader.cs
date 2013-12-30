using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibAudio
{
    public class AudioStreamReader : AudioReaderBase
    {
        public AudioStreamReader(Stream s) : base(s)
        {
        }

        //In a live stream, there is no end.
        public override bool EndOfFile()
        {
            return false;
        }
    }
}
