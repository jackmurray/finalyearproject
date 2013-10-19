using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public enum HeaderType
    {
        ID3,
        MP3
    };

    /// <summary>
    /// MP3 bitrates, in KBit/s
    /// </summary>
    public enum BitRate
    {
        ThirtyTwo = 1, Fourty, FourtyEight, FiftySix, SixtyFour, Eighty, NinetySix, OneHundredTwelve, OneHundredTwentyEight, OneHundredSixty, OneHundredNinetyTwo, TwoHundredTwentyFour, TwoHundredFiftySix, ThreeHundredTwenty
    }

    /// <summary>
    /// MP3 sampling frequencies, in KHz
    /// </summary>
    public enum Frequency
    {
        FourtyFourPointOne = 0, FourtyEight, ThirtyTwo
    }
}
