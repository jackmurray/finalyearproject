using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPDataPacket : RTPPacket
    {
        public RTPDataPacket(bool Padding, ushort SequenceNumber, uint Timestamp, uint SyncSource, byte[] Payload)
            : base(Padding, false, SequenceNumber, Timestamp, SyncSource, Payload)
        {
            
        }
    }
}
