using System;
using System.Text;
using System.Threading;
using LibConfig;
using LibSecurity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using global::LibSSDP;

namespace UnitTest
{
    [TestClass]
    public class LibSSDP
    {
        [TestMethod]
        public void TestPacketConstructAndSerialise()
        {
            SSDPPacket p = new SSDPAnnouncePacket();
            var key = KeyManager.GetKey();
            var cert = CertManager.GetCert(key);
            p = new SSDPAnnouncePacket(key, cert);
            p.Serialize();

            p = new SSDPResponsePacket(key, cert);
            p.Serialize();

            p = new SSDPSearchPacket();
            p.Serialize();
        }

        [TestMethod]
        public void TestPacketParse()
        {
            var key = KeyManager.GetKey();
            var cert = CertManager.GetCert(key);
            SSDPAnnouncePacket p = new SSDPAnnouncePacket(key, cert);
            SSDPAnnouncePacket p2 = (SSDPAnnouncePacket) SSDPPacket.Parse(Encoding.ASCII.GetString(p.Serialize()));

            string s1 = global::LibUtil.Util.FormatDate(p.Date);
            string s2 = global::LibUtil.Util.FormatDate(p2.Date);
            Assert.AreEqual(s1, s2);
            Assert.AreEqual(p.Method, p2.Method);
            Assert.AreEqual(p.fingerprint, p2.fingerprint);
            Assert.AreEqual(p.friendlyName, p2.friendlyName);
            Assert.AreEqual(p.Location, p2.Location);
        }

        [TestMethod]
        public void TestPacketSignature()
        {
            var key = KeyManager.GetKey();
            var cert = CertManager.GetCert(key);
            Config.LoadTrustedKeys();
            TrustedKeys.Add(cert.ToDotNetPublicCert());

            SSDPAnnouncePacket p = new SSDPAnnouncePacket(key, cert);
            byte[] serialize = p.Serialize();
            SSDPAnnouncePacket p2 = (SSDPAnnouncePacket)SSDPPacket.Parse(Encoding.ASCII.GetString(serialize));
            
            byte[] stripped = Encoding.ASCII.GetBytes(Util.StripPacket(serialize));
            Assert.IsTrue(Util.CheckSignature(p2, stripped));
        }
    }
}
