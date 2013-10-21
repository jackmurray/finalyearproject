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
        /// Returns an already-parsed AudioFileReader object that will read the given stream. If no suitable object can be created,
        /// null will be returned.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static AudioFileReader FindReaderForFile(Stream s)
        {
            AudioFileReader r = new ID3Tag(s);
            if (r.CheckMagic())
            {
                r.Parse();
                r.Skip((int)(r as ID3Tag).Size);
            }
            long pos = r.Position; //save this position so we can jump back here if an attempt fails.

            MP3Format mp3 = new MP3Format(s);
            mp3.EatGarbageData();
            if (!mp3.CheckMagic())
                return null;
            r = mp3;

            r.Parse();
            return r;
        }
    }
}
