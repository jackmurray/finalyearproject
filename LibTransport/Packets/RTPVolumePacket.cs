using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUtil;

namespace LibTransport
{
    public class RTPVolumePacket : RTPControlPacket
    {
        public int Volume { get; set; }

        public RTPVolumePacket(ushort SequenceNumber, uint Timestamp, uint SyncSource, int volume)
            : base(RTPControlAction.Volume, Util.Encode(volume), SequenceNumber, Timestamp, SyncSource)
        {
            this.Volume = volume;
        }
    }
}
