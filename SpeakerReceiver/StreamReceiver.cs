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
        private RTPInputStream s;
        private bool shouldRunReceiver = true, shouldRunPlayer = true;
        private Thread receiveThread, playerThread;
        private DateTime basetime;
        private Trace Log = Trace.GetInstance("LibTransport");
        private AudioPlayer player = null;
        private List<RTPPacket> Buffer = new List<RTPPacket>();

        public delegate void NotifyKeyRotation();
        public event NotifyKeyRotation OnKeyRotatePacketReceived;

        public StreamReceiver(RTPInputStream s)
        {
            this.s = s;
            s.SetReceiveTimeout(1000);
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

            this.player = new AudioPlayer();
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
        /// Call from the receive thread to reset the player thread.
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
                    case RTPControlAction.Stop:
                        this.EndPlayerThread();
                        break;
                    default:
                        Log.Warning("Control packet received but unable to handle.");
                        break;
                }
            }
            else //data packet
            {
                if (basetime == null)
                    return; //if we haven't yet got a basetime we can't proceed processing a data packet.

                player.Write(_p.Payload);
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
                                this.basetime = cp.ComputeBaseTime();
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
                        else
                        {
                            BufferPacket(p);
                        }
                    }
                    else
                    {
                        BufferPacket(p);
                    }
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
            lock (Buffer)
            {
                Buffer.Add(p);
            }
        }

        /// <summary>
        /// Code executed by the audio player thread. This thread isn't started until there is at least one packet in the buffer.
        /// </summary>
        private void PlayerThreadProc()
        {
            int i = 0;
            while (shouldRunPlayer)
            {
                RTPPacket p = null;
                while (p == null)
                {
                    //this section uses Monitor directly instead of the lock statement because we don't want to sleep holding the lock or have to acquire it twice
                    Monitor.Enter(Buffer);
                    if (Buffer.Count > i)
                    {
                        p = Buffer[i];
                        Monitor.Exit(Buffer);
                        i++;
                    }
                    else
                    {
                        Monitor.Exit(Buffer);
                        Thread.Sleep(1);
                        if (!shouldRunPlayer) return;
                    }
                }
                DateTime packetactiontime = RTPPacket.BuildDateTime(p.Timestamp, this.basetime);
                if (packetactiontime > DateTime.UtcNow)
                {
                    TimeSpan sleeptime = packetactiontime - DateTime.UtcNow;
                    Thread.Sleep((int) sleeptime.TotalMilliseconds);
                }

                HandlePacket(p);
            }
            player.Reset(); //this thread is terminating, so reset the audio output for the next one.
        }

        public void DeliverNewKey(byte[] key, byte[] nonce)
        {
            this.s.DeliverNewKey(key, nonce);
        }
    }
}
