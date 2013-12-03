using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using LibSecurity;

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

            RTPPacket p;
            p = this.useEncryption ? RTPPacket.Parse(data, pekm) : RTPPacket.Parse(data);

            return p;
        }
    }
}
