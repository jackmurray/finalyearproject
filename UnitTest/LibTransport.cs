﻿using System;
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
    }
}
