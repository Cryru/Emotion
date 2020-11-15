#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Audio
{
    /// <summary>
    /// Converts audio of one sample rate to another.
    /// Optimized for feeding the sound device buffer audio data.
    /// Frame - Samples * Channels
    /// Sample - Sound, one for each channel
    /// </summary>
    public class AudioStreamer
    {
        public int SourceSamples { get; protected set; }
        public int SourceConvFormatSamples { get; protected set; }
        public int SourceSamplesPerChannel { get; protected set; }
        public Memory<float> SoundData { get; protected set; }
        public AudioFormat SourceFormat { get; protected set; }

        public int ConvSamples { get; protected set; }
        public float ResampleRatio { get; protected set; }
        public int ConvQuality { get; protected set; } = 10;
        public AudioFormat ConvFormat { get; protected set; }

        /// <summary>
        /// The resampling progress.
        /// </summary>
        public float Progress
        {
            get
            {
                int channels = ConvFormat?.Channels ?? SourceFormat.Channels;
                return (float) _srcResume / SourceConvFormatSamples * channels;
            }
        }

        protected double _srcResume;
        protected int _dstResume;

        protected int _convQuality2;
        protected double _resampleStep;

        protected enum ChannelConversion : byte
        {
            SimulateMono,
            SimulateStereo,
            None
        }

        protected ChannelConversion _channelConversion;

        public AudioStreamer(AudioFormat srcFormat, Memory<float> floatAudioData)
        {
            SourceFormat = srcFormat;
            SourceSamples = floatAudioData.Length;
            SoundData = floatAudioData;

            Debug.Assert(srcFormat.BitsPerSample == 32);
            Debug.Assert(srcFormat.IsFloat);
        }

        /// <summary>
        /// Sets the format the stream should convert to.
        /// </summary>
        /// <param name="dstFormat">The format to convert to.</param>
        /// <param name="quality">The conversion quality.</param>
        /// <param name="keepProgress">Whether to keep the resampling progress. If false the stream will begin from the beginning.</param>
        public virtual void SetConvertFormat(AudioFormat dstFormat, int quality = 10, bool keepProgress = true)
        {
            ConvFormat = dstFormat;
            ConvQuality = quality;
            _convQuality2 = quality * 2;
            ResampleRatio = (float) dstFormat.SampleRate / SourceFormat.SampleRate;

            _channelConversion = ChannelConversion.None;
            SourceConvFormatSamples = SourceSamples;
            int channels = ConvFormat.Channels;
            switch (channels)
            {
                case 2 when SourceFormat.Channels == 1:
                    SourceConvFormatSamples *= 2;
                    _channelConversion = ChannelConversion.SimulateStereo;
                    break;
                case 1 when SourceFormat.Channels == 2:
                    SourceConvFormatSamples /= 2;
                    _channelConversion = ChannelConversion.SimulateMono;
                    break;
            }

            SourceSamplesPerChannel = SourceConvFormatSamples / channels;
            ConvSamples = (int) (SourceConvFormatSamples * ResampleRatio);
            _resampleStep = (double) SourceSamplesPerChannel / (ConvSamples / channels);

            if (keepProgress)
                _dstResume = (int) (_srcResume / _resampleStep * ConvFormat.Channels);
            else
                Reset();
        }

        /// <summary>
        /// Get the next N frames in the buffer - resampled.
        /// </summary>
        /// <param name="frameCount">The frames to get.</param>
        /// <param name="buffer">The buffer to fill with the samples.</param>
        /// <returns>How many frames were gotten.</returns>
        public virtual int GetNextFrames(int frameCount, Span<float> buffer)
        {
            // Gets the resampled samples.
            int sampleCount = frameCount * ConvFormat.Channels;
            Debug.Assert((int) (_srcResume / _resampleStep * ConvFormat.Channels) - _dstResume <= 1);
            int convertedSamples = PartialResample(ref _srcResume, ref _dstResume, sampleCount, buffer);
            if (convertedSamples == 0) return 0;

            return convertedSamples / ConvFormat.Channels;
        }

        /// <summary>
        /// Returns the specified amount of resampled samples.
        /// Note that resampling may reduce or increase the number of samples.
        /// </summary>
        /// <param name="srcStartIdx">The source sample to resume from.</param>
        /// <param name="dstSampleIdx">The destination sample to resume from.</param>
        /// <param name="getSamples">The number of resampled samples to return.</param>
        /// <param name="samples">The buffer to fill with data.</param>
        /// <returns>How many samples were returned. Can not be more than the ones requested.</returns>
        protected int PartialResample(ref double srcStartIdx, ref int dstSampleIdx, int getSamples, Span<float> samples)
        {
            int channels = ConvFormat.Channels;

            // Nyquist half of destination sampleRate
            const double fMaxDivSr = 0.5f;
            const double rG = 2 * fMaxDivSr;

            int iStart = dstSampleIdx;

            // Snap if more samples requested than left.
            if (dstSampleIdx + getSamples >= ConvSamples) getSamples = ConvSamples - dstSampleIdx;

            // Verify that the number of samples will fit.
            if (samples.Length < getSamples)
            {
                Engine.Log.Warning($"The provided buffer to the audio streamer is of invalid size {samples.Length} while {getSamples} were requested.", MessageSource.Audio);
                getSamples = samples.Length;
            }

            // srcStartIdx == (dstSampleIdx / channels) *_resampleStep;

            // Resample the needed amount.
            Span<float> soundData = SoundData.Span;
            for (; dstSampleIdx < ConvSamples; dstSampleIdx += channels)
            {
                // Check if gotten enough samples.
                if (dstSampleIdx + channels - iStart > getSamples) return getSamples;

                int targetBufferIdx = dstSampleIdx - iStart;
                for (var c = 0; c < channels; c++)
                {
                    var rY = 0.0;
                    int tau;
                    for (tau = -ConvQuality; tau < ConvQuality; tau++)
                    {
                        var inputSampleIdx = (int) (srcStartIdx + tau);
                        double relativeIdx = inputSampleIdx - srcStartIdx;

                        // Hann Window. Scale and calculate sinc
                        double rW = 0.5 - 0.5 * Math.Cos(Maths.TWO_PI_DOUBLE * (0.5 + relativeIdx / _convQuality2));
                        double rA = Maths.TWO_PI_DOUBLE * relativeIdx * fMaxDivSr;
                        var rSnc = 1.0;
                        if (rA != 0) rSnc = Math.Sin(rA) / rA;
                        if (inputSampleIdx < 0 || inputSampleIdx >= SourceSamplesPerChannel) continue;
                        rY += rG * rW * rSnc * GetChannelConvertedSample(inputSampleIdx * channels + c, soundData);
                    }

                    float value = MathF.Min(MathF.Max(-1, (float) rY), 1);
                    samples[targetBufferIdx + c] = value;
                }

                srcStartIdx += _resampleStep;
            }

            return ConvSamples - iStart;
        }

        /// <summary>
        /// Returns the specified sample from the source as a float, converted into the output channel format.
        /// </summary>
        /// <param name="sampleIdx">The sample index (in the converted format) to return.</param>
        /// <param name="soundData">The source sound data to get the sample from.</param>
        /// <returns>The specified sample as a float.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetChannelConvertedSample(int sampleIdx, Span<float> soundData)
        {
            switch (_channelConversion)
            {
                case ChannelConversion.SimulateMono:
                    sampleIdx *= 2;
                    // If simulating mono from stereo get the other channel and average them.
                    float left = soundData[sampleIdx];
                    float right = soundData[sampleIdx + 1];
                    return (left + right) / 2f;
                case ChannelConversion.SimulateStereo:
                    sampleIdx /= 2;
                    return soundData[sampleIdx];
                default:
                    return soundData[sampleIdx];
            }
        }

        /// <summary>
        /// Restart the streamer's pointer.
        /// </summary>
        public virtual void Reset()
        {
            _dstResume = 0;
            _srcResume = 0;
        }

        #region Static API

        /// <summary>
        /// Get a sample from a PCM as a float.
        /// </summary>
        /// <param name="sampleIdx">The index of the sample.</param>
        /// <param name="srcData">The PCM</param>
        /// <param name="srcFormat">The audio format of the PCM</param>
        /// <returns>The requested sample as a float.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSampleAsFloat(int sampleIdx, ReadOnlySpan<byte> srcData, AudioFormat srcFormat)
        {
            float output;
            switch (srcFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    output = (float) srcData[sampleIdx] / byte.MaxValue;
                    break;
                case 16: // short
                    var dataShort = BitConverter.ToInt16(srcData.Slice(sampleIdx * 2, 2));
                    if (dataShort < 0)
                        output = (float) -dataShort / short.MinValue;
                    else
                        output = (float) dataShort / short.MaxValue;
                    break;
                case 32 when !srcFormat.IsFloat: // int
                    var dataInt = BitConverter.ToInt32(srcData.Slice(sampleIdx * 4, 4));
                    if (dataInt < 0)
                        output = (float) -dataInt / int.MinValue;
                    else
                        output = (float) dataInt / int.MaxValue;
                    break;
                case 32: // float
                    output = BitConverter.ToSingle(srcData.Slice(sampleIdx * 4, 4));
                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by SourceFormat - {srcFormat.BitsPerSample}", MessageSource.Audio);
                    return 0;
            }

            return output;
        }

        /// <summary>
        /// Set a sample in the PCM from a float.
        /// </summary>
        /// <param name="sampleIdx">The index of the sample.</param>
        /// <param name="value">The value of the sample to set, as a float.</param>
        /// <param name="dstData">The PCM</param>
        /// <param name="dstFormat">The audio format of the PCM</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSampleAsFloat(int sampleIdx, float value, Span<byte> dstData, AudioFormat dstFormat)
        {
            switch (dstFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    sampleIdx /= 4;
                    dstData[sampleIdx] = (byte) (value * byte.MaxValue);
                    break;
                case 16: // short
                    sampleIdx /= 2;
                    Span<short> dataShort = MemoryMarshal.Cast<byte, short>(dstData);
                    if (value < 0)
                        dataShort[sampleIdx] = (short) (-value * short.MinValue);
                    else
                        dataShort[sampleIdx] = (short) (value * short.MaxValue);
                    break;
                case 32 when !dstFormat.IsFloat: // int
                    Span<int> dataInt = MemoryMarshal.Cast<byte, int>(dstData);
                    if (value < 0)
                        dataInt[sampleIdx] = (int) (-value * int.MinValue);
                    else
                        dataInt[sampleIdx] = (int) (value * int.MaxValue);

                    break;
                case 32:
                    Span<float> dataFloat = MemoryMarshal.Cast<byte, float>(dstData);
                    dataFloat[sampleIdx] = value;
                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by DestinationFormat - {dstFormat.BitsPerSample}", MessageSource.Audio);
                    break;
            }
        }

        #endregion
    }
}