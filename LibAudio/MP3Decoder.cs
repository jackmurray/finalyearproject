﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LibTrace;
using MPG123Wrapper;

namespace LibAudio
{
    public static class MP3Decoder
    {
        private const int BUF_SIZE = 16384; //Trying to calc this value by doing 1152*channels*samplesize doesn't work for some frames, so just make it bigger (it's ~9k if you math it so we only waste a bit)

        /// <summary>
        /// Decoded sample size in *BITS*
        /// </summary>
        public const int SAMPLE_SIZE = 16;

        private static IntPtr handle;
        private static Trace Log;

        /// <summary>
        /// Sets up the MPG123 handle and sets the output format.
        /// </summary>
        public static void Init()
        {
            MPG123Interop.mpg123_init();
            handle = MPG123Interop.mpg123_new(null, IntPtr.Zero);

            MPG123Interop.mpg123_format_none(handle); //remove all output formats from the list of acceptable ones
            MPG123Interop.mpg123_format(handle, 44100, 2, MPG123Interop.MPG123_ENC_SIGNED_16); //force output to be stereo 44.1kHz signed 16-bit. if the input doesn't match this mpg123 will resample it for us.

            MPG123Interop.mpg123_open_feed(handle);
            MP3Decoder.Log = LibTrace.Trace.GetInstance("LibAudio");
        }

        public static byte[] Decode(byte[] mp3data)
        {
            IntPtr nativein, nativeout, nativedone;
            byte[] managedout = new byte[BUF_SIZE];
            int done;

            nativein = Marshal.AllocHGlobal(mp3data.Length);
            nativeout = Marshal.AllocHGlobal(BUF_SIZE);
            nativedone = AllocInt();

            Marshal.Copy(mp3data, 0, nativein, mp3data.Length); //copy our input data to the native input buffer;

            int ret = MPG123Interop.mpg123_decode(handle, nativein, mp3data.Length, nativeout, BUF_SIZE, nativedone);
            if (ret == MPG123Interop.MPG123_NEW_FORMAT)
            {
                LogFormatChange();
                ret = MPG123Interop.mpg123_decode(handle, nativein, mp3data.Length, nativeout, BUF_SIZE, nativedone); //do the decode again so we can get the data
            }
            else if (ret == MPG123Interop.MPG123_NEED_MORE)
            {
            }
            else if (ret != MPG123Interop.MPG123_OK)
            {
                int err = MPG123Interop.mpg123_errcode(handle);
                Log.Error("MPG123: Error " + err);
            }

            done = Marshal.ReadInt32(nativedone);
            Marshal.Copy(nativeout, managedout, 0, done);

            Marshal.FreeHGlobal(nativedone);
            Marshal.FreeHGlobal(nativein);
            Marshal.FreeHGlobal(nativeout);

            if (!managedout.Any(b => b != 0))
                Log.Verbose("MP3 packet decoded as all NULLs");

            //Log.Verbose("MP3 decoder output size: " + done);
            return managedout.Take(done).ToArray(); //only return as many bytes as there actually are of data
        }

        private static void LogFormatChange()
        {
            IntPtr rate, channels, encoding;

            MPG123Interop.mpg123_getformat(handle, out rate, out channels, out encoding);

            Log.Verbose(string.Format("libmpg123: Format changed to rate={0} channels={1} encoding={2}", rate, channels, encoding));
        }

        private static IntPtr AllocInt()
        {
            return Marshal.AllocHGlobal(sizeof (int));
        }
    }
}
