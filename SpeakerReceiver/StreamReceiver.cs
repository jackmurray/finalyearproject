using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
        private Thread receiveThread;

        private DateTime basetime;
        private ushort SamplesPerFrame;
        private ushort Frequency;
        private byte Channels;

        private Trace Log = Trace.GetInstance("LibTransport");
        private AudioPlayer player = null;
        private Dictionary<int, RTPPacket> Buffer = new Dictionary<int, RTPPacket>();
        private int i = 1; //read pointer for the buffer. yes, 1, because the first packet has sequencenumber=1
        private int packetPos = 0; //position within the current packet, if a partial read is needed to satisfy the player's request.
        private int dataPacketsBuffered = 0; //count of how many data packets we have. don't want to just use Buffer.Count() because it will include controls too.
        private int minBufPackets = int.MaxValue; //so we don't need an additional check for 'did we calculate the value yet'

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

            while (receiveThread.IsAlive)
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
        }

        public void SetEncryptionKey(PacketEncrypterKeyManager pekm)
        {
            this.s.EnableEncryption(pekm);
        }

        public void SetVerifier(Verifier v)
        {
            this.s.EnableVerification(v);
        }

        /// <summary>
        /// Call from the player thread to stop it.
        /// </summary>
        private void EndPlayerThread()
        {
            shouldRunPlayer = false;
            Buffer.Clear();
        }

        private void HandleControlPacket(RTPControlPacket p)
        {
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
                            if (s.State == StreamState.Started)
                            {
                                Log.Information("Got a Play packet while already playing. Ignoring it.");
                            }
                            else
                            {
                                var playPacket = (cp as RTPPlayPacket);
                                this.basetime = playPacket.baseTime;
                                Log.Verbose("Taking " + basetime + ":" + basetime.Millisecond +
                                            " as the base time stamp.");
                                this.SamplesPerFrame = playPacket.SamplesPerFrame;
                                this.Frequency = playPacket.Frequency;
                                this.Channels = playPacket.Channels;

                                double ratefrac = (double)LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME)/1000; //fraction of 1 second we want to buffer
                                double framefrac = Frequency/((double)SamplesPerFrame/Channels); //num of frames to make 1 s
                                minBufPackets = (int)Math.Ceiling(framefrac*ratefrac);

                                player.Setup(this.Callback, Frequency, Channels);
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
                            
                        }
                        else if (cp.Action == RTPControlAction.Pause)
                        {

                        } //don't care about pause as it's not that interesting (basically the same as Stop).
                        else
                        {
                            Log.Warning("Unknown control packet received.");
                        }
                    }
                    else
                    {
                        dataPacketsBuffered++;

                        if (s.State != StreamState.Started && s.State != StreamState.Paused && dataPacketsBuffered >= minBufPackets)
                        {
                            lock (player)
                            //perhaps not strictly necessary but we want to make sure that nothing tries to write at the same time.
                            {
                                Log.Verbose("Hit our target of " + minBufPackets + ", starting playback.");
                                this.player.Start();
                                s.State = StreamState.Started;
                            }
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

        public void DeliverNewKey(byte[] key, byte[] nonce)
        {
            this.s.DeliverNewKey(key, nonce);
        }

        /// <summary>
        /// Callback called by SDL whenever it wants data.
        /// </summary>
        /// <param name="user">Unused.</param>
        /// <param name="buffer">The unmanaged pointer to the start of the buffer</param>
        /// <param name="length">The length of the buffer we're requested to fill</param>
        public void Callback(IntPtr user, IntPtr buffer, int length)
        {
            byte[] managedbuf = new byte[length];
            int managedbufptr = 0;

            while (length > 0)
            {
                lock (syncLock)
                {
                    if (Buffer.ContainsKey(i))
                    {
                        RTPPacket p = Buffer[i];
                        if (p.Marker)
                        {
                            HandleControlPacket(p as RTPControlPacket);
                            Buffer.Remove(i);
                            i++;
                            packetPos = 0;
                        }
                        else
                        {
                            if ((p.Payload.Length - packetPos) <= length)
                                //if this packet isn't enough to fully satisfy the request, copy all its contents and loop again
                            {
                                Array.Copy(p.Payload, packetPos, managedbuf, managedbufptr, p.Payload.Length - packetPos);
                                managedbufptr += p.Payload.Length;
                                length -= p.Payload.Length;
                                Buffer.Remove(i);
                                i++;
                                packetPos = 0;
                                dataPacketsBuffered--;
                            }
                            else //take part of the packet and make a note of how much we used.
                            {
                                Array.Copy(p.Payload, 0, managedbuf, managedbufptr, length);
                                packetPos = length;
                                managedbufptr += length;
                                length = 0;
                                //we don't increment i because we want to get the rest of the packet next time
                            }
                        }
                    }
                    else //packet not in buffer. zero fill instead
                    {
                        Thread.Sleep(1);
                        int needed = Math.Min(SamplesPerFrame * 4, length);
                        Array.Clear(managedbuf, managedbufptr, needed);
                            //TODO: integrate properly with format sample size
                        managedbufptr += needed;
                        length -= needed;
                        i++;
                        packetPos = 0;
                        Log.Verbose("Zero-filling " + needed);
                    }
                }
            }

            Marshal.Copy(managedbuf, 0, buffer, managedbuf.Length); //although we don't actually need the lock any more, we don't want any other threads to run until this completes so we won't release it until the copy is done
        }
    }
}
