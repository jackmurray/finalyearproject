using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibAudio;

namespace UnitTest
{
    [TestClass]
    public class LibAudio
    {
        [TestMethod]
        public void TestID3Parse()
        {
            ID3Tag t = new ID3Tag(File.OpenRead("D:\\Music\\Lacuna Coil\\Dark Adrenaline\\End of Time.mp3"));
            t.Parse();
            Assert.AreEqual(t.Size, (uint)0x08C6);

            t = new ID3Tag(File.OpenRead("D:\\Music\\Icon For Hire\\Scripted\\The Grey.mp3"));
            t.Parse();
            Assert.AreEqual(t.Size, (uint)0x017ED5);
        }
    }
}
