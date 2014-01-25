using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibTransport
{
    class RTPSyncPacket : RTPTimestampPacket
    {
        public RTPSyncPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, DateTime dt)
            : base(RTPControlAction.Sync, SequenceNumber, Timestamp, SyncSource, dt, null)
        {
        }
    }
}
