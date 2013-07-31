using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibTrace;

namespace UnitTest
{
    [TestClass]
    public class LibTrace
    {
        [TestMethod]
        public void TestMonoTracing()
        {
            global::LibTrace.Trace t = global::LibTrace.Trace.GetInstance("LibTraceTest");

            PrivateObject p = new PrivateObject(t);
            Assert.IsTrue((bool)p.Invoke("ShouldLog", SourceLevels.All, TraceEventType.Critical));
            Assert.IsTrue((bool)p.Invoke("ShouldLog", SourceLevels.All, TraceEventType.Error));
            Assert.IsTrue((bool)p.Invoke("ShouldLog", SourceLevels.All, TraceEventType.Warning));
            Assert.IsTrue((bool)p.Invoke("ShouldLog", SourceLevels.All, TraceEventType.Information));
            Assert.IsTrue((bool)p.Invoke("ShouldLog", SourceLevels.All, TraceEventType.Verbose));

            Assert.IsTrue((bool)p.Invoke("ShouldLog", SourceLevels.Critical, TraceEventType.Critical));
            Assert.IsFalse((bool)p.Invoke("ShouldLog", SourceLevels.Critical, TraceEventType.Error));
            Assert.IsFalse((bool)p.Invoke("ShouldLog", SourceLevels.Critical, TraceEventType.Warning));
            Assert.IsFalse((bool)p.Invoke("ShouldLog", SourceLevels.Critical, TraceEventType.Information));
            Assert.IsFalse((bool)p.Invoke("ShouldLog", SourceLevels.Critical, TraceEventType.Verbose));
        }
    }
}
