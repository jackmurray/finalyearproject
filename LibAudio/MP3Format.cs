using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class MP3Format : IAudioFormat
    {
        private AudioReaderBase s;

        public MP3Format(AudioReaderBase s)
        {
            this.s = s;
        }

        public int BitRate { get; private set; }
        public int Frequency { get; private set; }
        public byte Channels { get { return 2; } } //TODO: this properly

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

            byte b = s.Read(1)[0];
            this.BitRate = BitRateLookup[(b & 0xF0) >> 4];
                //Mask out the bits we want, and shift them over so we get the 'real' value (as if we'd just read that value and not masked it out).

            this.Frequency = FrequencyLookup[(b & 0x0C) >> 2];
            this.Padding = ((b & 0x02) >> 1) == 1;

            s.SkipBack(3); //we don't read the last byte of the header, so only skip back 3 not 4.
        }

        public bool CheckMagic()
        {
            byte[] MAGIC_1 = new byte[] {0xFF, 0xFB};
            //The 12 '1' bits (FFF) are the MP3 sync bits, and the B means MPEG-1 Layer 3 no error protection
            byte[] MAGIC_2 = new byte[] {0xFF, 0xFA}; //A = with error protection

            if (s.CheckBytes(MAGIC_1))
                return true;
            else if (s.CheckBytes(MAGIC_2))
                return true;
            else return false;
        }

        public void EatGarbageData(uint limit = 0) //this will fail horribly if used on a non-mp3 stream.
        {
            uint count = 0;
            while (!EndOfFile() && !CheckMagic())
            {
                s.Skip(1);
                count++;
                if (count == limit)
                    return;
            }
        }

        private bool CheckMagicAndEat()
        {
            bool res = CheckMagic();
            if (res)
                s.Skip(2);
            return res;
        }

        public byte[] GetFrame()
        {
            byte[] buf = s.Read(this.BytesPerFrame);

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

        public double GetFrameLength()
        {
            //assume that the sample frequency never changes in the file. should always be the case.
            return (1152/(double) this.Frequency)*1000;
        }

        public bool EndOfFile()
        {
            return s.EndOfFile();
        }

        public void SeekToStart()
        {
            this.s.Reset();
            this.EatGarbageData();
            this.Parse();
        }

        public byte[] GetHeader()
        {
            return new byte[0];
        }
    }
}
