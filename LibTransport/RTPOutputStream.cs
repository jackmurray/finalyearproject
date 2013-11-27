using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibAudio;
using LibSecurity;
using LibTrace;

namespace LibTransport
{
    public class RTPOutputStream : RTPStreamBase
    {
        private IAudioFormat audio;
        private ushort seq = 0;
        private long encryption_ctr = 0;
        private uint syncid = (uint)new Random().Next();
        private DateTime basetimestamp;

        public delegate void StreamingCompletedHandler(object sender, EventArgs args);
        public event StreamingCompletedHandler StreamingCompleted;
        private bool continueStreaming = true;

        public RTPOutputStream(IPEndPoint ep, bool useEncryption) : base(ep, useEncryption)
        {
            
        }

        public RTPOutputStream(IPEndPoint ep, bool useEncryption, byte[] key, byte[] nonce) : this(ep, useEncryption)
        {
            if (!useEncryption)
                throw new ArgumentException("This constructor must have useEncryption set to true.");

            this.key = key;
            this.nonce = nonce;
            this.crypto = new PacketEncrypter(key, encryption_ctr, nonce, true);
        }

        public void Send(RTPPacket p)
        {
            byte[] data = useEncryption == false ? p.Serialise() : p.SerialiseEncrypted(crypto, encryption_ctr);
            c.Send(data, data.Length, ep);
            encryption_ctr += p.GetCounterIncrement(crypto);
            this.crypto.Init(this.key, encryption_ctr, nonce, true);
        }

        protected RTPPacket BuildPacket(byte[] data)
        {
            uint ts = this.nextTimestamp(); //get the timestamp before incrementing seq, so that the first timestamp will be basetime+0.
            ushort sequence = ++this.seq;
            //Log.Verbose("Building RTP packet seq:" + sequence);
            return new RTPDataPacket(false, false, sequence, ts, this.syncid, data, null);
        }

        protected RTPPacket BuildPlayPacket()
        {
            DateTime now = DateTime.UtcNow;
            this.basetimestamp = now.AddSeconds(LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME));
            return RTPControlPacket.BuildPlayPacket(++this.seq, RTPPacket.BuildTimestamp(this.basetimestamp), syncid);
        }

        protected RTPPacket BuildStopPacket()
        {
            return RTPControlPacket.BuildStopPacket(++this.seq, this.nextTimestamp(), this.syncid);
        }

        protected uint nextTimestamp()
        {
            DateTime packetdt = basetimestamp.AddMilliseconds(seq*audio.GetFrameLength()*1000);
            //Log.Verbose("Packet timestamp: " + packetdt + ":"+packetdt.Millisecond);
            return RTPPacket.BuildTimestamp(packetdt);
        }

        public void Stream(IAudioFormat audio)
        {
            this.audio = audio;
            this.Send(this.BuildPlayPacket());
            Log.Verbose("Base timestamp: " + basetimestamp + ":" + basetimestamp.Millisecond);
            new Thread(StreamThreadProc).Start();
        }

        private void StreamThreadProc()
        {
            int timerinterval = (int)Math.Truncate(1000 * audio.GetFrameLength()); //truncate to 3 decimal places.

            while (continueStreaming)
            {
                uint startTicks;
                int elapsedTicks, remainingTicks;
                startTicks = (uint) Environment.TickCount;
                TimerTick();
                elapsedTicks = (int)((uint)Environment.TickCount - startTicks);
                remainingTicks = timerinterval - elapsedTicks;
                if (remainingTicks > 0) Thread.Sleep(remainingTicks);
            }
        }

        private void TimerTick()
        {
            this.Send(this.BuildPacket(audio.GetFrame()));
            if (audio.EndOfFile())
            {
                this.Send(this.BuildStopPacket());
                continueStreaming = false;
                if (this.StreamingCompleted != null)
                    StreamingCompleted(this, null);
            }
        }

        public void Stop()
        {
            this.continueStreaming = false;
        }
    }
}
