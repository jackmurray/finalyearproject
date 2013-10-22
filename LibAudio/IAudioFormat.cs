using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public interface IAudioFormat
    {
        void Parse();
        bool CheckMagic();
        byte[] GetFrame();
        Tuple<float, byte[]> GetDataForTime(float time);
        bool EndOfFile();

        int BitRate {get;}
        int Frequency {get;}
    }
}
