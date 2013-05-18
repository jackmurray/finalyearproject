using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class MP3Format : AudioFileReader, IAudioFormat
    {
        public static readonly byte[] MAGIC = {};

        public MP3Format(Stream s) : base(s)
        {

        }

        /// <summary>
        /// Parses the input stream and extracts data from it.
        /// </summary>
        /// <exception cref="FormatException">For invalid header formats.</exception>
        public override void Parse()
        {
            HeaderType t = DetermineType();
            if (t == HeaderType.ID3)
            {
                Reset();
                ID3Tag tag = new ID3Tag(_s);
                tag.Parse();
                Skip(tag.Size); //Skip over the rest of the ID3 header. We should now be pointing to the MP3 header, so we'll go ahead and parse that now.
            }
            else if (t == HeaderType.MP3)
            {

            }
            else return; //DetermineType() will have thrown an exception anyway.
        }

        private HeaderType DetermineType()
        {
            if (CheckMagic(ID3Tag.MAGIC)) //We have an ID3 header.
                return HeaderType.ID3;

            Reset(); //Go back to the start of the stream so we can try again for the MP3 header.

            if (CheckMagic(MAGIC)) //We've got an MP3 header.
                return HeaderType.MP3;

            throw new FormatException("Unsupported file type.");
        }
    }

    public enum HeaderType
    {
        ID3,
        MP3
    };
}
