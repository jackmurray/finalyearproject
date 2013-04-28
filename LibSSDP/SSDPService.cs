using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibTrace;
using LibSecurity;

namespace LibSSDP
{
    public class SSDPService
    {
        private KeyManager key;
        private Trace Log = new Trace("SSDPService");

        public SSDPService(KeyManager key)
        {
            this.key = key;
        }

        private void Announce()
        {
            UdpClient client = Util.GetClient();
            SSDPPacket.BuildAnnouncePacket(key).Send(client);
            client.Close();
        }

        private void ResponderThreadProc()
        {
            byte[] data;
            UdpClient client = Util.GetClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); //Windows's SSDP thing already has a socket here, so we need this option so we can open one too.
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(SSDPPacket.LocalEndpoint);
            while (true)
            {
                try
                {
                    data = client.Receive(ref SSDPPacket.LocalEndpoint);
                    try
                    {
                        SSDPPacket p = SSDPPacket.Parse(Encoding.ASCII.GetString(data));
                        if (p == null)
                        {
                            Log.Information("We got an SSDP packet, but it's not one of ours. Ignoring...");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Information("Invalid packet received. Ignoring...");
                    }
                }
                catch (SocketException ex)
                {
                    Log.Critical("SSDP responder thread died!");
                    Thread.CurrentThread.Abort();
                }
            }
        }

        public void Start()
        {
            Announce();
            Thread t = new Thread(ResponderThreadProc);
            t.Start();
        }
    }
}
