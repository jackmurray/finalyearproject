using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using LibAudio;
using LibSecurity;

namespace LibTransport
{
    public class RTPInputStream : RTPStreamBase
    {
        protected Verifier verifier;
        public SupportedFormats Format;

        public RTPInputStream(IPEndPoint ep) : base(ep)
        {
            c.Client.Bind(new IPEndPoint(IPAddress.Any, ep.Port));
        }

        public void SetReceiveTimeout(int ms)
        {
            c.Client.ReceiveTimeout = ms;
        }

        public RTPPacket Receive()
        {
            IPEndPoint remoteHost = null;
            byte[] data = c.Receive(ref remoteHost);

            try
            {
                RTPPacket p;
                if (useAuthentication)
                    p = RTPPacket.Parse(data, pekm, verifier);
                else if (useEncryption)
                    p = RTPPacket.Parse(data, pekm);
                else
                    p = RTPPacket.Parse(data);

                this.seq = p.SequenceNumber;
                if (!p.Marker) DecodePayload(p as RTPDataPacket);
                return p;
            }
            catch (CryptographicException ex)
            {
                Log.Warning("CryptographicException in RTPPacket.Receive(): " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Sets the next key in the PEKM to the arguments. Call RotateKey() to use this new key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nonce"></param>
        public void DeliverNewKey(byte[] key, byte[] nonce)
        {
            this.pekm.SetNextKey(key, nonce);
        }

        /// <summary>
        /// Changes the key in the PEKM to the one set by DeliverNewKey
        /// </summary>
        public void RotateKey()
        {
            this.pekm.UseNextKey();
            Log.Verbose("RTPInputStream: last seq we saw was " + this.seq);
        }

        public void EnableVerification(Verifier v)
        {
            this.useAuthentication = true;
            this.verifier = v;
            Log.Information("Enabling RTP signature verification");
        }

        private void DecodePayload(RTPDataPacket p)
        {
            if (Format == SupportedFormats.WAV)
                return;

            if (Format == SupportedFormats.MP3)
                p.Payload = MP3Decoder.Decode(p.Payload);
        }
    }
}
