using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTransport
{
    public class RTPControlPacket : RTPPacket
    {
        public RTPControlAction Action { get; protected set; }
        public byte[] ExtraData { get; protected set; }

        //Call the base constructor with a Payload built from the Action and the ExtraData converted to a new byte[]
        public RTPControlPacket(RTPControlAction Action, byte[] ExtraData, ushort SequenceNumber, uint Timestamp, uint SyncSource) : 
            base(false, true, SequenceNumber, Timestamp, SyncSource, new byte[]{(byte)Action}.Concat(ExtraData ?? new byte[0]).ToArray())
        {
            this.Action = Action;
            this.ExtraData = ExtraData;
        }
    }

    public enum RTPControlAction {Play, Pause, Stop, RotateKey}
}
