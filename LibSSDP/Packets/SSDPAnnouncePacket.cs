﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LibSecurity;

namespace LibSSDP
{
    public class SSDPAnnouncePacket : SSDPPacket
    {
        internal SSDPAnnouncePacket()
        {
            Method = LibSSDP.Method.Announce;
        }

        protected override string GetSpecificHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("NTS: ssdp:alive\r\n");
            sb.AppendFormat("NT: {0}\r\n", ServiceType);

            return sb.ToString();
        }

        internal static SSDPAnnouncePacket Build(KeyManager key)
        {
            string fingerprint = key.GetFingerprint();
            SSDPAnnouncePacket packet = new SSDPAnnouncePacket()
            {
                fingerprint = fingerprint
            };
            packet.Signature = packet.GetSignature(key);
            return packet;
        }
    }
}
