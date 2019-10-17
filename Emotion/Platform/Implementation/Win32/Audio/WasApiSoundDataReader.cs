#region Using

using System;
using System.Diagnostics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    /// <summary>
    /// Provides samples for the WasApi render client..
    /// </summary>
    public class WasApiSoundDataReader
    {
        public int SourceBytesPerFrame;
        public byte[] SoundData;
        public int SourceChannels;
        public int SourceBitsPerSample;
        public int SourceSampleRate;
        public bool IsFloat;

        public int ConvBytesPerFrame;
        public byte[] ConvertedSoundData;
        public WaveFormat ConvFormat;

        public int Pointer;

        public WasApiSoundDataReader(WaveSoundAsset wav)
        {
            SourceBytesPerFrame = wav.Channels * (wav.BitsPerSample / 8);
            SoundData = wav.SoundData;
            SourceChannels = wav.Channels;
            SourceBitsPerSample = wav.BitsPerSample;
            SourceSampleRate = wav.SampleRate;
            IsFloat = wav.IsFloat;

            ConvBytesPerFrame = 0;
            ConvertedSoundData = null;
            ConvFormat = null;

            Pointer = 0;
        }

        public void SetFormat(WaveFormat format)
        {
            // Initialize the converted data as a copy.
            ConvertedSoundData = new byte[SoundData.Length];
            new Span<byte>(SoundData).CopyTo(new Span<byte>(ConvertedSoundData));

            // Convert data format.
            bool isFloat = IsFloat;
            bool destFloat = format.IsFloat();
            if (SourceChannels != format.Channels || SourceBitsPerSample != format.BitsPerSample || isFloat != destFloat || SourceSampleRate != format.SampleRate)
                AudioContext.ConvertFormat(SourceBitsPerSample, isFloat, SourceSampleRate, SourceChannels,
                    format.BitsPerSample, destFloat, format.SampleRate, format.Channels,
                    ref ConvertedSoundData);

            ConvFormat = format;
            ConvBytesPerFrame = format.Channels * (format.BitsPerSample / 8);
        }

        public int Read(Span<byte> copyTo, int framesAmount)
        {
            // Check if a format is set.
            if (ConvertedSoundData == null)
            {
                Engine.Log.Warning("Unset output audio format.", MessageSource.Audio);
                Debug.Assert(false);
                return 0;
            }

            if (Pointer == ConvertedSoundData.Length)
                return 0;

            int requestSize = framesAmount * ConvBytesPerFrame;
            int actualAmount = requestSize;
            if (Pointer + actualAmount >= ConvertedSoundData.Length)
                actualAmount = ConvertedSoundData.Length - Pointer;
            new Span<byte>(ConvertedSoundData, Pointer, actualAmount).CopyTo(copyTo);
            Pointer += actualAmount;
            Engine.Log.Trace($"Requested {framesAmount}, and sent {actualAmount / ConvBytesPerFrame}, ptr is now {Pointer}/{ConvertedSoundData.Length}", "OTTT");
            return actualAmount / ConvBytesPerFrame;
        }
    }
}