using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibSecurity;

namespace UnitTest
{
    [TestClass]
    public class LibSecurity
    {
        [TestMethod]
        public void TestKeyGeneration()
        {
            var key = KeyManager.Create();
            key.WriteKeyToFile("key.txt");
        }
    }
}
