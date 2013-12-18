using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibSecurity;

namespace UnitTest
{
    [TestClass]
    public class LibSecurity
    {
        private readonly byte[] NIST_DATA1 = {0x6b, 0xc1, 0xbe, 0xe2, 0x2e, 0x40, 0x9f, 0x96, 0xe9, 0x3d, 0x7e, 0x11, 0x73, 0x93, 0x17, 0x2a};

        private readonly byte[] NIST_RESULT1 = {0x87, 0x4d, 0x61, 0x91, 0xb6, 0x20, 0xe3, 0x26, 0x1b, 0xef, 0x68, 0x64, 0x99, 0x0d, 0xb6, 0xce};

        [TestMethod]
        public void TestGenerateKey()
        {
            KeyManager.CreatePermenantKey();
        }

        [TestMethod]
        public void TestGenerateCert()
        {
            CertManager.Create(KeyManager.CreatePermenantKey());
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

        private PacketEncrypter SetupEncrypter(bool forEncryption)
        {
            //this is the NIST test data for AES-CTR
            byte[] key = new byte[] { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c, };
            byte[] nonce = new byte[] { 0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7 };
            long ctr = -506097522914230529; //0xf8f9fafbfcfdfeff in signed long
            return new PacketEncrypter(new PacketEncrypterKeyManager(key, nonce), ctr, forEncryption);
        }

        [TestMethod]
        public void TestPacketCryptoExactBlock()
        {
            PacketEncrypter enc = SetupEncrypter(true);
            byte[] output = enc.Encrypt(NIST_DATA1);
            Assert.IsTrue(
                output.SequenceEqual(NIST_RESULT1));
        }

        [TestMethod]
        public void TestPacketCryptoPartialBlock()
        {
            PacketEncrypter enc = SetupEncrypter(true);
            byte[] output = enc.Encrypt(NIST_DATA1.Take(NIST_DATA1.Length-1).ToArray());
            Assert.IsTrue(
                output.SequenceEqual(NIST_RESULT1.Take(NIST_RESULT1.Length-1).ToArray()));
        }

        [TestMethod]
        public void TestPacketCryptoDecryptExactBlock()
        {
            PacketEncrypter enc = SetupEncrypter(true);
            byte[] cipher = enc.Encrypt(NIST_DATA1);
            enc = SetupEncrypter(false);
            byte[] plain = enc.Decrypt(cipher);
            Assert.IsTrue(plain.SequenceEqual(NIST_DATA1));
        }

        [TestMethod]
        public void TestPacketCryptoDecryptPartialBlock()
        {
            PacketEncrypter enc = SetupEncrypter(true);
            byte[] cipher = enc.Encrypt(NIST_DATA1.Take(NIST_DATA1.Length - 1).ToArray());
            enc = SetupEncrypter(false);
            byte[] plain = enc.Decrypt(cipher);
            Assert.IsTrue(plain.SequenceEqual(NIST_DATA1.Take(NIST_DATA1.Length - 1).ToArray()));
        }
    }
}
