using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibSecurity;
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
        public bool HasExtension { get; protected set; }
        public bool Marker { get; protected set; }
        public const ushort PayloadType = 35;
        public ushort SequenceNumber { get; protected set; }
        public uint Timestamp {get; protected set;}
        public uint SyncSource { get; protected set; }
        public byte[] Payload { get; protected set;}
        public byte[] ExtensionData { get; protected set; }

        protected byte[] extensionHeaderID = {0x00, 0x00};

        protected RTPPacket(bool Padding, bool Extension, bool Marker, ushort SequenceNumber, uint Timestamp,
                         uint SyncSource, byte[] Payload, byte[] ExtensionData)
        {
            this.Padding = Padding;
            this.HasExtension = Extension;
            this.Marker = Marker;
            this.SequenceNumber = SequenceNumber;
            this.Timestamp = Timestamp;
            this.SyncSource = SyncSource;
            this.Payload = Payload;
            this.ExtensionData = ExtensionData;
        }

        private MemoryStream SerialiseHeader(bool includeExtension = false)
        {
            MemoryStream ms = new MemoryStream();
            byte byte1 = 0x00, byte2 = 0x00;
            if (Padding) byte1 |= 0x20;
            if (includeExtension) byte1 |= 0x10;
            byte1 |= (Version << 6);

            byte2 |= (byte)(PayloadType);
            if (Marker) byte2 |= 0x80;

            ms.WriteByte(byte1);
            ms.WriteByte(byte2);
            ms.Write(Util.Encode(SequenceNumber), 0, 2);
            ms.Write(Util.Encode(Timestamp), 0, 4);
            ms.Write(Util.Encode(SyncSource), 0, 4);

            return ms;
        }

        public byte[] Serialise()
        {
            MemoryStream ms = SerialiseHeader();
            ms.Write(Payload, 0, Payload.Length);
            return ms.ToArray();
        }

        public byte[] SerialiseEncrypted(PacketEncrypter crypto, long encryption_ctr, Signer s = null)
        {
            MemoryStream ms = SerialiseHeader(true);
            ms.Write(extensionHeaderID, 0, extensionHeaderID.Length);

            MemoryStream extensionHeader = new MemoryStream();
            byte[] encodedctr = Util.Encode(encryption_ctr);
            extensionHeader.Write(encodedctr, 0, encodedctr.Length);//write the 8-byte CTR

            if (s != null)
            {
                byte[] sig = s.Sign(Payload);
                extensionHeader.Write(sig, 0, sig.Length);
            }

            byte[] extensionData = extensionHeader.ToArray();
            ushort extensionLen = (ushort) ((extensionHeader.Length / 4)); //RTP counts the number of 32-bit blocks.
            extensionLen += (ushort)(extensionHeader.Length % 4 != 0 ? 1 : 0);

            byte[] encodedExtensionLen = Util.Encode(extensionLen);
            ms.Write(encodedExtensionLen, 0, encodedExtensionLen.Length);
            ms.Write(extensionData, 0, extensionData.Length);

            ms.Write(crypto.Encrypt(Payload), 0, Payload.Length);
            return ms.ToArray();
        }

        public int GetCounterIncrement(PacketEncrypter crypto)
        {
            int wholeBlocks = Payload.Length/crypto.GetBlockSize();
            int extra = Payload.Length%crypto.GetBlockSize();
            return wholeBlocks + (extra != 0 ? 1 : 0);
        }

        public static uint BuildTimestamp(DateTime dt)
        {
            TimeSpan fullspan = (dt - new DateTime(1970, 1, 1, 0, 0, 0));
            long secs = (long)fullspan.TotalSeconds;
            ushort rtpsecs = (ushort)(secs % (1 << 16));
            double frac = (fullspan.Milliseconds == 0 ? 0 : fullspan.TotalSeconds - secs);
            double shifted = frac * (1 << 16);
            uint fixpoint = ((uint)shifted);

            uint ret = rtpsecs;
            ret <<= 16;
            ret += fixpoint;
            return ret;
        }

        public static DateTime BuildDateTime(uint timestamp, DateTime basetime)
        {
            ushort msecbits = (ushort) (timestamp & 0x0000FFFF);
            double msec = (double)msecbits/(1 << 16);
            uint secsbits = timestamp & 0xFFFF0000;
            secsbits >>= 16;

            basetime = basetime.Subtract(new TimeSpan(0, 0, 0, 0, basetime.Millisecond));
            long unixtime = (long)(basetime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            DateTime ret = basetime.Subtract(new TimeSpan(0, 0, (int)(unixtime%(1 << 16))));

            return ret.AddSeconds(secsbits).AddSeconds(msec);
        }

        public static RTPPacket Parse(byte[] data, PacketEncrypterKeyManager pekm = null, Verifier v = null)
        {
            if ((data[0] & 0xC0) != 0x80)
                throw new FormatException("RTP version must be 2.");

            bool Padding = ((data[0] & 0x20) != 0x00);

            bool extension = false;
            byte[] extensionData = new byte[0]; //initialiser is to shut the compiler up. it complains about a possible uninitialised access in the data.Skip() ternary but it's actually ok because it'll never be accessed if it wasn't set.
            RTPExtensionHeader decodedExtensionData = null;
            if ((data[0] & 0x10) != 0x00) //packet has extension header and therefore is encrypted (and maybe signed)
            {
                extension = true;
                //ignoring the extension type ID for now. it would be at index 12.
                int extensionLen = Util.DecodeUshort(data, 14) * 4;
                extensionData = data.Skip(16).Take(extensionLen).ToArray();
                decodedExtensionData = new RTPExtensionHeader(extensionData);
            }

            if ((data[0] & 0x0F) != 0x00)
                throw new FormatException("CSRC not supported.");

            bool isControl = ((data[1] & 0x80) != 0x00);

            if ((data[1] & 0x7F) != PayloadType)
                throw new FormatException("Payload type must be 35.");

            ushort seq = Util.DecodeUshort(data, 2);
            uint timestamp = Util.DecodeUint(data, 4);
            uint ssrc = Util.DecodeUint(data, 8);
            byte[] payload = data.Skip(12 + (extension == true ? extensionData.Length+4 : 0)).ToArray(); //if the extension flag is set, the payload is at 12 (length of base header) + extensionDataLength + 4 (for the extension header info fields)

            if (pekm != null && decodedExtensionData != null)
            {
                var crypto = new PacketEncrypter(pekm, decodedExtensionData.ctr, false);
                payload = crypto.Decrypt(payload);
            }

            if (v != null)
            {
                bool sigcheck = v.Verify(payload, decodedExtensionData.sig);
                if (!sigcheck)
                    throw new CryptographicException("Packet seq=" + seq + " failed signature check.");
            }

            if (!isControl)
                return new RTPDataPacket(Padding, extension, seq, timestamp, ssrc, payload, extensionData);
            else
            {
                if (payload.Length < 1)
                    throw new FormatException("Got a control packet with no payload!");

                RTPControlAction a = (RTPControlAction) payload[0];
                byte[] extradata = new byte[0];

                if (payload.Length > 1)
                    extradata = payload.Skip(1).ToArray(); //take the payload minus the first byte as the extra data.

                return new RTPControlPacket(a, extradata, extension, seq, timestamp, ssrc, extensionData);
            }
        }
    }

    class RTPExtensionHeader
    {
        public long ctr { get; protected set; }
        public byte[] sig { get; protected set; }

        public RTPExtensionHeader(byte[] data)
        {
            int ptr = 0;
            ctr = Util.DecodeLong(data, 0);
            ptr += sizeof (long);
            sig = new byte[data.Length - ptr];
            Array.Copy(data, ptr, sig, 0, sig.Length);
        }
    }
}
