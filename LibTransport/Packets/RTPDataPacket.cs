using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPDataPacket : RTPPacket
    {
        public RTPDataPacket(bool Padding, bool Extension, ushort SequenceNumber, uint Timestamp, uint SyncSource, byte[] Payload, byte[] ExtensionData)
            : base(Padding, Extension, false, SequenceNumber, Timestamp, SyncSource, Payload, ExtensionData)
        {
            
        }
    }
}
