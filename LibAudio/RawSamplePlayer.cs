using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace LibAudio
{
    public class RawSamplePlayer
    {
        private WaveOut output;
        private BufferedWaveProvider buf;

        public RawSamplePlayer()
        {
            output = new WaveOut();
            buf = new BufferedWaveProvider(new WasapiLoopbackCapture().WaveFormat);
            output.Init(buf);
        }

        public void Play(byte[] data)
        {
            buf.AddSamples(data, 0, Math.Min(data.Length, buf.BufferLength));
            output.Play();
        }
    }
}
