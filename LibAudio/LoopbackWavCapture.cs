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
        private WaveFileWriter w;
        private Stream basestream;
        public bool Playing { get; protected set; }

        public LoopbackWavCapture(Stream s)
        {
            basestream = s;
            w = new WaveFileWriter(s, loopback.WaveFormat);
            loopback.DataAvailable += LoopbackOnDataAvailable;
            loopback.StartRecording();
            Playing = true;
        }

        private void LoopbackOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            //This callback is executed on the Recording thread started by the WasapiLoopbackCapture object.
            //We lock the stream (yeah it's bad practice but I don't want to introduce a holding object for the 
            //stream yet) and write the data to it.
            lock (basestream)
            {
                w.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
            }
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
