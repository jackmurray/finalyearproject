using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
   public class RTPFetchKeyPacket : RTPControlPacket
    {
       public RTPFetchKeyPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource) : base(RTPControlAction.FetchKey, null, SequenceNumber, Timestamp, SyncSource)
       {
       }
    }
}
