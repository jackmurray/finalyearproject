using System;
using System.Collections.Generic;
using System.IO;
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
            base(false, true, SequenceNumber, Timestamp, SyncSource, EncodePayload(Action, ExtraData))
        {
            this.Action = Action;
            this.ExtraData = ExtraData;
        }

        protected static byte[] EncodePayload(RTPControlAction action, byte[] extraData)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte) action);
            if (extraData != null && extraData.Length > 0)
                ms.Write(extraData, 0, extraData.Length);

            return ms.ToArray();
        }
    }

    public enum RTPControlAction {Play, Pause, Stop, FetchKey, SwitchKey, Sync, Volume}
}
