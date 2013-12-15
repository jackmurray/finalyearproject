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
            this.seq = p.SequenceNumber;

            return p;
        }

        /// <summary>
        /// Sets the next key in the PEKM to the arguments. Call RotateKey() to use this new key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nonce"></param>
        public void DeliverNewKey(byte[] key, byte[] nonce)
        {
            this.pekm.SetNextKey(key, nonce);
        }

        /// <summary>
        /// Changes the key in the PEKM to the one set by DeliverNewKey
        /// </summary>
        public void RotateKey()
        {
            this.pekm.UseNextKey();
            Log.Verbose("RTPInputStream: last seq we saw was " + this.seq);
        }
    }
}
