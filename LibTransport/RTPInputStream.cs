﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using LibSecurity;

namespace LibTransport
{
    public class RTPInputStream : RTPStreamBase
    {
        public RTPInputStream(IPEndPoint ep, bool useEncryption) : base(ep, useEncryption)
        {
            c.Client.Bind(new IPEndPoint(IPAddress.Any, ep.Port));
        }

        public RTPInputStream(IPEndPoint ep, bool useEncryption, byte[] key, byte[] nonce) : this(ep, useEncryption)
        {
            this.key = key;
            this.nonce = nonce;
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
            p = LibConfig.Config.GetFlag(LibConfig.Config.ENABLE_ENCRYPTION) ? RTPPacket.Parse(data, key, nonce) : RTPPacket.Parse(data);

            return p;
        }
    }
}
