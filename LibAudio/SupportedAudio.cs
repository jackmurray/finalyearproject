using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public static class SupportedAudio
    {
        /// <summary>
        /// Returns an already-parsed IAudioFormat object that will read the given stream. If no suitable object can be created,
        /// null will be returned.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IAudioFormat FindReaderForFile(AudioReaderBase s)
        {
            var Log = LibTrace.Trace.GetInstance("LibAudio");
            ID3Tag id3 = new ID3Tag(s);
            if (id3.CheckMagic())
            {
                id3.Parse();
                s.Skip((int)id3.Size);
            }
            long pos = s.Position; //save this position so we can jump back here if an attempt fails.
            
            //Do WAV first because it has a definite format. MP3 you just have to scan for the sync bytes
            //because there can be garbage data after the ID3 header. Scan far enough in a non-MP3 file and
            //you can find random sync bytes which confuses everything.

            WAVFormat wav = TestSimple<WAVFormat>(s);
            if (wav != null) return wav;

            s.Position = pos;
            MP3Format mp3 = TestMP3(s);
            if (mp3 != null)
            {
                mp3.Parse();
                Log.Verbose("Detected format: MP3. The following format info will only be correct for CBR files.");
                Log.Verbose(String.Format("Audio format: MP3 {0}kbit {1}kHz.", mp3.BitRate, mp3.Frequency));
                Log.Verbose(String.Format("{0} bytes/frame. Duration {1}sec", mp3.BytesPerFrame, mp3.GetFrameLength()));
                return mp3;
            }

            return null;
        }

        private static MP3Format TestMP3(AudioReaderBase s)
        {
            var mp3 = new MP3Format(s);
            mp3.EatGarbageData(32 * 1024); //scan the first 32K of the file. after that then give up.
            if (mp3.CheckMagic())
                return mp3;
            else return null;
        }

        private static T TestSimple<T>(AudioReaderBase s) where T : class, IAudioFormat
        {
            var obj = (T)Activator.CreateInstance(typeof (T), s);
            if (obj.CheckMagic())
            {
                obj.Parse();
                return obj;
            }
            else return null;
        }
    }
}
