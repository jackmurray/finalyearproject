using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibSecurity;
using LibTransport;
using LibTrace;
using LibAudio;

namespace SpeakerReceiver
{
    public class StreamReceiver
    {
        private object syncLock = new object();
        private RTPInputStream s;
        private bool shouldRunReceiver = true, shouldRunPlayer = true;
        private Thread receiveThread, playerThread;
        private DateTime basetime;
        private Trace Log = Trace.GetInstance("LibTransport");
        private AudioPlayer player = null;
        private Dictionary<int, RTPPacket> Buffer = new Dictionary<int, RTPPacket>();
        private int i = 1; //read pointer for the buffer. yes, 1, because the first packet has sequencenumber=1

        public delegate void NotifyKeyRotation();
        public event NotifyKeyRotation OnKeyRotatePacketReceived;

        public StreamReceiver(RTPInputStream s)
        {
            this.s = s;
            s.SetReceiveTimeout(1000);
            this.player = new AudioPlayer();
        }

        /// <summary>
        /// Shut down all receiver threads.
        /// </summary>
        public void Stop()
        {
            //only call this method from outside the streamreceiver, because the internal threads are the ones being shut down!
            shouldRunReceiver = false;
            shouldRunPlayer = false;

            while (receiveThread.IsAlive || playerThread.IsAlive)
            {
            }
            return;
        }

        public void Start()
        {
            shouldRunReceiver = true;
            shouldRunPlayer = true;

            this.receiveThread = new Thread(ReceiveThreadProc);
            this.receiveThread.Start();

            this.ResetPlayerThread();
        }

        public void SetEncryptionKey(PacketEncrypterKeyManager pekm)
        {
            this.s.EnableEncryption(pekm);
        }

        public void SetVerifier(Verifier v)
        {
            this.s.EnableVerification(v);
        }

        private void ResetPlayerThread()
        {
            shouldRunPlayer = true;
            this.playerThread = new Thread(PlayerThreadProc);
        }

        /// <summary>
        /// Call from the player thread to stop it.
        /// </summary>
        private void EndPlayerThread()
        {
            shouldRunPlayer = false;
            Buffer.Clear();
        }

        private void HandlePacket(RTPPacket _p)
        {
            if (_p.Marker) //control packet
            {
                RTPControlPacket p = _p as RTPControlPacket;
                switch (p.Action)
                {
                    case RTPControlAction.Play:
                        Log.Information("Stream started.");
                        break;
                    case RTPControlAction.Stop:
                        Log.Information("Stop packet received - end of stream.");
                        this.EndPlayerThread();
                        break;
                    case RTPControlAction.Pause:
                        Log.Information("Pausing stream.");
                        this.EndPlayerThread();
                        break;
                    case RTPControlAction.FetchKey:
                    case RTPControlAction.SwitchKey:
                    case RTPControlAction.HeaderSync:
                        Log.Verbose(
                            "Fetch/SwitchKey/HeaderSync packet taken from buffer. Don't care as it's already been actioned.");
                        break;
                    default:
                        Log.Warning(String.Format("Control packet received but unable to handle. Type={0} seq={1}", p.Action, p.SequenceNumber));
                        break;
                }
            }
            else //data packet
            {
                if (basetime == null)
                    return; //if we haven't yet got a basetime we can't proceed processing a data packet.

                lock (player)
                {
                    player.Write(_p.Payload);
                }
            }
        }

        private void ReceiveThreadProc()
        {
            RTPPacket p;
            while (shouldRunReceiver)
            {
                try
                {
                    p = s.Receive();
                    if (p == null) //if it was null the RTPInputStream had to drop the packet and will have logged an error.
                        continue;
                    if (p.Marker)
                    {
                        var cp = p as RTPControlPacket;
                        if (cp.Action == RTPControlAction.Play)
                        {
                            if (playerThread.ThreadState == ThreadState.Running ||
                                playerThread.ThreadState == ThreadState.WaitSleepJoin)
                            {
                                Log.Information("Got a Play packet while already playing. Ignoring it.");
                            }
                            else
                            {
                                this.basetime = (cp as RTPPlayPacket).baseTime;
                                Log.Verbose("Taking " + basetime + ":" + basetime.Millisecond +
                                            " as the base time stamp.");
                                if (playerThread.ThreadState == ThreadState.Unstarted)
                                    //the first time this object is used.
                                    playerThread.Start();
                                if (playerThread.ThreadState == ThreadState.Stopped)
                                {
                                    this.ResetPlayerThread();
                                    this.playerThread.Start();
                                }
                            }
                        }
                        else if (cp.Action == RTPControlAction.FetchKey)
                        {
                            Log.Verbose("Got a FetchKey packet, firing fetch event...");
                            if (OnKeyRotatePacketReceived != null)
                                OnKeyRotatePacketReceived();
                        }
                        else if (cp.Action == RTPControlAction.SwitchKey)
                        {
                            this.s.RotateKey();
                        }
                        else if (cp.Action == RTPControlAction.Stop)
                        {
                            lock (syncLock)
                            {
                                Log.Verbose("There are " + Buffer.Count + " packets left in the buffer.");
                            }
                        }
                        else if (cp.Action == RTPControlAction.HeaderSync)
                        {
                            lock (player) //perhaps not strictly necessary but we want to make sure that nothing tries to write at the same time.
                            {
                                player.Write(cp.ExtraData);
                            }
                        }
                        else if (cp.Action == RTPControlAction.Pause)
                        {
                            
                        } //don't care about pause as it's not that interesting (basically the same as Stop).
                        else
                        {
                            Log.Warning("Unknown control packet received.");
                        }
                    }

                    BufferPacket(p); //always buffer all packets, even if they're control.
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                        throw;
                }
            }
        }

        private void BufferPacket(RTPPacket p)
        {
            lock (syncLock)
            {
                Buffer[p.SequenceNumber] = p;
            }
        }

        /// <summary>
        /// Code executed by the audio player thread. This thread isn't started until there is at least one packet in the buffer.
        /// </summary>
        private void PlayerThreadProc()
        {
            i = 1;
            int maxAllowedError = LibConfig.Config.GetInt(LibConfig.Config.MAX_STREAM_ERROR); //in millisec
            int streamBufferTime = LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME); //in millisec
            double totalError = 0;
            while (shouldRunPlayer)
            {
                RTPPacket p = null;

                while (p == null)
                {
                    //this section uses Monitor directly instead of the lock statement because we don't want to sleep holding the lock or have to acquire it twice
                    Monitor.Enter(syncLock);
                    if (Buffer.ContainsKey(i))
                    {
                        p = Buffer[i];
                        Buffer.Remove(i);
                        Monitor.Exit(syncLock);
                        i++;
                    }
                    else
                    {
                        if (!shouldRunPlayer)
                        {
                            Monitor.Exit(syncLock);
                            return;
                        }
                        Log.Warning("Receiver buffer underrun!");
                        /*Log.Warning(String.Format("It is currently {0} and the last packet we got is for {1}",
                                                  LibUtil.Util.FormatDate(DateTime.UtcNow),
                                                  LibUtil.Util.FormatDate(RTPPacket.BuildDateTime(Buffer[i - 1].Timestamp, this.basetime))));*/
                        
                        Monitor.Exit(syncLock);
                        Thread.Sleep(streamBufferTime); //maybe after sleeping this thread there'll be some more packets.
                    }
                }

                DateTime packetactiontime = RTPPacket.BuildDateTime(p.Timestamp, this.basetime);
                TimeSpan sleeptime = packetactiontime - DateTime.UtcNow;
                int sleep = (int) sleeptime.TotalMilliseconds;
                if (sleep > 0)
                {
                    //totalError += actualtime - timersleep;

                    /*if (totalError >= maxAllowedError)
                    {
                        //Log.Verbose(totalError + "ms of drift has built up, reducing it by " + maxAllowedError); //disabled for perf
                        timersleep += maxAllowedError;
                        totalError -= maxAllowedError;
                    }*/

                    //if (timersleep > 0)
                        Thread.Sleep(sleep);
                }

                HandlePacket(p);
                //Log.Verbose("Remaining: " + (Buffer.Count - i)); //disabled for perf
            }
            player.Reset(); //this thread is terminating, so reset the audio output for the next one.
        }

        public void DeliverNewKey(byte[] key, byte[] nonce)
        {
            this.s.DeliverNewKey(key, nonce);
        }
    }
}
