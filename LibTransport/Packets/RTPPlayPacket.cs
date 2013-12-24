using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPPlayPacket : RTPControlPacket
    {
        public RTPPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime timestamp) : base(RTPControlAction.Play, EncodeTimestamp(timestamp), SequenceNumber, Timestamp, SyncSource)
        {
        }

        private static byte[] EncodeTimestamp(DateTime timestamp)
        {
            return LibUtil.Util.Encode(timestamp.Ticks);
        }
    }
}
