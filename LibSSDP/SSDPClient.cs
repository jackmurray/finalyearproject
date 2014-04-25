using System;
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
        private UdpClient sender, listener;
        private Trace Log = Trace.GetInstance("SSDPClient");
        public event ResponsePacketReceived OnResponsePacketReceived;
        public event ResponsePacketReceived OnAnnouncePacketReceived;

        private Thread announceListenerThread, responseHandlerThread;
        private bool shuttingDown = false;

        public SSDPClient()
        {
            sender = Util.GetClient();
            listener = Util.GetListener(SSDPPacket.LocalEndpoint);
        }

        public void StartDiscovery()
        {
            SSDPSearchPacket p = SSDPSearchPacket.Build();
            p.Send(sender);
            responseHandlerThread = new Thread(SSDPClientThreadProc);
            responseHandlerThread.Name = "SSDP Response Processor";
            responseHandlerThread.Start(sender);

            announceListenerThread = new Thread(SSDPClientThreadProc);
            announceListenerThread.Name = "SSDP Announce Listener";
            announceListenerThread.Start(listener);
        }

        private void SSDPClientThreadProc(object o)
        {
            byte[] data;
            IPEndPoint remoteEP = null;
            UdpClient c = o as UdpClient;
            c.Client.ReceiveTimeout = 1000; //force the loop to run each second so we can exit the thread when we need to
            while (true)
            {
                try
                {
                    if (shuttingDown)
                        return;
                    data = c.Receive(ref remoteEP);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                        continue;
                    else throw;
                }

                try
                {
                    ParsePacket(data, remoteEP);
                }
                catch (Exception)
                {
                    Log.Verbose("Invalid packet received. Ignoring...");
                }
            }
        }

        private void ParsePacket(byte[] data, IPEndPoint remoteEP)
        {
            SSDPPacket p = SSDPPacket.Parse(Encoding.ASCII.GetString(data));
            if (p == null)
            {
                //Log.Verbose("We got an SSDP packet, but it's not one of ours. Ignoring...");
                return;
            }
            else //Got a valid packet.
            {
                if (p.Method == Method.Respond || p.Method == Method.Announce)
                {
                    var stripped = Util.StripPacket(data);
                    Log.Verbose("Stripped response: " + stripped);

                    var args = new ResponsePacketReceivedArgs()
                    {
                        Packet = p as SSDPSignedPacket,
                        Source = remoteEP.Address,
                        StrippedPacket = Encoding.ASCII.GetBytes(stripped)
                    };
                    if (p.Method == Method.Announce)
                        OnAnnouncePacketReceived(this, args);
                    else if (p.Method == Method.Respond)
                        OnResponsePacketReceived(this, args);
                    else throw new Exception("This will never happen."); //unless you forget to change the if statement above when you change this one.
                }
                else
                {
                    Log.Verbose("Got an SSDP discovery message. Don't care.");
                    return;
                }
            }
        }

        

        public void Stop()
        {
            shuttingDown = true; //no locking needed since it's only ever written here.
        }
    }

    public delegate void ResponsePacketReceived(object sender, ResponsePacketReceivedArgs args);

    public class ResponsePacketReceivedArgs
    {
        public SSDPSignedPacket Packet { get; set; }
        public IPAddress Source { get; set; }
        public byte[] StrippedPacket { get; set; } //packet data with signature stripped, ready for verification.
    }
}
