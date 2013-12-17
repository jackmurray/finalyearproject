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
        private ushort deltaSeq = 0; //deltaSeq is used to count the number of packets which shouldn't consume a timestamp interval (e.g. rotatekey packets)
        private long encryption_ctr = 0;
        private uint syncid = (uint)new Random().Next();
        private DateTime basetimestamp;
        private Signer signer = null;

        public delegate void StreamingCompletedHandler(object sender, EventArgs args);
        public event StreamingCompletedHandler StreamingCompleted;
        private bool continueStreaming = true;

        private bool rotateKeyRequested = false; //Has a rotation been requested?
        private bool rotateKeyWaiting = false; //Are we now waiting until the correct time to perform the rotation?
        private DateTime rotateKeyTime;

        public RTPOutputStream(IPEndPoint ep) : base(ep)
        {
            
        }

        public override void EnableEncryption(PacketEncrypterKeyManager pekm)
        {
            base.EnableEncryption(pekm);
            this.crypto = new PacketEncrypter(pekm, encryption_ctr, true);
        }

        public void EnableAuthentication(Signer s)
        {
            this.signer = s;
            this.useAuthentication = true;
        }

        /// <summary>
        /// Request a key rotation as soon as possible. To avoid locks, the time this occurs is not guaranteed, but will not take more than 2 packets.
        /// </summary>
        public void RotateKey()
        {
            rotateKeyRequested = true;
        }

        public void Send(RTPPacket p)
        {
            byte[] data = useEncryption == false ? p.Serialise() : p.SerialiseEncrypted(crypto, encryption_ctr, signer);
            c.Send(data, data.Length, ep);
            encryption_ctr += p.GetCounterIncrement(crypto);
            InitKey();
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

        protected void InitKey()
        {
            this.crypto.Init(pekm, encryption_ctr, true);
        }

        /// <summary>
        /// Returns a new RotateKey packet, and the time at which it is to be actioned.
        /// </summary>
        /// <returns></returns>
        protected Tuple<DateTime, RTPPacket> BuildRotateKeyPacket()
        {
            this.deltaSeq++;
            DateTime calculatedTime = this.nextTimestampAsDT();
            int configbuftime = LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME);
            int rotateKeyTime = LibConfig.Config.GetInt(LibConfig.Config.ROTATE_KEY_TIME);

            int delta = (configbuftime < rotateKeyTime) ? rotateKeyTime - configbuftime : 0;
            DateTime actualTime = calculatedTime.AddSeconds(delta);
            Log.Verbose("Built RotateKey packet for time " + actualTime + ":" + actualTime.Millisecond);
            return new Tuple<DateTime,RTPPacket>(actualTime, RTPControlPacket.BuildRotateKeyPacket(++this.seq, RTPPacket.BuildTimestamp(actualTime), this.syncid));
        }

        protected DateTime nextTimestampAsDT()
        {
            return basetimestamp.AddMilliseconds((seq - deltaSeq) * audio.GetFrameLength() * 1000); ;
        }

        protected uint nextTimestamp()
        {
            DateTime packetdt = nextTimestampAsDT();
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
            this.processPendingEvents();

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

        private void processPendingEvents()
        {
            if (this.rotateKeyRequested)
            {
                Log.Verbose("RTPOutputStream: RotateKey requested.");
                rotateKeyRequested = false;
                rotateKeyWaiting = true;
                var r = BuildRotateKeyPacket();
                this.rotateKeyTime = r.Item1;
                this.pekm.SetNextKey(this.pekm.GenerateNewKey(), this.pekm.GenerateNewNonce());
                this.Send(r.Item2);
            }
            if (this.rotateKeyWaiting)
            {
                if (this.rotateKeyTime <= DateTime.UtcNow)
                {
                    Log.Verbose("RTPOutputStream: Rotating key. The first seq to use the new key is " + (this.seq + 1));
                    this.pekm.UseNextKey();
                    InitKey();
                    this.rotateKeyWaiting = false;
                }
            }
        }
    }
}
