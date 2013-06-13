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
        private CertManager cert;
        private Trace Log = Trace.GetInstance("SSDPService");

        /// <summary>
        /// When we receive a packet, the IP gets put in here.
        /// </summary>
        private IPEndPoint remoteEP;

        public SSDPService(KeyManager key, CertManager cert)
        {
            this.key = key;
            this.cert = cert;
        }

        private void Announce()
        {
            UdpClient client = Util.GetClient();
            new SSDPAnnouncePacket(key, cert).Send(client);
            client.Close();
        }

        private void ResponderThreadProc()
        {
            byte[] data;
            UdpClient client = Util.GetListener(SSDPPacket.LocalEndpoint);
            
            while (true)
            {
                try
                {
                    data = client.Receive(ref remoteEP);
                    try
                    {
                        SSDPPacket p = SSDPPacket.Parse(Encoding.ASCII.GetString(data));
                        if (p == null)
                        {
                            Log.Verbose("We got an SSDP packet, but it's not one of ours. Ignoring...");
                            continue;
                        }
                        else //Got a valid packet.
                        {
                            if (p.Method == Method.Search) //If someone is looking for us, respond. We don't care about other announcers.
                            {
                                Log.Information("Got an SSDP search. Sending response.");
                                SSDPResponsePacket response = new SSDPResponsePacket(key, cert);
                                response.Send(remoteEP);
                            }
                            else
                            {
                                Log.Verbose("Got an SSDP announce/response message. Don't care.");
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Verbose("Invalid packet received. Ignoring...");
                    }
                }
                catch (SocketException ex)
                {
                    Log.Critical("SSDP socket operation failed: " + ex.Message);
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
