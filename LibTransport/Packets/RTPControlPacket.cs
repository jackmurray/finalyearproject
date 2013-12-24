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

        public DateTime ComputeBaseTime()
        {
            if (this.Action != RTPControlAction.Play)
                throw new InvalidOperationException("Can only call ComputeBaseTime on a Play packet.");

            /*DateTime sendingTimestamp = new DateTime(LibUtil.Util.DecodeLong(ExtraData, 0));
            //Latency code disabled for now. Might need it in future if sync is an issue.
            //TimeSpan latency = DateTime.UtcNow - sendingTimestamp; //time it took for the packet to get here.

            //LibTrace.Trace.GetInstance("LibTransport").Verbose("Calculated play packet latency as " + latency.TotalMilliseconds + "ms");

            DateTime basetime = RTPPacket.BuildDateTime(this.Timestamp, sendingTimestamp)/* - latency;*/
            return new DateTime(LibUtil.Util.DecodeLong(ExtraData, 0));
        }

        /// <summary>
        /// Build a play packet based at the current UTC time.
        /// </summary>
        /// <returns></returns>
        public static RTPControlPacket BuildPlayPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, long baseTime)
        {
            byte[] extra = LibUtil.Util.Encode(baseTime);
            //extension stuff set to false/null as it'll be set later.
            return new RTPControlPacket(RTPControlAction.Play, extra, SequenceNumber, Timestamp, SyncSource);
        }

        public static RTPControlPacket BuildStopPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource)
        {
            return new RTPControlPacket(RTPControlAction.Stop, null, SequenceNumber, Timestamp, SyncSource);
        }

        public static RTPControlPacket BuildFetchKeyPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource)
        {
            return new RTPControlPacket(RTPControlAction.FetchKey, null, SequenceNumber, Timestamp, SyncSource);
        }

        public static RTPControlPacket BuildSwitchKeyPacket(ushort SequenceNumber, uint Timestamp, uint SyncSource)
        {
            return new RTPControlPacket(RTPControlAction.SwitchKey, null, SequenceNumber, Timestamp, SyncSource);
        }

        public static RTPControlPacket BuildPausePacket(ushort SequenceNumber, uint Timestamp, uint SyncSource)
        {
            return new RTPControlPacket(RTPControlAction.Pause, null, SequenceNumber, Timestamp, SyncSource);
        }
    }

    public enum RTPControlAction {Play, Pause, Stop, FetchKey, SwitchKey}
}
