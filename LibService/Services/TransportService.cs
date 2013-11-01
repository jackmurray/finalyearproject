﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace LibService
{
    public class TransportService : ServiceBase
    {
        private UdpClient _c;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">New UdpClient object that will be used for communication. Initialise it as AF_INET but don't do anything else.</param>
        public TransportService(UdpClient c)
        {
            Name = "TransportService";
            Operations = new List<string>() {"JoinGroup"};
            _c = c;
        }

        public override ServiceMessageResponse HandleMessage(ServiceMessage m, X509Certificate remoteParty)
        {
            switch (m.operationID)
            {
                case "JoinGroup":
                    IPAddress ip;
                    if (!IPAddress.TryParse(m.Data, out ip))
                        return Error("Failed to parse IP.");
                    if (!LibUtil.Util.IsMulticastAddress(ip))
                        return Error("IP address was not a multicast group address.");
                    try
                    {
                        _c.JoinMulticastGroup(ip);
                        return Success();
                    }
                    catch (Exception ex)
                    {
                        return Error(ex.Message);
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid message received.");
            }
        }
    }
}