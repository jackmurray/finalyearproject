using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibService;

namespace UnitTest
{
    /// <summary>
    /// Summary description for LibService
    /// </summary>
    [TestClass]
    public class LibService
    {
        [TestMethod]
        public void TestHttpSerialize()
        {
            HttpMessage m = new HttpMessage()
                {
                    Method = HttpMethod.GET,
                    Type = HttpMessageType.REQUEST,
                    URL = "/test/1"
                };
            m.Headers.Add("Test", "ASDF");
            m.Headers.Add("Host", "example");

            string s = m.Serialize();
            Assert.AreEqual(@"GET /test/1 HTTP/1.0
Test: ASDF
Host: example

", s);

        }

        [TestMethod]
        public void TestHttpParse()
        {
            string s = @"GET /test/1 HTTP/1.0
Test: ASDF
Host: example

";
            HttpMessage m = HttpMessage.Parse(s);
        }
    }
}
