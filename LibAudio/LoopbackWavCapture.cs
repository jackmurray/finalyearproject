using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace LibAudio
{
    public class LoopbackWavCapture
    {
        private WasapiLoopbackCapture loopback;
        private WaveFileWriter w;
        private Stream basestream;
        public bool Playing { get; protected set; }
        bool firstRun = true;
        private int bytesWritten = 0, startBytes = 0;
        public delegate void startcallback();

        private startcallback thecallback;

        public LoopbackWavCapture(Stream s, int startBytes, startcallback cb)
        {
            loopback = new WasapiLoopbackCapture();
            basestream = s;
            thecallback = cb;
            this.startBytes = startBytes;
            w = new WaveFileWriter(s, loopback.WaveFormat);
            loopback.DataAvailable += LoopbackOnDataAvailable;
            loopback.StartRecording();
            Playing = true;
        }

        private void LoopbackOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            if (firstRun)
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                firstRun = false;
            }

            w.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
            bytesWritten += waveInEventArgs.BytesRecorded;

            if (startBytes != 0 && bytesWritten >= startBytes)
            {
                startBytes = 0;
                thecallback();
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
