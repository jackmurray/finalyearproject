using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public class WAVFormat : AudioFileReader, IAudioFormat
    {
        /// <summary>
        /// Since WAV doesn't have frames, we aim to read up to FRAME_LENGTH_TARGET bytes worth of samples
        /// and pretend that that's a frame instead. The actual frame size is determined during Parse()
        /// once we know the Frequency and Sample Size.
        /// </summary>
        private const ushort FRAME_LENGTH_TARGET = 1400;

        private static readonly byte[] MAGIC_RIFF = {0x52, 0x49, 0x46, 0x46}; //ASCII 'RIFF'
        private static readonly byte[] MAGIC_WAVE = {0x57, 0x41, 0x56, 0x45}; //ASCII 'WAVE'
        private static readonly byte[] MAGIC_FORMAT = {0x66, 0x6D, 0x74, 0x20}; //ASCII 'fmt ' (space)
        private static readonly byte[] MAGIC_DATA = {0x64, 0x61, 0x74, 0x61}; //ASCII 'data'

        private WavFmtHeader FmtHeader;
        private ushort ActualFrameLength, SamplesPerFrame;
        private int DataStartPos = 0;

        public WAVFormat(Stream s) : base(s)
        {
        }

        public void Parse()
        {
            //we start out with the stream position ready to read the first byte after the RIFF header.
            //hopefully that's the FMT header, but if not just read until we get it.

            while (!CheckBytes(MAGIC_FORMAT))
            {
                if (EndOfFile())
                    throw new FormatException("Failure parsing WAV. Hit EOF while searching for FORMAT header.");
                Skip(1);
            }

            using (BinaryReader bin = new BinaryReader(_s, Encoding.ASCII, true))
            {
                this.FmtHeader = new WavFmtHeader()
                    {
                        Magic = bin.ReadChars(4),
                        size = bin.ReadUInt32(),
                        Format = bin.ReadUInt16(),
                        NumChannels = bin.ReadUInt16(),
                        SampleRate = bin.ReadUInt32(),
                        ByteRate = bin.ReadUInt32(),
                        BlockAlign = bin.ReadUInt16(),
                        BitsPerSample = bin.ReadUInt16()
                    };
            }

            //Go directly to DATA. Do not parse go (or other structures in the header we don't care about).
            while (!CheckBytes(MAGIC_DATA))
            {
                if (EndOfFile())
                    throw new FormatException("Failure parsing WAV. Hit EOF while searching for DATA header.");
                Skip(1);
            }
            Skip(MAGIC_DATA.Length); 
            Skip(4); //after this the stream points at the start of the data.
            this.DataStartPos = (int)_s.Position;

            SamplesPerFrame = (ushort) (FRAME_LENGTH_TARGET/(FmtHeader.BitsPerSample/8));
            ActualFrameLength = (ushort)(SamplesPerFrame * (FmtHeader.BitsPerSample / 8));
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
            Skip(4); //at this point we know it's a WAV and that Parse() will get called next, so push the
            //position forward past the RIFF header ready to work on the others.
            return true;
        }

        public byte[] GetFrame()
        {
            return Read(ActualFrameLength);
        }

        public float GetFrameLength()
        {
            return SamplesPerFrame/FmtHeader.SampleRate;
        }

        public void SeekToStart()
        {
            this._s.Position = 0;
            this.Parse();
        }

        public byte[] GetHeader()
        {
            _s.Position = 0;
            byte[] header = Read(DataStartPos);
            return header;
        }
    }

    class WavFmtHeader
    {
        //taken from https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
        public char[] Magic;
        public uint size;
        public ushort Format;
        public ushort NumChannels;
        public uint SampleRate;
        public uint ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;
    }
}
