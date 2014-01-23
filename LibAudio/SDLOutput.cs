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

        public static void Init()
        {
            SDL.SDL_Init(SDL.SDL_INIT_AUDIO);
        }

        public static void OpenDevice(SDL.SDL_AudioCallback callback, int freq, byte channels, ushort samples)
        {
            string devname = SDL.SDL_GetAudioDeviceName(0, 0);

            want = new SDL.SDL_AudioSpec()
            {
                freq = freq,
                channels = channels,
                samples = samples,
                format = SDL.AUDIO_F32,
                callback = callback
            };

            dev = SDL.SDL_OpenAudioDevice(devname, 0, ref want, out have, (int)SDL.SDL_AUDIO_ALLOW_FORMAT_CHANGE);
        }

        public static void Play()
        {
            SDL.SDL_PauseAudioDevice(dev, 0); //unpause the device (it will always be opened paused)
        }
    }
}
