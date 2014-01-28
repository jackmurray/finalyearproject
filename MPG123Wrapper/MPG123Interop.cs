using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MPG123Wrapper
{
    public static class MPG123Interop
    {
        public const int MPG123_OK = 0;
        public const int MPG123_ERR = -1;
        public const int MPG123_NEW_FORMAT = -11;
        public const int MPG123_NEED_MORE = -10;
        public const int MPG123_DONE = -12;

        //we only care about this format so we're not going to copy/paste everything.
        public const int MPG123_ENC_FLOAT_32 = 0x200;

        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_init();
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mpg123_exit();
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr mpg123_new(string decoder, IntPtr error);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void mpg123_delete(IntPtr mh);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int mpg123_open(IntPtr mh, string path);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_read(IntPtr mh, byte[] outmemory, int outmemsize, out IntPtr done);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_outblock(IntPtr mh);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_seek(IntPtr mh, int sampleoff, int whence);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_id3(IntPtr mh, out IntPtr v1, out IntPtr v2);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_getformat(IntPtr mh, out IntPtr rate, out IntPtr channels, out IntPtr encoding);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_decode(IntPtr mh, IntPtr inmemory, int inmemsize, IntPtr outmemory, int outmemsize, IntPtr done);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_format_none(IntPtr mh);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_format_all(IntPtr mh);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_format(IntPtr mh, int rate, int channels, int encodings);

        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_errcode(IntPtr mh);
        [DllImport("libmpg123.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int mpg123_open_feed(IntPtr mh);
    }
}
