using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class MP3Format : AudioFileReader
    {
        public MP3Format(Stream s)
            : base(s)
        {

        }

        /// <summary>
        /// Parses the input stream and extracts data from it.
        /// </summary>
        /// <exception cref="FormatException">For invalid header formats.</exception>
        public override void Parse()
        {
            if (!CheckMagicAndEat())
                throw new FormatException("Expected MP3 header but didn't get one!");

            byte b = Read(1)[0];
            int bitrate = (b & 0xF0) >> 4;
                //Mask out the bits we want, and shift them over so we get the 'real' value (as if we'd just read that value and not masked it out).
            if (!Enum.TryParse(bitrate.ToString(), out BitRate))
                throw new FormatException("Invalid bitrate in input.");
            int freq = (b & 0x0C) >> 2;
            if (!Enum.TryParse(freq.ToString(), out Frequency))
                throw new FormatException("Invalid frequency in input.");

        }

        public override bool CheckMagic()
        {
            byte[] MAGIC_1 = new byte[] {0xFF, 0xFB};
                //The 12 '1' bits (FFF) are the MP3 sync bits, and the B means MPEG-1 Layer 3 no error protection
            byte[] MAGIC_2 = new byte[] {0xFF, 0xFA}; //A = with error protection

            if (CheckBytes(MAGIC_1))
                return true;
            else if (CheckBytes(MAGIC_2))
                return true;
            else return false;
        }

        private bool CheckMagicAndEat()
        {
            bool res = CheckMagic();
            if (res)
                Skip(2);
            return res;
        }
    }
}
