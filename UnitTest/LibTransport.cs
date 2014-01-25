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
            DateTime dt = DateTime.UtcNow;
            dt = dt.Subtract(new TimeSpan(0, 0, 0, 0, dt.Millisecond)); //strip off the milliseconds because in the real use-case we won't have any.
            uint timestamp = RTPPacket.BuildTimestamp(2000);
            DateTime dt2 = RTPPacket.BuildDateTime(timestamp, dt);
            Assert.AreEqual(dt.AddSeconds(2), dt2);
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

            var p3 = new RTPControlPacket(RTPControlAction.Play, null, 1234, 5678, 9090);
            var p4 = RTPPacket.Parse(p3.Serialise());
            Assert.IsTrue(p3.Payload.SequenceEqual(p4.Payload));
            //no need to test anything other than payload as the logic/code doesn't change for the other fields.

            p3 = new RTPControlPacket(RTPControlAction.Pause, new byte[]{0x01, 0x02}, 1234, 5678, 9090);
            p4 = RTPPacket.Parse(p3.Serialise());
            Assert.IsTrue(p3.Payload.SequenceEqual(p4.Payload));
        }
    }
}
