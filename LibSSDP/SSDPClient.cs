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

        public SSDPClient()
        {
            sender = Util.GetClient();
            listener = Util.GetListener(SSDPPacket.LocalEndpoint);
        }

        public void StartDiscovery()
        {
            SSDPSearchPacket p = SSDPSearchPacket.Build();
            p.Send(sender);
            responseHandlerThread = new Thread(SenderResponseHandler);
            responseHandlerThread.Name = "SSDP Response Processor";
            responseHandlerThread.Start();

            announceListenerThread = new Thread(AnnounceListenerResponseHandler);
            announceListenerThread.Name = "SSDP Announce Listener";
            announceListenerThread.Start();
        }

        private void SenderResponseHandler()
        {
            byte[] data;
            IPEndPoint remoteEP = null;
            while (true)
            {
                data = sender.Receive(ref remoteEP);
                try
                {
                    ParsePacket(data, remoteEP);
                }
                catch (Exception ex)
                {
                    Log.Verbose("Invalid packet received. Ignoring...");
                }
            }
        }

        private void AnnounceListenerResponseHandler()
        {
            byte[] data;
            IPEndPoint remoteEP = null;
            while (true)
            {
                data = listener.Receive(ref remoteEP);
                try
                {
                    ParsePacket(data, remoteEP);
                }
                catch (Exception ex)
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
                Log.Information("We got an SSDP packet, but it's not one of ours. Ignoring...");
                return;
            }
            else //Got a valid packet.
            {
                if (p.Method == Method.Respond || p.Method == Method.Announce)
                {
                    string stripped = Encoding.ASCII.GetString(data);
                    int pos = stripped.IndexOf("USN: fingerprint"); //TODO: do this properly (i.e. remove the headers and serialise the packet rather than just hacking the end of the string off).
                    stripped = stripped.Remove(pos);
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
                    Log.Information("Got an SSDP discovery message. Don't care.");
                    return;
                }
            }
        }

        public void Stop()
        {
            if (responseHandlerThread != null)
                responseHandlerThread.Abort();
            if (announceListenerThread != null)
                announceListenerThread.Abort();
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
