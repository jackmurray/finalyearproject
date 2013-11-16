using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibTransport;
using LibTrace;
using LibAudio;

namespace SpeakerReceiver
{
    public class StreamReceiver
    {
        private RTPInputStream s;
        private bool shouldRun = true;
        private Thread receiveThread, playerThread;
        private DateTime basetime;
        private Trace Log = Trace.GetInstance("LibTransport");
        private AudioPlayer player = null;

        private List<RTPDataPacket> Buffer = new List<RTPDataPacket>();

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
            shouldRun = false;
            while (receiveThread.IsAlive || playerThread.IsAlive)
            {
            }
            return;
        }

        public void Start()
        {
            this.player = new AudioPlayer();
            this.receiveThread = new Thread(ReceiveThreadProc);
            this.receiveThread.Start();

            this.playerThread = new Thread(PlayerThreadProc);
        }

        /// <summary>
        /// Call from the receive thread to reset the player thread.
        /// </summary>
        private void Reset()
        {
            shouldRun = false;
            while (playerThread.IsAlive)
            {
            }
            shouldRun = true;
            Buffer.Clear();
            this.playerThread = new Thread(PlayerThreadProc);
        }

        private void HandlePacket(RTPPacket _p)
        {
            if (_p.Marker) //control packet
            {
                RTPControlPacket p = _p as RTPControlPacket;
                switch (p.Action)
                {
                    case RTPControlAction.Play:
                        this.basetime = p.ComputeBaseTime();
                        Log.Verbose("Taking " + basetime + ":" + basetime.Millisecond + " as the base time stamp.");
                        break;
                    case RTPControlAction.Stop:
                        this.Reset();
                        break;
                }
            }
            else //data packet
            {
                if (basetime == null)
                    return; //if we haven't yet got a basetime we can't proceed processing a data packet.

                lock (Buffer)
                {
                    Buffer.Add(_p as RTPDataPacket);
                }

                if (this.playerThread != null && this.playerThread.ThreadState == ThreadState.Unstarted)
                    this.playerThread.Start();
            }
        }

        private void ReceiveThreadProc()
        {
            RTPPacket p;
            while (shouldRun)
            {
                try
                {
                    p = s.Receive();
                    HandlePacket(p);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                        throw;
                }
            }
        }

        /// <summary>
        /// Code executed by the audio player thread. This thread isn't started until there is at least one packet in the buffer.
        /// </summary>
        private void PlayerThreadProc()
        {
            int i = 0;
            while (shouldRun)
            {
                RTPDataPacket p = null;
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
                        if (!shouldRun) return;
                    }
                }
                DateTime packetactiontime = RTPPacket.BuildDateTime(p.Timestamp, this.basetime);
                if (packetactiontime > DateTime.UtcNow)
                {
                    TimeSpan sleeptime = packetactiontime - DateTime.UtcNow;
                    Thread.Sleep((int) sleeptime.TotalMilliseconds);
                }
                player.Write(p.Payload);
            }
        }
    }
}
