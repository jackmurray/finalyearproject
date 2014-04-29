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
        private Stream basestream;
        public bool Playing { get; protected set; }
        bool firstRun = true;
        private int bytesWritten = 0, startBytes = 0;
        public delegate void startcallback();

        public ushort bitspersample, channels, freq;

        private startcallback thecallback;

        public LoopbackWavCapture(Stream s, int startBytes, startcallback cb)
        {
            loopback = new WasapiLoopbackCapture();

            bitspersample = (ushort) loopback.WaveFormat.BitsPerSample;
            channels = (ushort) loopback.WaveFormat.Channels;
            freq = (ushort) loopback.WaveFormat.SampleRate;

            basestream = s;
            thecallback = cb;
            this.startBytes = startBytes;
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

            basestream.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
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
            basestream.Close();
        }
    }
}
