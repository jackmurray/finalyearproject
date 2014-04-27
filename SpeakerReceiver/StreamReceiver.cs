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
        private bool shouldRunReceiver = true, shouldStartPlaying = false;
        private bool shouldRunPlayer = false; //used so the HandlePacket() function can signal the callback's fetch loop to stop when it's time.
        private Thread receiveThread;
        private int maxStreamError = LibConfig.Config.GetInt(LibConfig.Config.MAX_STREAM_ERROR); //drop samples after we become this many ms late

        private DateTime basetime;
        private ushort SamplesPerFrame;
        private ushort Frequency;
        private byte Channels;
        private byte BitsPerSample;

        private Trace Log = Trace.GetInstance("LibTransport");
        private AudioPlayer player = null;
        private Dictionary<int, RTPPacket> Buffer = new Dictionary<int, RTPPacket>();
        private ushort i = 1; //read pointer for the buffer. yes, 1, because the first packet has sequencenumber=1
        private int packetPos = 0; //position within the current packet, if a partial read is needed to satisfy the player's request.
        private int dataPacketsBuffered = 0; //count of how many data packets we have. don't want to just use Buffer.Count() because it will include controls too.
        private int minBufPackets = int.MaxValue; //so we don't need an additional check for 'did we calculate the value yet'
        private int volume = SDLOutput.MaxVolume;

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

            while (receiveThread.IsAlive)
            {
            }
            return;
        }

        public void Start()
        {
            shouldRunReceiver = true;

            this.receiveThread = new Thread(ReceiveThreadProc);
            this.receiveThread.Start();
            this.player = new AudioPlayer();
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
            this.player.Stop();
            Buffer.Clear();
            i = 1;
            dataPacketsBuffered = 0;
            packetPos = 0;
            minBufPackets = int.MaxValue;
            shouldRunPlayer = false;
        }

        private void HandleControlPacket(RTPControlPacket p)
        {
            switch (p.Action)
            {
                case RTPControlAction.Play:
                    Log.Information("Stream started.");
                    s.State = StreamState.Started;
                    break;
                case RTPControlAction.Stop:
                    Log.Information("Stop packet received - end of stream.");
                    this.EndPlayerThread();
                    s.State = StreamState.Stopped;
                    break;
                case RTPControlAction.Pause:
                    Log.Information("Pausing stream.");
                    this.player.Pause();
                    s.State = StreamState.Paused;
                    break;
                case RTPControlAction.FetchKey:
                case RTPControlAction.SwitchKey:
                case RTPControlAction.Sync:
                case RTPControlAction.Volume:
                    Log.Verbose(
                        "Fetch/SwitchKey/HeaderSync/Volume packet taken from buffer. Don't care as it's already been actioned.");
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
                            var playPacket = (cp as RTPPlayPacket);

                            if (s.State == StreamState.Started)
                            {
                                Log.Information("Got a Play packet while already playing. Ignoring it.");
                            }
                            else if (s.State == StreamState.Paused)
                            {
                                ResetStreamStateForPlayback(playPacket);
                            }
                            else
                            {
                                ResetStreamStateForPlayback(playPacket);
                                shouldRunPlayer = true;

                                this.SamplesPerFrame = playPacket.SamplesPerFrame;
                                this.Frequency = playPacket.Frequency;
                                this.Channels = playPacket.Channels;
                                this.BitsPerSample = playPacket.BitsPerSample;
                                this.s.Format = playPacket.Format; //tell the RTPInputStream what format we've got so it can decode things for us.

                                double ratefrac = (double)LibConfig.Config.GetInt(LibConfig.Config.STREAM_BUFFER_TIME)/1000; //fraction of 1 second we want to buffer
                                double framefrac = Frequency/((double)SamplesPerFrame/Channels); //num of frames to make 1 s
                                minBufPackets = (int)Math.Ceiling(framefrac*ratefrac) - 1; //-1 so we start a little early
                                
                                if (playPacket.Format == SupportedFormats.WAV)
                                    player.Setup(this.Callback, Frequency, Channels, BitsPerSample);
                                else if (playPacket.Format == SupportedFormats.MP3)
                                {
                                    MP3Decoder.Init();
                                    player.Setup(this.Callback, Frequency, Channels, MP3Decoder.SAMPLE_SIZE);
                                    this.BitsPerSample = MP3Decoder.SAMPLE_SIZE;
                                }
                                else
                                    throw new FormatException("Don't know what sample size format=" + playPacket.Format +
                                                              " should be decoded to!");
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
                        else if (cp.Action == RTPControlAction.Sync)
                        {
                            
                        }
                        else if (cp.Action == RTPControlAction.Pause)
                        {

                        } //don't care about pause as it's not that interesting (basically the same as Stop).
                        else if (cp.Action == RTPControlAction.Volume)
                        {
                            lock (syncLock)
                            {
                                volume = (cp as RTPVolumePacket).Volume;
                            }
                        }
                        else
                        {
                            Log.Warning("Unknown control packet received.");
                        }
                    }
                    else
                    {
                        dataPacketsBuffered++;

                        if (shouldStartPlaying && s.State != StreamState.Started && dataPacketsBuffered >= minBufPackets) //if we're not started and we have enough packets that we can start, do so.
                        {
                            Log.Verbose("Hit our target of " + minBufPackets + ", starting playback.");
                            this.player.Start();
                            s.State = StreamState.Started;
                            shouldStartPlaying = false; //we're started now, so set this to false so that we can't restart without a play packet.
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

        private void ResetStreamStateForPlayback(RTPPlayPacket p)
        {
            this.basetime = p.baseTime;
            shouldStartPlaying = true;
            lock (syncLock)
            {
                i = (ushort)(p.SequenceNumber + 1); //start the callback fetching data from the next packet after this.
            }
            Buffer.Clear(); //a play packet is the start of a stream (or a restart of one) so ditch everything we have so far because it's no good now.
            dataPacketsBuffered = 0;

            Log.Verbose("Received " + LibUtil.Util.FormatDate(basetime) + " as the base time stamp from packet " + p.SequenceNumber);
        }

        private void BufferPacket(RTPPacket p)
        {
            lock (syncLock)
            {
                Buffer[p.SequenceNumber] = p;
            }
        }

        /// <summary>
        /// Removes packet i from the buffer and resets variables.
        /// </summary>
        private void DebufferPacket()
        {
            lock (syncLock)
            {
                Buffer.Remove(i);
                i++;
                packetPos = 0;
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

            while (length > 0 && shouldRunPlayer)
            {
                lock (syncLock)
                {
                    if (Buffer.ContainsKey(i))
                    {
                        RTPPacket p = Buffer[i];
                        if (p.Marker)
                        {
                            HandleControlPacket(p as RTPControlPacket);
                            DebufferPacket();
                        }
                        else
                        {
                            DateTime packettime = RTPPacket.BuildDateTime(p.Timestamp, basetime);
                            TimeSpan span = packettime - DateTime.UtcNow;
                            if (span.TotalMilliseconds < -maxStreamError)
                            {
                                double latems = Math.Abs(span.TotalMilliseconds);
                                //Log.Verbose("Packet " + i + " " + latems + "ms late");                                
                                double packetTimeLength = (((double)SamplesPerFrame/Channels)/Frequency)*1000; //length in milliseconds
                                if (packetTimeLength <= latems)
                                    //if dropping this packet won't make up all the lateness, drop it all and loop again so we can drop more.
                                {
                                    //Log.Verbose("Dropping packet " + i);
                                    DebufferPacket();
                                    continue;
                                }
                                else
                                {
                                    double dropfrac = latems / packetTimeLength;
                                    int dropsamples = (int) (dropfrac*SamplesPerFrame); //drop this many samples from this packet.
                                    packetPos = dropsamples*(BitsPerSample/8); //drop the samples by setting packetPos to skip over them.
                                    //Log.Verbose("Dropping " + packetPos + " bytes (" + dropsamples + " samples) from packet " + i);
                                }
                            }

                            if ((p.Payload.Length - packetPos) <= length)
                                //if this packet isn't enough to fully satisfy the request, copy all its contents and loop again
                            {
                                Array.Copy(p.Payload, packetPos, managedbuf, managedbufptr, p.Payload.Length - packetPos);
                                managedbufptr += p.Payload.Length;
                                length -= p.Payload.Length;
                                DebufferPacket();
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
                        int needed = Math.Min(SamplesPerFrame * (BitsPerSample/8), length); //divide to get bytes
                        Array.Clear(managedbuf, managedbufptr, needed);
                        managedbufptr += needed;
                        length -= needed;
                        Log.Verbose("Packet " + i + " not available. Zero-filling " + needed);
                        i++;
                        packetPos = 0;
                    }
                }
            }

            byte[] mixbuf = new byte[managedbuf.Length];
            SDL2.SDL.SDL_MixAudioFormat(mixbuf, managedbuf, SDLOutput.AudioSpec.format, (uint)managedbuf.Length, volume); //mix the audio to the specifed volume

            Marshal.Copy(mixbuf, 0, buffer, mixbuf.Length); //copy the mixed audio to SDL's output buffer.
        }
    }
}
