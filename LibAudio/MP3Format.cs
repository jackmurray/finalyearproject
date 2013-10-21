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

        private int[] FrequencyLookup = {44100, 48000, 32000};

        private bool Padding;

        private int[] BitRateLookup =
            {
                0, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000,
                192000, 224000, 256000, 320000
            }; //values start at 1 for some reason

        public int BytesPerFrame {
            get { double val = 144 * ((double)BitRate / Frequency); //need to force floating point math because we need the intermediate decimal
                return (int)(val + (Padding ? 1 : 0)); 
            }
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
            this.BitRate = BitRateLookup[(b & 0xF0) >> 4]; //Mask out the bits we want, and shift them over so we get the 'real' value (as if we'd just read that value and not masked it out).

            this.Frequency = FrequencyLookup[(b & 0x0C) >> 2];
            this.Padding = ((b & 0x02) >> 1) == 1;
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
