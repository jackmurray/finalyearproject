using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using LibConfig;
using LibSecurity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibService;

namespace UnitTest
{
    /// <summary>
    /// Summary description for LibService
    /// </summary>
    [TestClass]
    public class LibService
    {
        [TestMethod]
        public void TestHttpSerialize()
        {
            HttpMessage m = new HttpMessage()
                {
                    Method = HttpMethod.GET,
                    Type = HttpMessageType.REQUEST,
                    URL = "/test/1"
                };
            m.Headers.Add("Test", "ASDF");
            m.Headers.Add("Host", "example");

            string s = m.Serialize();
            Assert.AreEqual(@"GET /test/1 HTTP/1.0
Test: ASDF
Host: example

", s);

        }

        [TestMethod]
        public void TestHttpParse()
        {
            string s = @"GET /test/1 HTTP/1.0
Test: ASDF
Host: example

";
            HttpMessage m = HttpMessage.Parse(s);
        }

        [TestMethod]
        public void TestFindByteSequence()
        {
            Assert.AreEqual(2, HttpMessage.FindByteSequence(new byte[] { 0x01 }, new byte[] { 0x00, 0x00, 0x01, 0x00 }, 0));
            Assert.AreEqual(0, HttpMessage.FindByteSequence(new byte[] { 0x01 }, new byte[] { 0x01 }, 0));
            Assert.AreEqual(-1, HttpMessage.FindByteSequence(new byte[] { 0x01 }, new byte[] { 0x00 }, 0));
            Assert.AreEqual(2, HttpMessage.FindByteSequence(new byte[] { 0x01, 0x01 }, new byte[] { 0x00, 0x00, 0x01, 0x01 }, 0));
            Assert.AreEqual(-1, HttpMessage.FindByteSequence(new byte[] { 0x01, 0x01 }, new byte[] { 0x00, 0x00, 0x00, 0x01 }, 0));
        }

        [TestMethod]
        public void TestSSLConnection()
        {
            var key = KeyManager.GetKey();
            var cert = CertManager.GetCert(key);
            TrustedKeys.Add(cert.ToDotNetPublicCert()); //make sure we trust ourselves
            SslServer s = new SslServer(cert.ToDotNetCert(key));
            s.Listen(10451);

            SslClient c = new SslClient(cert.ToDotNetCert(key));
            c.Connect(new System.Net.IPEndPoint(IPAddress.Loopback, 10451));

            c.Close();
            s.Stop();
        }
    }
}
