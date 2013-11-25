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

        protected PacketEncrypter crypto;
        protected bool useEncryption;
        protected byte[] key, nonce;
        //CTR is the sequence value. don't need a separate value here.

        protected RTPStreamBase(IPEndPoint ep, bool useEncryption)
        {
            if (ep.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException("IP endpoint must be AF_INET. IPv6 is currently not supported.");
            if (!LibUtil.Util.IsMulticastAddress(ep.Address))
                throw new ArgumentException("IP address must be in the range 224.0.0.0/4 (multicast)");

            c = new UdpClient(AddressFamily.InterNetwork);
            c.JoinMulticastGroup(ep.Address);
            this.ep = ep;

            this.useEncryption = useEncryption;
            Log.Information("Creating RTP stream endpoint. Encryption = " + this.useEncryption);
        }
    }
}