using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUtil;

namespace LibTransport
{
    public class RTPTimestampPacket : RTPControlPacket
    {
        public DateTime Time { get; protected set; }

        public RTPTimestampPacket(RTPControlAction action, ushort SequenceNumber, uint Timestamp, uint SyncSource,
                                  DateTime time, byte[] Payload)
            : base(action, JoinPayload(time, Payload), SequenceNumber, Timestamp, SyncSource)
        {
            this.Time = time;
        }

        private static byte[] JoinPayload(DateTime time, byte[] Payload)
        {
            byte[] data = Payload != null ? new byte[Payload.Length + sizeof (long)] : new byte[sizeof (long)];
            byte[] ticks = Util.Encode(time.Ticks);

            Array.Copy(ticks, data, ticks.Length);

            if (Payload != null)
                Array.Copy(Payload, 0, data, sizeof (long), Payload.Length);

            return data;
        }

        public static DateTime ComputeBaseTime(byte[] extraData)
        {
            /*DateTime sendingTimestamp = new DateTime(LibUtil.Util.DecodeLong(ExtraData, 0));
            //Latency code disabled for now. Might need it in future if sync is an issue.
            //TimeSpan latency = DateTime.UtcNow - sendingTimestamp; //time it took for the packet to get here.

            //LibTrace.Trace.GetInstance("LibTransport").Verbose("Calculated play packet latency as " + latency.TotalMilliseconds + "ms");

            DateTime basetime = RTPPacket.BuildDateTime(this.Timestamp, sendingTimestamp)/* - latency;*/
            return new DateTime(Util.DecodeLong(extraData, 0));
        }
    }
}
