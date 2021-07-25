#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Standard.Audio;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.ComBaseApi.COM
{
    /// <summary>
    /// AUDCLNT_SHAREMODE
    /// </summary>
    public enum AudioClientShareMode
    {
        /// <summary>
        /// AUDCLNT_SHAREMODE_SHARED,
        /// </summary>
        Shared,

        /// <summary>
        /// AUDCLNT_SHAREMODE_EXCLUSIVE
        /// </summary>
        Exclusive
    }

    /// <summary>
    /// AUDCLNT_STREAMFLAGS
    /// </summary>
    [Flags]
    public enum AudioClientStreamFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// AUDCLNT_STREAMFLAGS_CROSSPROCESS
        /// </summary>
        CrossProcess = 0x00010000,

        /// <summary>
        /// AUDCLNT_STREAMFLAGS_LOOPBACK
        /// </summary>
        Loopback = 0x00020000,

        /// <summary>
        /// AUDCLNT_STREAMFLAGS_EVENTCALLBACK
        /// </summary>
        EventCallback = 0x00040000,

        /// <summary>
        /// AUDCLNT_STREAMFLAGS_NOPERSIST
        /// </summary>
        NoPersist = 0x00080000
    }

    /// <summary>
    /// Summary description for WaveFormatEncoding.
    /// </summary>
    public enum WaveFormatEncoding : ushort
    {
        /// <summary>WAVE_FORMAT_UNKNOWN,	Microsoft Corporation</summary>
        Unknown = 0x0000,

        /// <summary>WAVE_FORMAT_PCM		Microsoft Corporation</summary>
        Pcm = 0x0001,

        /// <summary>WAVE_FORMAT_ADPCM		Microsoft Corporation</summary>
        Adpcm = 0x0002,

        /// <summary>WAVE_FORMAT_IEEE_FLOAT Microsoft Corporation</summary>
        IeeeFloat = 0x0003,

        /// <summary>WAVE_FORMAT_EXTENSIBLE</summary>
        Extensible = 0xFFFE, // Microsoft 

        /// <summary></summary>
        WAVE_FORMAT_DEVELOPMENT = 0xFFFF
    }

    /// <summary>
    /// Represents a Wave file format
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormat
    {
        /// <summary>format type</summary>
        public WaveFormatEncoding Tag;

        /// <summary>number of channels</summary>
        public short Channels;

        /// <summary>sample rate</summary>
        public int SampleRate;

        /// <summary>for buffer estimation</summary>
        public int AverageBytesPerSecond;

        /// <summary>block size of data</summary>
        public short BlockAlign;

        /// <summary>number of bits per sample of mono data</summary>
        public short BitsPerSample;

        /// <summary>number of following bytes</summary>
        public short ExtraSize;

        public virtual bool IsFloat()
        {
            return Tag == WaveFormatEncoding.IeeeFloat;
        }

        public bool Equals(WaveFormat obj)
        {
            if (obj == null) return false;
            return obj.Channels == Channels && SampleRate == obj.SampleRate && BitsPerSample == obj.BitsPerSample && IsFloat() == obj.IsFloat();
        }

        public AudioFormat ToEmotionFormat()
        {
            return new AudioFormat(BitsPerSample, IsFloat(), Channels, SampleRate);
        }
    }

    [Flags]
    public enum ChannelMask
    {
        SpeakerFrontLeft = 0x1,
        SpeakerFrontRight = 0x2,
        SpeakerFrontCenter = 0x4,

        Mono = SpeakerFrontCenter,
        Stereo = SpeakerFrontLeft | SpeakerFrontRight
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormatExtensible : WaveFormat
    {
        public short ValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0
        public ChannelMask ChannelMask; // which channels are present in stream
        public Guid SubFormat;

        public static Guid SubFormatIEEEFloat = new Guid("00000003-0000-0010-8000-00aa00389b71");

        public static WaveFormatExtensible FromEmotionFormat(AudioFormat emFormat)
        {
            var nuFormat = new WaveFormatExtensible
            {
                BitsPerSample = (short) emFormat.BitsPerSample,
                ValidBitsPerSample = (short) emFormat.BitsPerSample,
                ExtraSize = 22,
                Channels = (short) emFormat.Channels,
                Tag = WaveFormatEncoding.Extensible,
                SampleRate = emFormat.SampleRate
            };
            if (emFormat.IsFloat) nuFormat.SubFormat = SubFormatIEEEFloat;
            nuFormat.ChannelMask = emFormat.Channels switch
            {
                1 => ChannelMask.Mono,
                2 => ChannelMask.Stereo,
                _ => nuFormat.ChannelMask
            };
            nuFormat.CalculateAlignAndAverage();
            return nuFormat;
        }

        public void CalculateAlignAndAverage()
        {
            BlockAlign = (short) (Channels * BitsPerSample / 8);
            AverageBytesPerSecond = SampleRate * BlockAlign;
        }

        public override bool IsFloat()
        {
            return SubFormat.Equals(SubFormatIEEEFloat);
        }
    }

    /// <summary>
    /// Windows CoreAudio IAudioClient interface
    /// Defined in AudioClient.h
    /// </summary>
    [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    internal interface IAudioClient
    {
        [PreserveSig]
        int Initialize(AudioClientShareMode shareMode,
            AudioClientStreamFlags streamFlags,
            long hnsBufferDuration, // REFERENCE_TIME
            long hnsPeriodicity, // REFERENCE_TIME
            IntPtr pFormat,
            [In] ref Guid audioSessionGuid);

        /// <summary>
        /// The GetBufferSize method retrieves the size (maximum capacity) of the endpoint buffer.
        /// </summary>
        int GetBufferSize(out uint bufferSize);

        [return: MarshalAs(UnmanagedType.I8)]
        long GetStreamLatency();

        int GetCurrentPadding(out int currentPadding);

        [PreserveSig]
        int IsFormatSupported(
            AudioClientShareMode shareMode,
            [In] WaveFormat pFormat,
            IntPtr closestMatchFormat);

        int GetMixFormat(out IntPtr deviceFormatPointer);

        // REFERENCE_TIME is 64 bit int        
        int GetDevicePeriod(out long defaultDevicePeriod, out long minimumDevicePeriod);

        int Start();

        int Stop();

        int Reset();

        int SetEventHandle(IntPtr eventHandle);

        /// <summary>
        /// The GetService method accesses additional services from the audio client object.
        /// </summary>
        /// <param name="interfaceId">The interface ID for the requested service.</param>
        /// <param name="interfacePointer">
        /// Pointer to a pointer variable into which the method writes the address of an instance of
        /// the requested interface.
        /// </param>
        [PreserveSig]
        int GetService([In] [MarshalAs(UnmanagedType.LPStruct)]
            Guid interfaceId, [Out] [MarshalAs(UnmanagedType.IUnknown)]
            out object interfacePointer);
    }
}