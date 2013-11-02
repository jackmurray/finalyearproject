using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibTransport;
using LibTrace;

namespace SpeakerReceiver
{
    public class StreamReceiver
    {
        private RTPInputStream s;
        private bool shouldRun = true;
        private Thread t;

        public StreamReceiver(RTPInputStream s)
        {
            this.s = s;
            s.SetReceiveTimeout(1000);
        }

        public void Stop()
        {
            shouldRun = false;
            while (t.IsAlive)
            {
            }
            return;
        }

        public void Start()
        {
            this.t = new Thread(ThreadProc);
            this.t.Start();
        }

        private void ThreadProc()
        {
            RTPPacket p;
            while (shouldRun)
            {
                try
                {
                    p = s.Receive();
                    DateTime dt = RTPPacket.BuildDateTime(p.Timestamp, DateTime.UtcNow); //change utcnow to the actual base timestamp when the control protocol is implemented.
                    Trace.GetInstance("LibTransport").Verbose("Packet timestamp: " + dt + ":" + dt.Millisecond);
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
