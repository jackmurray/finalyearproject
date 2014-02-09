using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace LibAudio
{
    public static class SDLOutput
    {
        private static uint dev;
        private static SDL.SDL_AudioSpec want, have;
        private static bool Inited = false;

        public static SDL.SDL_AudioSpec AudioSpec { get { return have; } }
        public static int MaxVolume { get { return SDL.SDL_MIX_MAXVOLUME; } }

        public static void Init()
        {
            if (!Inited)
            {
                SDL.SDL_Init(SDL.SDL_INIT_AUDIO);
                Inited = true;
            }
        }

        public static void OpenDevice(SDL.SDL_AudioCallback callback, int freq, byte channels, ushort samples, byte bitspersample)
        {
            string devname = SDL.SDL_GetAudioDeviceName(0, 0);

            ushort format;

            switch (bitspersample)
            {
                case 32:
                    format = SDL.AUDIO_F32; //float 32 (single)
                    break;
                case 16:
                    format = SDL.AUDIO_S16; //signed 16-bit
                    break;
                case 8:
                    format = SDL.AUDIO_U8; //unsigned 8-bit
                    break;
                default:
                    throw new FormatException("Unsupported audio sample size (" + bitspersample + ")");
            }

            want = new SDL.SDL_AudioSpec()
            {
                freq = freq,
                channels = channels,
                samples = samples,
                format = format,
                callback = callback
            };

            dev = SDL.SDL_OpenAudioDevice(devname, 0, ref want, out have, (int)SDL.SDL_AUDIO_ALLOW_FORMAT_CHANGE);
            LibTrace.Trace.GetInstance("SDL").Verbose(string.Format("Initialised playback at {0}-bit {1}Hz {2}CH", bitspersample, freq, channels));
        }

        public static void Play()
        {
            SDL.SDL_PauseAudioDevice(dev, 0); //unpause the device (it will always be opened paused)
        }

        public static void Pause()
        {
            SDL.SDL_PauseAudioDevice(dev, 1);
        }

        /// <summary>
        /// Stop playback.
        /// </summary>
        public static void Stop()
        {
            SDL.SDL_CloseAudioDevice(dev);
        }
    }
}
