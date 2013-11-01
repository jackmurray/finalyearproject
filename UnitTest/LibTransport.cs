using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using LibTransport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class LibTransport
    {
        [TestMethod]
        [ExpectedExceptionAttribute(typeof(ArgumentException))]
        public void TestRTPOutputStreamConstructorFail_OutOfRange1()
        {
            new RTPOutputStream(new IPEndPoint(IPAddress.Parse("223.255.255.255"), 80));
            
        }

        [TestMethod]
        [ExpectedExceptionAttribute(typeof(ArgumentException))]
        public void TestRTPOutputStreamConstructorFail_OutOfRange2()
        {
            new RTPOutputStream(new IPEndPoint(IPAddress.Parse("240.0.0.0"), 80));
            
        }

        [TestMethod]
        [ExpectedExceptionAttribute(typeof(ArgumentException))]
        public void TestRTPOutputStreamConstructorFail_WrongAF()
        {
            new RTPOutputStream(new IPEndPoint(IPAddress.Parse("::1"), 80));
        }
        
        [TestMethod]
        public void TestRTPOutputStreamConstructor()
        {
            new RTPOutputStream(new IPEndPoint(IPAddress.Parse("224.0.0.0"), 80));
            new RTPOutputStream(new IPEndPoint(IPAddress.Parse("239.255.255.255"), 80));
        }

        [TestMethod]
        public void TestRTPOutputStreamSend()
        {
            RTPPacket p = new RTPDataPacket(false, 1, 1, 1, new byte[] {0xDE, 0xAD, 0xBE, 0xEF});
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("224.1.1.1"), 1000);
            new RTPOutputStream(ep).Send(p);
        }

        [TestMethod]
        public void TestRTPPacketTimeStamp()
        {
            RTPPacket.BuildTimestamp(DateTime.UtcNow);
        }

        [TestMethod]
        public void TestRTPPacketParse()
        {
            var p = new RTPDataPacket(false, 1234, 5678, 9090, new byte[] {0x13, 0x37});
            var p2 = RTPPacket.Parse(p.Serialise());
            Assert.AreEqual(p.Marker, p2.Marker);
            Assert.AreEqual(p.Padding, p2.Padding);
            Assert.AreEqual(p.SequenceNumber, p2.SequenceNumber);
            Assert.IsTrue(p.Payload.SequenceEqual(p2.Payload));
            Assert.AreEqual(p.SyncSource, p2.SyncSource);
            Assert.AreEqual(p.Timestamp, p2.Timestamp);
        }
    }
}
