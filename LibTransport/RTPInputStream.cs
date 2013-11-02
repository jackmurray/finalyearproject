using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace LibTransport
{
    public class RTPInputStream : RTPStreamBase
    {
        public RTPInputStream(IPEndPoint ep) : base(ep)
        {
            c.Client.Bind(new IPEndPoint(IPAddress.Any, ep.Port));
        }

        public void SetReceiveTimeout(int ms)
        {
            c.Client.ReceiveTimeout = ms;
        }

        public RTPPacket Receive()
        {
            IPEndPoint remoteHost = null;
            byte[] data = c.Receive(ref remoteHost);
            return RTPPacket.Parse(data);
        }
    }
}
