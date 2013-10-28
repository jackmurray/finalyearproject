using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibAudio;

namespace LibTransport
{
    public class RTPOutputStream
    {
        private UdpClient c;
        private IPEndPoint ep;
        private IAudioFormat audio;
        private ushort seq = 0;
        private uint syncid = (uint)new Random().Next();
        private DateTime basetimestamp;

        public RTPOutputStream(IPEndPoint ep)
        {
            if (ep.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("IP endpoint must be AF_INET. IPv6 is currently not supported.");
            if (!LibUtil.Util.IsMulticastAddress(ep.Address))
                throw new ArgumentException("IP address must be in the range 224.0.0.0/4 (multicast)");

            c = new UdpClient(AddressFamily.InterNetwork);
            c.JoinMulticastGroup(ep.Address);
            this.ep = ep;
        }

        public void Send(RTPPacket p)
        {
            byte[] data = p.Serialise();
            c.Send(data, data.Length, ep);
        }

        protected RTPPacket BuildPacket(byte[] data)
        {
            return new RTPDataPacket(false, ++this.seq, this.nextTimestamp(), this.syncid, data);
        }

        protected uint nextTimestamp()
        {
            return 0;
        }

        public void Stream(IAudioFormat audio)
        {
            this.audio = audio;
            this.basetimestamp = DateTime.Now.AddSeconds(LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME));
        }
    }
}
