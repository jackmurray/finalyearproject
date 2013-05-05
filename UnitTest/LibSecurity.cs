using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibSecurity;

namespace UnitTest
{
    [TestClass]
    public class LibSecurity
    {
        [TestMethod]
        public void TestGenerateKey()
        {
            KeyManager.Create();
        }

        [TestMethod]
        public void TestGenerateCert()
        {
            CertManager.Create(KeyManager.Create());
        }
    }
}
