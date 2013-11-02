using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibAudio;
using LibTrace;

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
        private static Trace Log = Trace.GetInstance("LibTransport");

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
            uint ts = this.nextTimestamp(); //get the timestamp before incrementing seq, so that the first timestamp will be basetime+0.
            ushort sequence = ++this.seq;
            Log.Verbose("Building RTP packet seq:" + sequence);
            return new RTPDataPacket(false, sequence, ts, this.syncid, data);
        }

        protected uint nextTimestamp()
        {
            DateTime packetdt = basetimestamp.AddMilliseconds(seq*audio.GetFrameLength());
            Log.Verbose("Packet timestamp: " + packetdt + ":"+packetdt.Millisecond);
            return RTPPacket.BuildTimestamp(packetdt);
        }

        public void Stream(IAudioFormat audio)
        {
            this.audio = audio;
            this.basetimestamp = DateTime.UtcNow.AddSeconds(LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME));
            Log.Verbose("Base timestamp: " + basetimestamp + ":" + basetimestamp.Millisecond);
            this.Send(this.BuildPacket(audio.GetFrame()));
        }
    }
}
