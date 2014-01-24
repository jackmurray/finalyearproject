using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPPlayPacket : RTPControlPacket
    {
        public DateTime baseTime;
        public ushort SamplesPerFrame;
        public ushort Frequency;
        public byte Channels;

        public RTPPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime baseTime, ushort SamplesPerFrame, ushort Frequency, byte Channels) :
            base(RTPControlAction.Play, EncodeData(baseTime, SamplesPerFrame, Frequency, Channels), SequenceNumber, Timestamp, SyncSource)
        {
            this.baseTime = baseTime;
            this.SamplesPerFrame = SamplesPerFrame;
            this.Frequency = Frequency;
            this.Channels = Channels;
        }

        private static byte[] EncodeData(DateTime timestamp, ushort samples, ushort freq, byte channels)
        {
            MemoryStream ms = new MemoryStream();
            byte[] ticks = LibUtil.Util.Encode(timestamp.Ticks);
            ms.Write(ticks, 0, ticks.Length);

            byte[] samplesenc = LibUtil.Util.Encode(samples);
            ms.Write(samplesenc, 0, samplesenc.Length);

            byte[] freqenc = LibUtil.Util.Encode(freq);
            ms.Write(freqenc, 0, freqenc.Length);

            ms.Write(new byte[] {channels}, 0, 1);

            return ms.ToArray();
        }

        public static DateTime ComputeBaseTime(byte[] extraData)
        {
            /*DateTime sendingTimestamp = new DateTime(LibUtil.Util.DecodeLong(ExtraData, 0));
            //Latency code disabled for now. Might need it in future if sync is an issue.
            //TimeSpan latency = DateTime.UtcNow - sendingTimestamp; //time it took for the packet to get here.

            //LibTrace.Trace.GetInstance("LibTransport").Verbose("Calculated play packet latency as " + latency.TotalMilliseconds + "ms");

            DateTime basetime = RTPPacket.BuildDateTime(this.Timestamp, sendingTimestamp)/* - latency;*/
            return new DateTime(LibUtil.Util.DecodeLong(extraData, 0));
        }
    }
}
