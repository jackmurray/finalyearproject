using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace LibAudio
{
    public class LoopbackWavCapture
    {
        private WasapiLoopbackCapture loopback = new WasapiLoopbackCapture();
        private FileStream fs = new FileStream("C:\\temp\\samples.wav", FileMode.OpenOrCreate);
        private WaveFileWriter w;
        public bool Playing { get; protected set; }

        public LoopbackWavCapture()
        {
            w = new WaveFileWriter(fs, loopback.WaveFormat);
            loopback.DataAvailable += LoopbackOnDataAvailable;
            loopback.StartRecording();
            Playing = true;
        }

        private void LoopbackOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            w.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        public void Stop()
        {
            loopback.StopRecording();
            loopback.DataAvailable -= LoopbackOnDataAvailable;
            Playing = false;
            w.Close();
        }
    }
}
