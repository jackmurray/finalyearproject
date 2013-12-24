using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
   public class RTPStopPacket : RTPControlPacket
    {
       public RTPStopPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource) : base(RTPControlAction.Stop, null, SequenceNumber, Timestamp, SyncSource)
       {
       }
    }
}
