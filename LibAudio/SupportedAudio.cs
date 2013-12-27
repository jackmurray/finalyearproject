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
        public static IAudioFormat FindReaderForFile(Stream s)
        {
            ID3Tag id3 = new ID3Tag(s);
            if (id3.CheckMagic())
            {
                id3.Parse();
                id3.Skip((int)id3.Size);
            }
            long pos = id3.Position; //save this position so we can jump back here if an attempt fails.
            
            //Do WAV first because it has a definite format. MP3 you just have to scan for the sync bytes
            //because there can be garbage data after the ID3 header. Scan far enough in a non-MP3 file and
            //you can find random sync bytes which confuses everything.

            WAVFormat wav = TestSimple<WAVFormat>(s);
            if (wav != null) return wav;

            s.Seek(pos, SeekOrigin.Begin);
            MP3Format mp3 = TestMP3(s);
            if (mp3 != null)
            {
                mp3.Parse(); return mp3;
            }

            return null;
        }

        private static MP3Format TestMP3(Stream s)
        {
            var mp3 = new MP3Format(s);
            mp3.EatGarbageData(32 * 1024); //scan the first 32K of the file. after that then give up.
            if (mp3.CheckMagic())
                return mp3;
            else return null;
        }

        private static T TestSimple<T>(Stream s) where T : class, IAudioFormat
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
