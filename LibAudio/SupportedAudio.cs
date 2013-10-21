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
        public static IAudioFormat FindReaderForFile(Stream s)
        {
            ID3Tag id3 = new ID3Tag(s);
            if (id3.CheckMagic())
            {
                id3.Parse();
                id3.Skip((int)id3.Size);
            }
            long pos = id3.Position; //save this position so we can jump back here if an attempt fails.

            IAudioFormat audio;
            MP3Format mp3 = new MP3Format(s);
            mp3.EatGarbageData();
            if (!mp3.CheckMagic())
                return null;
            audio = mp3;

            audio.Parse();
            return audio;
        }
    }
}
