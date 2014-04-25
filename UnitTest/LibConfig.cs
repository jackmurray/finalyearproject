using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using LibConfig;
using LibUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class LibConfig
    {
        [TestMethod]
        public void TestTrustDatabaseAdd()
        {
            Config.LoadTrustedKeys();
            ClearTrustDatabase();
            X509Certificate cert = TestUtil.GetCert(TestUtil.GetKey()).ToDotNetPublicCert();
            Assert.IsFalse(TrustedKeys.Contains(cert.GetCertHashString()));
            TrustedKeys.Add(cert);
            Assert.IsTrue(TrustedKeys.Contains(cert.GetCertHashString()));
        }

        [TestMethod]
        public void ClearTrustDatabase()
        {
            foreach (var f in TrustedKeys.GetAllKeys())
                TrustedKeys.Remove(f);

            Assert.AreEqual(0, TrustedKeys.GetAllKeys().Count);
        }

        [TestMethod]
        public void TestLoadSave()
        {
            //load the keys, clear them all and add a single one. save. load again. make sure the one, and only the one, we just added is there
            Config.LoadTrustedKeys();
            TestTrustDatabaseAdd();
            Config.SaveTrustedKeys();
            Config.LoadTrustedKeys();
            Assert.AreEqual(1, TrustedKeys.GetAllKeys().Count);
            Assert.IsTrue(TrustedKeys.Contains(TestUtil.GetCert(TestUtil.GetKey()).ToDotNetPublicCert().GetCertHashString()));
        }
    }
}
