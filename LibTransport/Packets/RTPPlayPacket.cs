using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPPlayPacket : RTPControlPacket
    {
        public DateTime baseTime;

        public RTPPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime baseTime) : base(RTPControlAction.Play, EncodeTimestamp(baseTime), SequenceNumber, Timestamp, SyncSource)
        {
            this.baseTime = baseTime;
        }

        private static byte[] EncodeTimestamp(DateTime timestamp)
        {
            return LibUtil.Util.Encode(timestamp.Ticks);
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
