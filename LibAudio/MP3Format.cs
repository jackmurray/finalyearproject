using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class MP3Format : AudioFileReader, IAudioFormat
    {
        public MP3Format(Stream s)
            : base(s)
        {

        }

        public int BitRate { get; private set; }
        public int Frequency { get; private set; }

        private const int MP3_HEADER_SIZE = 4;
        private readonly int[] FrequencyLookup = {44100, 48000, 32000};

        private readonly int[] BitRateLookup =
            {
                0, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000,
                192000, 224000, 256000, 320000
            }; //values start at 1 for some reason

        private bool Padding;

        public int BytesPerFrame
        {
            get
            {
                double val = 144*((double) BitRate/Frequency);
                    //need to force floating point math because we need the intermediate decimal
                return (int) (val + (Padding ? 1 : 0));
            }
        }

        /// <summary>
        /// Parses the input stream and extracts data from it.
        /// </summary>
        /// <exception cref="FormatException">For invalid header formats.</exception>
        public void Parse()
        {
            if (EndOfFile())
                return;
            if (!CheckMagicAndEat())
                throw new FormatException("Expected MP3 header but didn't get one!");

            byte b = Read(1)[0];
            this.BitRate = BitRateLookup[(b & 0xF0) >> 4];
                //Mask out the bits we want, and shift them over so we get the 'real' value (as if we'd just read that value and not masked it out).

            this.Frequency = FrequencyLookup[(b & 0x0C) >> 2];
            this.Padding = ((b & 0x02) >> 1) == 1;

            this.SkipBack(3); //we don't read the last byte of the header, so only skip back 3 not 4.
        }

        public bool CheckMagic()
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

        public void EatGarbageData() //this will fail horribly if used on a non-mp3 stream.
        {
            while (!EndOfFile() && !CheckMagic())
            {
                Skip(1);
            }
        }

        private bool CheckMagicAndEat()
        {
            bool res = CheckMagic();
            if (res)
                Skip(2);
            return res;
        }

        public byte[] GetFrame()
        {
            byte[] buf = this.Read(this.BytesPerFrame);

            if (!this.EndOfFile())
            {
                this.EatGarbageData();
                    //there may or may not be garbage data so we'll chomp 0+ bytes until we hit a header.
                this.Parse(); //read the next header in case it's got a different padding setting.
            }

            return buf;
        }

        /*public Tuple<float, byte[]> GetDataForTime(float time)
        {
            //assume that the sample frequency never changes in the file. should always be the case.
            float framesPerSec = 1/this.GetFrameLength();
            int numFrames = (int)(time * framesPerSec);
            if (numFrames == 0) numFrames = 1; //always read at least one frame.
                //truncate, so we will probably be a little bit under what was asked for.
            MemoryStream ms = new MemoryStream();
                //do it this way so that we can handle variable bitrate frames where we can't math the size ahead of time.
            int i;
            for (i = 0; i < numFrames; i++)
            {
                byte[] temp = GetFrame();
                ms.Write(temp, 0, temp.Length);
                if (EndOfFile())
                    break;
            }
            float timeRead = numFrames * (i / numFrames) * 1/framesPerSec; //numFrames * fraction that we provided * secsPerFrame = total time read
            _trace.Verbose(time + " seconds asked for = " + numFrames + " MP3 frames. Giving " + timeRead);
            return new Tuple<float, byte[]>(timeRead, ms.ToArray());
        }*/

        public float GetFrameLength()
        {
            //assume that the sample frequency never changes in the file. should always be the case.
            return 1152 / (float)this.Frequency;
        }

        public void SeekToStart()
        {
            this._s.Position = 0;
            this.EatGarbageData();
            this.Parse();
        }
    }
}
