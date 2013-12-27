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
        private object synclock = new object(); //used so calls from the GUI thread won't mess things up.
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

        public OutputStreamState State { get; protected set; }

        public RTPOutputStream(IPEndPoint ep) : base(ep)
        {
            
        }

        public override void EnableEncryption(PacketEncrypterKeyManager pekm)
        {
            base.EnableEncryption(pekm);
            this.crypto = new PacketEncrypter(pekm, encryption_ctr, true);
        }

        public void EnableSigning(Signer s)
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
            byte[] data;
            if (useEncryption)
            {
                data = p.SerialiseEncrypted(crypto, encryption_ctr, signer);
                encryption_ctr += p.GetCounterIncrement(crypto);
                InitKey();
            }
            else
            {
                data = p.Serialise();
            }

            c.Send(data, data.Length, ep);
        }

        protected RTPPacket BuildPacket(byte[] data)
        {
            uint ts = this.nextTimestamp(); //get the timestamp before incrementing seq, so that the first timestamp will be basetime+0.
            ushort sequence = ++this.seq;
            //Log.Verbose("Building RTP packet seq:" + sequence);
            return new RTPDataPacket(false, sequence, ts, this.syncid, data);
        }
        
        protected void setupBaseTime()
        {
            this.basetimestamp = DateTime.UtcNow.AddSeconds(LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME));
        }

        protected RTPPacket BuildPlayPacket()
        {
            return new RTPPlayPacket(++this.seq, 0, syncid, this.basetimestamp);
        }

        protected RTPPacket BuildStopPacket()
        {
            return new RTPStopPacket(++this.seq, this.nextTimestamp(), this.syncid);
        }

        protected void InitKey()
        {
            this.crypto.Init(pekm, encryption_ctr, true);
        }

        /// <summary>
        /// Returns a new FetchKey packet, and the time at which it is to be actioned.
        /// </summary>
        /// <returns></returns>
        protected Tuple<DateTime, RTPPacket> BuildFetchKeyPacket()
        {
            this.deltaSeq++;
            DateTime calculatedTime = this.nextTimestampAsDT();
            int configbuftime = LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME);
            int rotateKeyTime = LibConfig.Config.GetInt(LibConfig.Config.ROTATE_KEY_TIME);

            int delta = (configbuftime < rotateKeyTime) ? rotateKeyTime - configbuftime : 0;
            DateTime actualTime = calculatedTime.AddSeconds(delta);
            Log.Verbose("Built FetchKey packet for time " + actualTime + ":" + actualTime.Millisecond);
            return new Tuple<DateTime,RTPPacket>(actualTime, new RTPFetchKeyPacket(++this.seq, RTPPacket.BuildTimestamp(actualTime), this.syncid));
        }

        protected RTPPacket BuildSwitchKeyPacket()
        {
            this.deltaSeq++;
            return new RTPSwitchKeyPacket(++this.seq, this.nextTimestamp(), this.syncid);
        }

        protected RTPPacket BuildPausePacket()
        {
            //no need to use deltaSeq here because we're going to reset everything anyway
            return new RTPPausePacket(++this.seq, this.nextTimestamp(), this.syncid);
        }

        protected DateTime nextTimestampAsDT()
        {
            return basetimestamp.AddMilliseconds((seq - deltaSeq) * audio.GetFrameLength() * 1000);
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
            StartStream(true);
        }

        private void StartStream(bool sendFileHeader = false)
        {
            byte[] header = null;
            if (sendFileHeader)
            {
                header = audio.GetHeader();
            }

            this.continueStreaming = true;
            this.setupBaseTime();
            this.Send(this.BuildPlayPacket());
            if (sendFileHeader && header != null && header.Length > 0)
            {
                ++this.deltaSeq;
                this.Send(new RTPDataPacket(false, ++this.seq, this.nextTimestamp(), this.syncid, header));
            }
            Log.Verbose("Base timestamp: " + basetimestamp + ":" + basetimestamp.Millisecond);
            new Thread(StreamThreadProc).Start();
            State = OutputStreamState.Started;
        }

        public void SendSync()
        {
            this.deltaSeq++;
            this.Send(this.BuildPlayPacket());
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
            /*
             * Acquire the lock here rather than in StreamThreadProc. Doing it here means that if we have to wait a few hundred usecs
             * to get the lock, it will be accounted for by the timing code with the result being that we Sleep() for less time.
             */
            lock (synclock)
            {
                this.processPendingEvents();

                this.Send(this.BuildPacket(audio.GetFrame()));
                if (audio.EndOfFile())
                {
                    this.Stop();
                    if (this.StreamingCompleted != null)
                        StreamingCompleted(this, null);
                }
            }
        }

        /// <summary>
        /// Stop the streaming thread and send a Stop packet.
        /// </summary>
        public void Stop()
        {
            lock (synclock)
            {
                Log.Information("Stopping stream.");
                this.continueStreaming = false;
                this.Send(this.BuildStopPacket());
                audio.SeekToStart();
            }
        }

        public void Pause()
        {
            lock (synclock)
            {
                Log.Information("Pausing stream.");
                this.continueStreaming = false;
                this.Send(this.BuildPausePacket());
                State = OutputStreamState.Paused;
            }
        }

        public void Resume()
        {
            lock (synclock)
            {
                Log.Information("Resuming stream.");
                deltaSeq = seq;
                StartStream();
            }
        }

        private void processPendingEvents()
        {
            if (this.rotateKeyRequested)
            {
                Log.Verbose("RTPOutputStream: RotateKey requested.");
                rotateKeyRequested = false;
                rotateKeyWaiting = true;
                var r = BuildFetchKeyPacket();
                this.rotateKeyTime = r.Item1;
                this.pekm.SetNextKey(this.pekm.GenerateNewKey(), this.pekm.GenerateNewNonce());
                this.Send(r.Item2);
            }
            if (this.rotateKeyWaiting)
            {
                if (this.rotateKeyTime <= DateTime.UtcNow)
                {
                    this.Send(this.BuildSwitchKeyPacket());
                    Log.Verbose("RTPOutputStream: Rotating key. The first seq to use the new key is " + (this.seq + 1));
                    this.pekm.UseNextKey();
                    /*
                     * We have to call InitKey() here because although we changed the key in the PEKM via UseNextKey(),
                     * the PacketEncrypter and its underlying crypto engine must be re-initialised with the new key before we try and send anything else
                     * because after this point the receivers will switch to the new key.
                     * */
                    InitKey();
                    this.rotateKeyWaiting = false;
                }
            }
        }
    }

    public enum OutputStreamState {Stopped, Started, Paused}
}
