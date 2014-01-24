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

        public RTPPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime baseTime, ushort SamplesPerFrame) : base(RTPControlAction.Play, EncodeData(baseTime, SamplesPerFrame), SequenceNumber, Timestamp, SyncSource)
        {
            this.baseTime = baseTime;
            this.SamplesPerFrame = SamplesPerFrame;
        }

        private static byte[] EncodeData(DateTime timestamp, ushort samples)
        {
            MemoryStream ms = new MemoryStream();
            byte[] ticks = LibUtil.Util.Encode(timestamp.Ticks);
            ms.Write(ticks, 0, ticks.Length);

            byte[] samplesenc = LibUtil.Util.Encode(samples);
            ms.Write(samplesenc, 0, samplesenc.Length);

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
