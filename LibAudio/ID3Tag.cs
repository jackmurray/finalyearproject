using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class ID3Tag : AudioFileReader
    {
        public static readonly byte[] MAGIC = new byte[] {0x49, 0x44, 0x33}; //ASCII string 'ID3'

        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public byte Flags { get; set; }
        public uint Size { get; set; }


        public ID3Tag(Stream s) : base(s)
        {

        }

        public void Parse()
        {
            if (!CheckMagic())
                throw new FormatException("Tried to parse a stream that didn't have an ID3 header!");
            Skip(MAGIC.Length);
            MajorVersion = Read(1)[0];
            MinorVersion = Read(1)[0];
            Flags = Read(1)[0];
            Size = ID3SizeFieldToInt(Read(4));
        }

        public bool CheckMagic()
        {
            return CheckBytes(MAGIC);
        }

        /// <summary>
        /// Converts an ID3 encoded size field to a uint. Note that the ID3v2 spec says that the MSB of each byte must be set to zero.
        /// We're going to assume that this is the case and not check for ourselves.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private uint ID3SizeFieldToInt(byte[] data)
        {
            if (data.Length != 4)
                throw new FormatException("Invalid ID3 size field length.");
            uint size = 0;
            size += data[3]; //The least significant byte can be added as-is.
            size += (uint)data[2] << 7; //For the rest of the bytes, we shift them left 7n (n=byte position) to get their 'true' decimal value
                                        //(because they're 7-bit bytes, not 8) and accumulate them.
            size += (uint)data[1] << 14;
            size += (uint)data[0] << 21;

            return size;
        }
    }
}
