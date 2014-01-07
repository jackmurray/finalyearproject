using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTransport
{
    class RTPHeaderSyncPacket : RTPControlPacket
    {
        public RTPHeaderSyncPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, byte[] AudioHeader) : base(RTPControlAction.HeaderSync, AudioHeader, SequenceNumber, Timestamp, SyncSource)
        {
        }
    }
}
