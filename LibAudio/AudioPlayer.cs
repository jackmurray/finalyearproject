using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
    }
}
