using System;
using System.Net;
using LibService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using global::LibCommon;

namespace UnitTest
{
    [TestClass]
    public class LibCommon
    {
        [TestMethod]
        public void TestDeviceConnect()
        {
            SslServer s = TestUtil.GetSSLServer();
            Device d = new Receiver(new System.Net.IPEndPoint(IPAddress.Loopback, 10451), s.GetFingerprint());

            d.GetSsl(TestUtil.GetCert(TestUtil.GetKey()), TestUtil.GetKey()).Close();
            
        }
    }
}
