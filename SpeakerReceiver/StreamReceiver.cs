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
        private Thread receiveThread;
        private DateTime basetime;
        private Trace Log = Trace.GetInstance("LibTransport");
        private AudioPlayer player = null;

        private List<RTPDataPacket> Buffer = new List<RTPDataPacket>();

        public StreamReceiver(RTPInputStream s)
        {
            this.s = s;
            s.SetReceiveTimeout(1000);
        }

        public void Stop()
        {
            shouldRun = false;
            while (receiveThread.IsAlive)
            {
            }
            return;
        }

        public void Start()
        {
            this.player = new AudioPlayer();
            this.receiveThread = new Thread(ReceiveThreadProc);
            this.receiveThread.Start();
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
                        this.player.Reset();
                        break;
                }
            }
            else //data packet
            {
                if (basetime == null)
                    return; //if we haven't yet got a basetime we can't proceed processing a data packet.

                Buffer.Add(_p as RTPDataPacket);
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
    }
}
