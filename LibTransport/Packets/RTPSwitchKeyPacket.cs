using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
   public class RTPSwitchKeyPacket : RTPControlPacket
    {
       public RTPSwitchKeyPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource) : base(RTPControlAction.SwitchKey, null, SequenceNumber, Timestamp, SyncSource)
       {
       }
    }
}
