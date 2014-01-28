using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibAudio
{
    public interface IAudioFormat
    {
        /// <summary>
        /// Populate any internal structures.
        /// </summary>
        void Parse();

        /// <summary>
        /// Does the input stream have a valid header (or other verification method) for this file format?
        /// </summary>
        /// <returns></returns>
        bool CheckMagic();

        /// <summary>
        /// Return one frame of audio data. What constitutes a frame is defined by the file format in question,
        /// and in cases such as WAV where there is no frame structure, what is returned is entirely the choice
        /// of the implementing class.
        /// </summary>
        /// <returns></returns>
        byte[] GetFrame();

        /// <summary>
        /// The length of one frame of audio data in milliseconds.
        /// </summary>
        /// <returns></returns>
        double GetFrameLength();

        /// <summary>
        /// Are we at the end of the file?
        /// </summary>
        /// <returns></returns>
        bool EndOfFile();

        /// <summary>
        /// Seek forwards to the start of the stream.
        /// </summary>
        void SeekToStart();

        /// <summary>
        /// Return the file header. For files such as MP3 that don't have a single file header, an empty
        /// array is acceptable.
        /// </summary>
        /// <returns></returns>
        byte[] GetHeader();

        ushort Frequency { get; }
        byte Channels { get; }
        ushort SamplesPerFrame { get; } //The *total* number of samples this format will return per frame. NOT the number of samples/channel/frame.
        byte BitsPerSample { get; }

        SupportedFormats Format { get; }
    }

    public enum SupportedFormats
    {
        WAV,
        MP3
    }
}
