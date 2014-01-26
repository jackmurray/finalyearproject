using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibAudio;

namespace LibTransport
{
    public class RTPPlayPacket : RTPTimestampPacket
    {
        public DateTime baseTime;
        public ushort SamplesPerFrame;
        public ushort Frequency;
        public byte Channels;
        public byte BitsPerSample;
        public SupportedFormats Format;

        public RTPPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime baseTime, ushort SamplesPerFrame, ushort Frequency, byte Channels, byte BitsPerSample, SupportedFormats Format) :
            base(RTPControlAction.Play, SequenceNumber, Timestamp, SyncSource, baseTime, EncodeData(SamplesPerFrame, Frequency, Channels, BitsPerSample, Format))
        {
            this.baseTime = baseTime;
            this.SamplesPerFrame = SamplesPerFrame;
            this.Frequency = Frequency;
            this.Channels = Channels;
            this.BitsPerSample = BitsPerSample;
            this.Format = Format;
        }

        private static byte[] EncodeData(ushort samples, ushort freq, byte channels, byte BitsPerSample, SupportedFormats Format)
        {
            MemoryStream ms = new MemoryStream();

            byte[] samplesenc = LibUtil.Util.Encode(samples);
            ms.Write(samplesenc, 0, samplesenc.Length);

            byte[] freqenc = LibUtil.Util.Encode(freq);
            ms.Write(freqenc, 0, freqenc.Length);

            ms.Write(new byte[] {channels}, 0, 1);
            ms.Write(new byte[] {BitsPerSample}, 0, 1);
            ms.Write(new byte[] {(byte) Format}, 0, 1);

            return ms.ToArray();
        }
    }
}
