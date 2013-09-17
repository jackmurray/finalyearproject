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
    }
}
