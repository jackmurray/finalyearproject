using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPPlayPacket : RTPTimestampPacket
    {
        public DateTime baseTime;
        public ushort SamplesPerFrame;
        public ushort Frequency;
        public byte Channels;

        public RTPPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime baseTime, ushort SamplesPerFrame, ushort Frequency, byte Channels) :
            base(RTPControlAction.Play, SequenceNumber, Timestamp, SyncSource, baseTime, EncodeData(SamplesPerFrame, Frequency, Channels))
        {
            this.baseTime = baseTime;
            this.SamplesPerFrame = SamplesPerFrame;
            this.Frequency = Frequency;
            this.Channels = Channels;
        }

        private static byte[] EncodeData(ushort samples, ushort freq, byte channels)
        {
            MemoryStream ms = new MemoryStream();

            byte[] samplesenc = LibUtil.Util.Encode(samples);
            ms.Write(samplesenc, 0, samplesenc.Length);

            byte[] freqenc = LibUtil.Util.Encode(freq);
            ms.Write(freqenc, 0, freqenc.Length);

            ms.Write(new byte[] {channels}, 0, 1);

            return ms.ToArray();
        }
    }
}
