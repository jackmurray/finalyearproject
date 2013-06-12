﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibTrace;

namespace LibSSDP
{
    public class SSDPClient
    {
        private UdpClient c;
        private IPEndPoint localEP, remoteEP;
        private Trace Log = Trace.GetInstance("SSDPClient");
        public event ResponsePacketReceived OnResponsePacketReceived;

        public SSDPClient()
        {
            c = Util.GetClient();
            localEP = c.Client.LocalEndPoint as IPEndPoint;
        }

        public void StartDiscovery()
        {
            SSDPSearchPacket p = SSDPSearchPacket.Build();
            p.Send(c);
            new Thread(ListenerThreadProc).Start();
        }

        private void ListenerThreadProc()
        {
            byte[] data;
            while (true)
            {
                data = c.Receive(ref remoteEP);
                try
                {
                    SSDPPacket p = SSDPPacket.Parse(Encoding.ASCII.GetString(data));
                    if (p == null)
                    {
                        Log.Information("We got an SSDP packet, but it's not one of ours. Ignoring...");
                        continue;
                    }
                    else //Got a valid packet.
                    {
                        if (p.Method == Method.Respond)
                        {
                            //The 'as' is safe because we've already checked the method.
                            string stripped = Encoding.ASCII.GetString(data);
                            int pos = stripped.IndexOf("USN: fingerprint"); //TODO: do this properly (i.e. remove the headers and serialise the packet rather than just hacking the end of the string off).
                            stripped = stripped.Remove(pos);
                            Log.Verbose("Stripped response: " + stripped);
                            OnResponsePacketReceived(this, new ResponsePacketReceivedArgs() { Packet = p as SSDPResponsePacket, Source = remoteEP.Address, StrippedPacket = Encoding.ASCII.GetBytes(stripped) });
                        }
                        else if (p.Method == Method.Announce)
                        {
                            //TODO
                        }
                        else
                        {
                            Log.Information("Got an SSDP discovery message. Don't care.");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Information("Invalid packet received. Ignoring...");
                }
            }
        }
    }

    public delegate void ResponsePacketReceived(object sender, ResponsePacketReceivedArgs args);

    public class ResponsePacketReceivedArgs
    {
        public SSDPResponsePacket Packet { get; set; }
        public IPAddress Source { get; set; }
        public byte[] StrippedPacket { get; set; } //packet data with signature stripped, ready for verification.
    }
}
