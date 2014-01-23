using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibConfig;

namespace LibAudio
{
    public class WAVFormat : IAudioFormat
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
        private AudioReaderBase s;

        public WAVFormat(AudioReaderBase s)
        {
            this.s = s;
        }

        public void Parse()
        {
            //we start out with the stream position ready to read the first byte after the RIFF header.
            //hopefully that's the FMT header, but if not just read until we get it.

            while (!s.CheckBytes(MAGIC_FORMAT))
            {
                if (s.EndOfFile())
                    throw new FormatException("Failure parsing WAV. Hit EOF while searching for FORMAT header.");
                s.Skip(1);
            }

            using (BinaryReader bin = new BinaryReader(s.InnerStream, Encoding.ASCII, true))
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
            while (!s.CheckBytes(MAGIC_DATA))
            {
                if (s.EndOfFile())
                    throw new FormatException("Failure parsing WAV. Hit EOF while searching for DATA header.");
                s.Skip(1);
            }
            s.Skip(MAGIC_DATA.Length);
            s.Skip(4); //after this the stream points at the start of the data.
            this.DataStartPos = (int)s.Position;

            SamplesPerFrame = (ushort) (FRAME_LENGTH_TARGET/(FmtHeader.BitsPerSample/8));
            ActualFrameLength = (ushort)(SamplesPerFrame * (FmtHeader.BitsPerSample / 8));

            var Log = LibTrace.Trace.GetInstance("LibAudio");
            Log.Verbose(String.Format("Audio format: {0}-bit WAV {1}kHz {2}CH.", FmtHeader.BitsPerSample, FmtHeader.SampleRate, FmtHeader.NumChannels));
            Log.Verbose(String.Format("{0} samples/frame ({1} bytes). Duration {2}ms", SamplesPerFrame, ActualFrameLength, GetFrameLength()));
            Log.Verbose(String.Format("Target buffer size: {0} packets.",
                                          Config.GetInt(Config.STREAM_BUFFER_TIME) / GetFrameLength()));
        }

        /// <summary>
        /// Check for the presence of the RIFF and WAVE magic values. If these are present then the file
        /// *is* a WAV. If the other headers aren't there (which we check in Parse()) then it's just a corrupted
        /// one, but still technically a WAV.
        /// </summary>
        /// <returns></returns>
        public bool CheckMagic()
        {
            if (!s.CheckBytes(MAGIC_RIFF))
                return false;
            s.Skip(8); //skip the magic we just read, and the next field (file size) that we don't care about
            if (!s.CheckBytes(MAGIC_WAVE))
                return false;
            s.Skip(4); //at this point we know it's a WAV and that Parse() will get called next, so push the
            //position forward past the RIFF header ready to work on the others.
            return true;
        }

        public byte[] GetFrame()
        {
            return s.Read(ActualFrameLength);
        }

        public double GetFrameLength()
        {
            return ((double) SamplesPerFrame/FmtHeader.SampleRate/FmtHeader.NumChannels)*1000;
        }

        public bool EndOfFile()
        {
            return s.EndOfFile();
        }

        public void SeekToStart()
        {
            s.Reset();
            this.Parse();
        }

        public byte[] GetHeader()
        {
            s.Reset();
            byte[] header = s.Read(DataStartPos);
            return header;
        }

        public int Frequency { get { return (int) FmtHeader.SampleRate; } }
        public byte Channels { get { return (byte)FmtHeader.NumChannels; } }
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
