using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using LibConfig;
using SDL2;

namespace LibAudio
{
    public class AudioPlayer
    {
        public delegate void AudioCallback(IntPtr user, IntPtr buffer, int Length); //hide the internals of SDL from the users of this assembly.

        private SDL.SDL_AudioCallback sdlcallback; //must keep a reference so it won't bt GC'd.
        
        public AudioPlayer()
        {
            Reset();
        }

        public void Reset()
        {
            SDLOutput.Init();
        }

        public void Write(byte[] data)
        {

        }

        /// <summary>
        /// Set up the AudioPlayer object ready for playback.
        /// </summary>
        /// <param name="header">The header of the audio file from which to extract the necessary information.</param>
        public void Setup(AudioCallback callback, byte[] header)
        {
            IAudioFormat af = SupportedAudio.FindReaderForFile(new AudioStreamReader(new MemoryStream(header)));
            this.Setup(callback, af.Frequency, af.Channels);
        }

        public void Setup(AudioCallback callback, int freq, byte channels)
        {
            this.sdlcallback = new SDL.SDL_AudioCallback(callback);
            SDLOutput.OpenDevice(sdlcallback, freq, channels, 8192);
        }

        public void Start()
        {
            SDLOutput.Play();
        }

        public void Pause()
        {
            SDLOutput.Pause();
        }

        public void Stop()
        {
            /*
             * SDL runs the callback on its own internal thread. The callback then processes packets trying to fill SDL’s buffer,
             * and executes control packets when it sees them. The Stop control packet then causes this same thread to eventually
             * call SDL_CloseAudioDevice(). That function then calls (on Windows anyway), WaitForSingleObject() passing the handle
             * to the callback thread and an infinite timout, so that the callback can finish executing before the thread is terminated.
             * This obviously causes a deadlock as the thread is left blocked waiting for itself to finish. Fix is pretty simple –
             * have the wrapper code that would call down into SDL_CloseAudioDevice() to fire up a new short-lived thread to actually do the call.
             * This means that the callback will be able to return, and WFSO will be able to return and halt the thread.
             */
            new Thread(SDLOutput.Stop).Start();
        }
    }
}
