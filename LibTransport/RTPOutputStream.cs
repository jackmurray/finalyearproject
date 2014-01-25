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
        private byte[] audioHeader;

        public delegate void StreamingCompletedHandler(object sender, EventArgs args);
        public event StreamingCompletedHandler StreamingCompleted;
        private bool continueStreaming = true;

        private bool rotateKeyRequested = false; //Has a rotation been requested?
        private bool rotateKeyWaiting = false; //Are we now waiting until the correct time to perform the rotation?
        private DateTime rotateKeyTime;

        private int bufferTime = LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME);

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
            this.basetimestamp = DateTime.UtcNow.AddMilliseconds(bufferTime);
        }

        protected RTPPacket BuildPlayPacket()
        {
            return new RTPPlayPacket(++this.seq, 0, syncid, this.basetimestamp, audio.SamplesPerFrame, audio.Frequency, audio.Channels);
        }

        protected RTPPacket BuildHeaderSyncPacket()
        {
            this.deltaSeq++;
            return new RTPHeaderSyncPacket(++this.seq, 0, syncid, this.audioHeader);
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
            DateTime actualTime = calculatedTime.AddMilliseconds(delta);
            Log.Verbose("Built FetchKey packet for time " + actualTime + ":" + actualTime.Millisecond);
            return new Tuple<DateTime,RTPPacket>(actualTime, new RTPFetchKeyPacket(++this.seq, RTPPacket.BuildTimestamp(actualTime, basetimestamp), this.syncid));
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
            return basetimestamp.AddMilliseconds((seq - deltaSeq) * audio.GetFrameLength());
        }

        protected uint nextTimestamp()
        {
            DateTime packetdt = nextTimestampAsDT();
            //Log.Verbose("Packet timestamp: " + packetdt + ":"+packetdt.Millisecond);
            return RTPPacket.BuildTimestamp(packetdt, basetimestamp);
        }

        public void Stream(IAudioFormat audio)
        {
            this.audio = audio;
            this.audioHeader = audio.GetHeader();
            StartStream(true);
        }

        private void StartStream(bool sendFileHeader = false)
        {
            this.continueStreaming = true;
            this.setupBaseTime();
            this.Send(this.BuildPlayPacket());
            if (sendFileHeader && this.audioHeader != null && this.audioHeader.Length > 0)
            {
                ++this.deltaSeq;
                this.Send(new RTPHeaderSyncPacket(++this.seq, 0, this.syncid, this.audioHeader));
                Log.Verbose("Sending audio header as requested.");
            }
            Log.Verbose("Base timestamp: " + basetimestamp + ":" + basetimestamp.Millisecond);
            new Thread(StreamThreadProc).Start();
            State = StreamState.Started;
        }

        public void SendHeaderSync()
        {
            if (this.audioHeader == null || this.audioHeader.Length == 0)
                return; //if we don't have a header (because we don't need one) then don't do anything.

            lock (synclock)
            {
                this.Send(this.BuildHeaderSyncPacket());
            }
        }

        public void SendSync()
        {
            lock (synclock)
            {
                this.deltaSeq++;
                this.Send(this.BuildPlayPacket());
            }
        }

        private void StreamThreadProc()
        {
            double frameLength;
            int timerinterval;

            while (continueStreaming)
            {
                uint startTicks;
                int elapsedTicks, remainingTicks;
                startTicks = (uint) Environment.TickCount;

                frameLength = audio.GetFrameLength(); //convert to milliseconds.
                timerinterval = (int)frameLength; //we have to truncate because you can't sleep for fractional milliseconds.

                RTPPacket p = TimerTick();
                if (p == null)
                    return; //if TimerTick() returns null then we're done streaming.

                elapsedTicks = (int)((uint)Environment.TickCount - startTicks);
                remainingTicks = timerinterval - elapsedTicks;

                TimeSpan diff = RTPPacket.BuildDateTime(p.Timestamp, basetimestamp) - DateTime.UtcNow; //difference between when the packet will be actioned and now.
                int behindms = (int) (bufferTime - diff.TotalMilliseconds); //difference between when the packet *should* be actioned and when it *will* be

                remainingTicks -= behindms; //behindms will be +ve if the packet is late, and -ve if it's early. this means we'll sleep less time if it's late or more time if it's early.

                if (remainingTicks > 0) Thread.Sleep(remainingTicks); //can't sleep for a -ve time. if remainingTicks ends up -ve it means we're late anyway, so we want to loop again immidiately.
                //else Log.Verbose("Not sleeping because remainingTicks=" + remainingTicks);
            }
        }

        private RTPPacket TimerTick()
        {
            /*
             * Acquire the lock here rather than in StreamThreadProc. Doing it here means that if we have to wait a few hundred usecs
             * to get the lock, it will be accounted for by the timing code with the result being that we Sleep() for less time.
             */
            lock (synclock)
            {
                if (!this.continueStreaming)
                    return null; //while we're waiting for the lock, this could have been set to false so check it again once we hold it.
                this.processPendingEvents();

                RTPPacket p = this.BuildPacket(audio.GetFrame());
                
                this.Send(p);
                if (audio.EndOfFile())
                {
                    this.Stop();
                    if (this.StreamingCompleted != null)
                        StreamingCompleted(this, null);
                }

                return p;
            }
        }

        /// <summary>
        /// Stop the streaming thread and send a Stop packet.
        /// </summary>
        public void Stop(bool seekToStart = true)
        {
            lock (synclock)
            {
                Log.Information("Stopping stream.");
                this.continueStreaming = false;
                this.Send(this.BuildStopPacket());
                if (seekToStart) audio.SeekToStart();
                seq = 0;
                deltaSeq = 0;
                encryption_ctr = 0;
            }
        }

        public void Pause()
        {
            lock (synclock)
            {
                Log.Information("Pausing stream.");
                this.continueStreaming = false;
                this.Send(this.BuildPausePacket());
                State = StreamState.Paused;
            }
        }

        public void Resume()
        {
            lock (synclock)
            {
                Log.Information("Resuming stream.");
                deltaSeq = seq;
                StartStream(true);
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
}
