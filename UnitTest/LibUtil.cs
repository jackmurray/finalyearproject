using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibUtil;

namespace UnitTest
{
    [TestClass]
    public class LibUtil
    {
        [TestMethod]
        public void TestHexStringToBytes()
        {
            byte[] result = Util.HexStringToBytes("0d");
            Assert.AreEqual(0x0D, result[0]);

            result = Util.HexStringToBytes("0001020304");
            Assert.IsTrue(result.SequenceEqual(new byte[] {0x00, 0x01, 0x02, 0x03, 0x04}));
        }

        [TestMethod]
        public void TestCircularStream()
        {
            CircularStream s = new CircularStream(10);
            s.Write(new byte[] {0x01, 0x01, 0x01, 0x01, 0x01}, 0, 5);
            Assert.IsTrue(s.Buffer.SequenceEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            s.Write(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, 0, 5);
            Assert.IsTrue(s.Buffer.SequenceEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }));
            s.Write(new byte[] { 0x02 }, 0, 1);
            Assert.IsTrue(s.Buffer.SequenceEqual(new byte[] { 0x02, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }));

            byte[] buf = new byte[5];
            s.Read(buf, 0, 5);
            Assert.IsTrue(buf.SequenceEqual(new byte[] { 0x02, 0x01, 0x01, 0x01, 0x01}));
            s.Read(buf, 0, 5);
            Assert.IsTrue(buf.SequenceEqual(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }));
            s.Read(buf, 0, 5);
            Assert.IsTrue(buf.SequenceEqual(new byte[] { 0x02, 0x01, 0x01, 0x01, 0x01 }));
        }
    }
}
