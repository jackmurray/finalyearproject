using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
   public class RTPPausePacket : RTPControlPacket
    {
       public RTPPausePacket(ushort SequenceNumber, uint Timestamp, uint SyncSource) : base(RTPControlAction.Pause, null, SequenceNumber, Timestamp, SyncSource)
       {
       }
    }
}
