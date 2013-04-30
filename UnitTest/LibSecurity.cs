using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibSecurity;

namespace UnitTest
{
    [TestClass]
    public class LibSecurity
    {
        [TestMethod]
        public void TestHash()
        {
            string hash = Hasher.Create().HashHex(System.Text.Encoding.ASCII.GetBytes("TEST"));
            Assert.AreEqual("94ee059335e587e501cc4bf90613e0814f00a7b08bc7c648fd865a2af6a22cc2", hash);
        }
    }
}
