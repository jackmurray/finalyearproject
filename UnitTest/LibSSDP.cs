using System;
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
    }
}
