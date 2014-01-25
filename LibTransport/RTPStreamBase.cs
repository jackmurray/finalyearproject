using System;
using System.Net;
using System.Net.Sockets;
using LibSecurity;
using LibTrace;

namespace LibTransport
{
    public abstract class RTPStreamBase
    {
        protected UdpClient c;
        protected IPEndPoint ep;
        protected static Trace Log = Trace.GetInstance("LibTransport");
        protected ushort seq = 0;

        protected PacketEncrypter crypto = null;
        protected bool useEncryption, useAuthentication;
        protected PacketEncrypterKeyManager pekm = null;
        //CTR is the sequence value. don't need a separate value here.

        public StreamState State { get; set; }

        protected RTPStreamBase(IPEndPoint ep)
        {
            if (ep.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("IP endpoint must be AF_INET. IPv6 is currently not supported.");
            if (!LibUtil.Util.IsMulticastAddress(ep.Address))
                throw new ArgumentException("IP address must be in the range 224.0.0.0/4 (multicast)");

            c = new UdpClient(AddressFamily.InterNetwork);
            c.JoinMulticastGroup(ep.Address);
            this.ep = ep;
            Log.Information("Creating RTP stream endpoint for " + ep);
        }

        public virtual void EnableEncryption(PacketEncrypterKeyManager pekm)
        {
            this.useEncryption = true;
            this.pekm = pekm;
            Log.Information("Enabling RTP packet encryption");
        }
    }

    public enum StreamState { Stopped, Started, Paused }
}