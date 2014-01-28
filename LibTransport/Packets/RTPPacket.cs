using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibAudio;
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
        public bool Marker { get; protected set; }
        public const ushort PayloadType = 35;
        public ushort SequenceNumber { get; protected set; }
        public uint Timestamp {get; protected set;}
        public uint SyncSource { get; protected set; }
        public byte[] Payload { get; set;}

        protected byte[] extensionHeaderID = {0x00, 0x00};

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

        public static uint BuildTimestamp(double offset)
        {
            return (uint)(offset * 1000); // * 1000 lets us hold on to the .xx fractional milliseconds accurately. it means the RTP timestamp field is measured in microseconds (usecs)
        }

        public static DateTime BuildDateTime(uint timestamp, DateTime basetime)
        {
            return basetime.AddMilliseconds((double)(timestamp)/1000);
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
                return new RTPDataPacket(Padding, seq, timestamp, ssrc, payload);
            else
            {
                if (payload.Length < 1)
                    throw new FormatException("Got a control packet with no payload!");

                RTPControlAction a = (RTPControlAction) payload[0];
                byte[] extradata = new byte[0];

                if (payload.Length > 1)
                    extradata = payload.Skip(1).ToArray(); //take the payload minus the first byte as the extra data.

                RTPControlPacket controlPacket;
                switch (a)
                {
                    case RTPControlAction.Play:
                        //format is 8 bytes of timestamp followed by 2 bytes of samplecount
                        byte[] ticks = extradata.Take(8).ToArray();
                        ushort samplesperframe = Util.DecodeUshort(extradata, 8);
                        ushort freq = Util.DecodeUshort(extradata, 10);
                        byte channels = extradata[12];
                        byte bitspersample = extradata[13];
                        SupportedFormats format = (SupportedFormats) extradata[14];

                        controlPacket = new RTPPlayPacket(seq, timestamp, ssrc, RTPTimestampPacket.ComputeBaseTime(ticks), samplesperframe, freq, channels, bitspersample, format);
                        break;
                    case RTPControlAction.Pause:
                        controlPacket = new RTPPausePacket(seq, timestamp, ssrc);
                        break;
                    case RTPControlAction.Stop:
                        controlPacket = new RTPStopPacket(seq, timestamp, ssrc);
                        break;
                    case RTPControlAction.FetchKey:
                        controlPacket = new RTPFetchKeyPacket(seq, timestamp, ssrc);
                        break;
                    case RTPControlAction.SwitchKey:
                        controlPacket = new RTPSwitchKeyPacket(seq, timestamp, ssrc);
                        break;
                    case RTPControlAction.Sync:
                        byte[] syncticks = extradata.Take(8).ToArray();
                        controlPacket = new RTPSyncPacket(seq, timestamp, ssrc, RTPTimestampPacket.ComputeBaseTime(syncticks));
                        break;

                    default: throw new FormatException("RTPPacket.Parse() does not know what class to use for Action=" + a);
                }
                return controlPacket;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RTPPacket))
                return false;

            RTPPacket p = obj as RTPPacket;
            if (p.SequenceNumber == this.SequenceNumber && p.Timestamp == this.Timestamp && p.SyncSource == this.SyncSource) //don't check payload because it's slow.
                return true;
            else return false;
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
