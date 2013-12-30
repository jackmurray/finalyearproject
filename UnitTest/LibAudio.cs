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
            ID3Tag t = new ID3Tag(new AudioFileReader(File.OpenRead("D:\\Music\\Lacuna Coil\\Dark Adrenaline\\End of Time.mp3")));
            t.Parse();
            Assert.AreEqual(t.Size, (uint)0x08C6);

            t = new ID3Tag(new AudioFileReader(File.OpenRead("D:\\Music\\Icon For Hire\\Scripted\\The Grey.mp3")));
            t.Parse();
            Assert.AreEqual(t.Size, (uint)0x017ED5);
        }

        [TestMethod]
        public void MP3WithID3Header()
        {
            AudioFileReader s = new AudioFileReader(File.OpenRead("D:\\Music\\Lacuna Coil\\Dark Adrenaline\\End of Time.mp3"));
            ID3Tag t = new ID3Tag(s);
            t.Parse();
            MP3Format mp3 = new MP3Format(s);
            s.Skip((int)t.Size);
            mp3.Parse();
            Assert.AreEqual(mp3.BitRate, 192000);
            Assert.AreEqual(mp3.Frequency, 44100);

            s = new AudioFileReader(File.OpenRead("D:\\Music\\Icon For Hire\\Scripted\\The Grey.mp3"));
            t = new ID3Tag(s);
            t.Parse();
            mp3 = new MP3Format(s);
            s.Skip((int)t.Size);
            mp3.Parse();
            Assert.AreEqual(mp3.BitRate, 320000);
            Assert.AreEqual(mp3.Frequency, 44100);
        }
    }
}
