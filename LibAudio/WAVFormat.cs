using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class WAVFormat : AudioFileReader, IAudioFormat
    {
        private static readonly byte[] MAGIC_RIFF = {0x52, 0x49, 0x46, 0x46}; //ASCII 'RIFF'
        private static readonly byte[] MAGIC_WAVE = {0x57, 0x41, 0x56, 0x45}; //ASCII 'WAVE'
        private static readonly byte[] MAGIC_FORMAT = {0x66, 0x6D, 0x74, 0x20}; //ASCII 'fmt ' (space)

        private uint Frequency, BitRate;
        private ushort NumChannels, BitsPerSample;

        public WAVFormat(Stream s) : base(s)
        {
        }

        public void Parse()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check for the presence of the RIFF and WAVE magic values. If these are present then the file
        /// *is* a WAV. If the other headers aren't there (which we check in Parse()) then it's just a corrupted
        /// one, but still technically a WAV.
        /// </summary>
        /// <returns></returns>
        public bool CheckMagic()
        {
            if (!CheckBytes(MAGIC_RIFF))
                return false;
            Skip(8); //skip the magic we just read, and the next field (file size) that we don't care about
            if (!CheckBytes(MAGIC_WAVE))
                return false;

            return true;
        }

        public byte[] GetFrame()
        {
            throw new NotImplementedException();
        }

        public float GetFrameLength()
        {
            throw new NotImplementedException();
        }

        public void SeekToStart()
        {
            throw new NotImplementedException();
        }
    }
}
