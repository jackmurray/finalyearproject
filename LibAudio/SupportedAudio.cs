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

            r = new MP3Format(s);
            if (!r.CheckMagic())
                return null;

            r.Parse();
            return r;
        }
    }
}
