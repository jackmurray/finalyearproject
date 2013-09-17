using System;
using System.Security.Cryptography;
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

        [TestMethod]
        public void TestChallengeResponse()
        {
            ChallengeResponse cr = new ChallengeResponse();
            var key = System.Text.Encoding.UTF8.GetBytes("KEY");
            byte[] sig = cr.Sign(key);
            Assert.IsTrue(cr.Verify(key, sig));

            var cr2 = new ChallengeResponse();
            Assert.IsFalse(cr2.Verify(key, sig)); //will fail because the challenge is different now.

            for (int i = 0; i < sig.Length; i++)
                sig[i] = 0x00;
            Assert.IsFalse(cr.Verify(key, sig)); //fails because the sig is invalid now
        }
    }
}
