﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibUtil;

namespace LibTransport
{
    /// <summary>
    /// Restricted implementation of RTP packet format. There's no support for the CSRC fields or extensions headers.
    /// </summary>
    public abstract class RTPPacket
    {
        public const short Version = 2;
        public bool Padding {get; protected set;}
        public bool Marker { get; protected set; }
        public const ushort PayloadType = 35;
        public ushort SequenceNumber { get; protected set; }
        public uint Timestamp {get; protected set;}
        public uint SyncSource { get; protected set; }
        public byte[] Payload { get; protected set;}

        protected RTPPacket(bool Padding, bool Marker, ushort SequenceNumber, uint Timestamp,
                         uint SyncSource, byte[] Payload)
        {
            this.Padding = Padding;
            this.Marker = Marker;
            this.SequenceNumber = SequenceNumber;
            this.Timestamp = Timestamp;
            this.SyncSource = SyncSource;
            this.Payload = Payload;
        }

        public byte[] Serialise()
        {
            MemoryStream ms = new MemoryStream();
            byte byte1 = 0x00, byte2 = 0x00;
            if (Padding) byte1 |= 0x20;

            byte1 |= (Version << 6);

            byte2 |= (byte)(PayloadType);
            if (Marker) byte2 |= 0x80;

            ms.WriteByte(byte1);
            ms.WriteByte(byte2);
            ms.Write(Util.Encode(SequenceNumber), 0, 2);
            ms.Write(Util.Encode(Timestamp), 0, 4);
            ms.Write(Util.Encode(SyncSource), 0, 4);

            ms.Write(Payload, 0, Payload.Length);

            return ms.ToArray();
        }

        public static uint BuildTimestamp(DateTime dt)
        {
            TimeSpan fullspan = (dt - new DateTime(1970, 1, 1, 0, 0, 0));
            long secs = (long)fullspan.TotalSeconds;
            ushort rtpsecs = (ushort)(secs % (1 << 16));
            double frac = fullspan.TotalSeconds - secs;
            double shifted = frac * (1 << 16);
            uint fixpoint = ((uint)shifted);

            uint ret = rtpsecs;
            ret <<= 16;
            ret += fixpoint;
            return ret;
        }

        public static RTPPacket Parse(byte[] data)
        {
            if ((data[0] & 0xC0) != 0x80)
                throw new FormatException("RTP version must be 2.");

            bool Padding = ((data[0] & 0x20) != 0x00);

            if ((data[0] & 0x10) != 0x00)
                throw new FormatException("RTP extensions are not supported.");

            if ((data[0] & 0x0F) != 0x00)
                throw new FormatException("CSRC not supported.");

            bool isControl = ((data[1] & 0x80) != 0x00);

            if ((data[1] & 0x7F) != PayloadType)
                throw new FormatException("Payload type must be 35.");

            ushort seq = Util.DecodeUshort(data, 2);
            uint timestamp = Util.DecodeUint(data, 4);
            uint ssrc = Util.DecodeUint(data, 8);
            byte[] payload = data.Skip(12).ToArray();

            return new RTPDataPacket(Padding, seq, timestamp, ssrc, payload); //TODO: return control packets when implemented.
        }
    }
}