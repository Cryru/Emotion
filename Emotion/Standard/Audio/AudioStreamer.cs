#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.Audio
{
    /// <summary>
    /// An object which converts to the specified format at runtime.
    /// Optimized for feeding the sound device buffer audio data.
    /// </summary>
    public class AudioStreamer
    {
        public int SourceSamples { get; protected set; }
        public Memory<byte> SoundData { get; protected set; }
        public AudioFormat SourceFormat { get; protected set; }

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
                return (float) _srcResume / _sourceConvLength * channels;
            }
        }

        protected double _srcResume;
        protected int _dstResume;

        protected int _sourceConvLength;
        protected int _dstLength;
        protected int _convQuality2;
        protected double _resampleStep;

        public AudioStreamer(AudioFormat srcFormat, Memory<byte> audioData)
        {
            SourceFormat = srcFormat;
            SourceSamples = audioData.Length / srcFormat.SampleSize;
            SoundData = audioData;
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

            _sourceConvLength = SourceSamples;
            switch (ConvFormat.Channels)
            {
                case 2 when SourceFormat.Channels == 1:
                    _sourceConvLength *= 2;
                    break;
                case 1 when SourceFormat.Channels == 2:
                    _sourceConvLength /= 2;
                    break;
            }

            _dstLength = (int) (_sourceConvLength * ResampleRatio);
            _resampleStep = (double) (_sourceConvLength / ConvFormat.Channels) / (_dstLength / ConvFormat.Channels);

            if (keepProgress)
                _dstResume = (int) (_srcResume / _resampleStep * ConvFormat.Channels);
            else
                Reset();
        }

        /// <summary>
        /// Get the next number of specified frames.
        /// </summary>
        /// <param name="frameCount">The frames to get.</param>
        /// <param name="buffer">The buffer to fill with the samples.</param>
        /// <returns>How many frames were gotten.</returns>
        public virtual int GetNextFrames(int frameCount, Span<byte> buffer)
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
        /// <param name="x">The source sample to resume from/</param>
        /// <param name="i">The destination sample to resume from.</param>
        /// <param name="getSamples">The number of resampled samples to return.</param>
        /// <param name="samples">The buffer to fill with data.</param>
        /// <returns>How many samples were returned. Can not be more than the ones requested.</returns>
        protected int PartialResample(ref double x, ref int i, int getSamples, Span<byte> samples)
        {
            int channels = ConvFormat.Channels;

            // Nyquist half of destination sampleRate
            const double fMaxDivSr = 0.5f;
            const double rG = 2 * fMaxDivSr;

            int iStart = i;

            // Snap if more samples requested than left.
            if (i + getSamples >= _dstLength) getSamples = _dstLength - i;

            // Verify that the number of samples will fit.
            int outputBps = ConvFormat.BitsPerSample / 8;
            int outputLength = samples.Length / outputBps;
            if (outputLength < getSamples)
            {
                Engine.Log.Warning($"The provided buffer to the audio streamer is of invalid size {samples.Length} while {getSamples} were requested.", MessageSource.Audio);
                getSamples = outputLength;
            }

            // Resample the needed amount.
            for (; i < _dstLength; i += channels)
            {
                for (var c = 0; c < channels; c++)
                {
                    var rY = 0.0;
                    int tau;
                    for (tau = -ConvQuality; tau < ConvQuality; tau++)
                    {
                        // input sample index.
                        var j = (int) (x + tau);

                        // Hann Window. Scale and calculate sinc
                        double rW = 0.5 - 0.5 * Math.Cos(2 * Math.PI * (0.5 + (j - x) / _convQuality2));
                        double rA = 2 * Math.PI * (j - x) * fMaxDivSr;
                        var rSnc = 1.0;
                        if (rA != 0) rSnc = Math.Sin(rA) / rA;
                        if (j < 0 || j >= _sourceConvLength / channels) continue;
                        rY += rG * rW * rSnc * GetSampleAsFloat(j * channels + c);
                    }

                    SetSampleAsFloat(i - iStart + c, MathF.Min(MathF.Max(-1, (float) rY), 1), samples);
                }

                x += _resampleStep;

                // Check if gotten enough samples for the partial resampling.
                if (i + channels - iStart < getSamples) continue;
                i += channels;
                return getSamples;
            }

            return _dstLength - iStart;
        }

        /// <summary>
        /// Returns the specified sample from the source as a float, converted into the output channel format.
        /// </summary>
        /// <param name="sampleIdx">The sample index (in the converted format) to return.</param>
        /// <param name="trueIndex">Whether the index is within the source buffer before channel conversion instead.</param>
        /// <param name="secondChannel">Whether sampling for a second channel. Used internally.</param>
        /// <returns>The specified sample as a float.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual float GetSampleAsFloat(int sampleIdx, bool trueIndex = false, bool secondChannel = false)
        {
            // Check if simulating stereo from mono.
            if (!trueIndex && SourceFormat.Channels == 1 && ConvFormat.Channels == 2) sampleIdx /= 2;

            // Check if simulating mono from stereo.
            if (!trueIndex && SourceFormat.Channels == 2 && ConvFormat.Channels == 1) sampleIdx *= 2;

            float output;

            Span<byte> data = SoundData.Span;
            switch (SourceFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    output = (float) data[sampleIdx] / byte.MaxValue;
                    break;
                case 16: // short
                    short dataShort = BitConverter.ToInt16(data.Slice(sampleIdx * 2, 2));
                    if (dataShort < 0)
                        output = (float) -dataShort / short.MinValue;
                    else
                        output = (float) dataShort / short.MaxValue;
                    break;
                case 32 when !SourceFormat.IsFloat: // int
                    int dataInt = BitConverter.ToInt32(data.Slice(sampleIdx * 4, 4));
                    if (dataInt < 0)
                        output = (float) -dataInt / int.MinValue;
                    else
                        output = (float) dataInt / int.MaxValue;
                    break;
                case 32: // float
                    output = BitConverter.ToSingle(data.Slice(sampleIdx * 4, 4));
                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat  - {SourceFormat.BitsPerSample}", MessageSource.Audio);
                    return 0;
            }

            // If simulating mono from stereo get the other channel and average them.
            // Without the "secondChannel" boolean you cannot get the value of the other channel
            // as it will loop in trying to convert it to mono as well.
            if (secondChannel || SourceFormat.Channels != 2 || ConvFormat.Channels != 1) return output;
            float outputRightChannel = GetSampleAsFloat(sampleIdx + 1, true, true);
            output = (output + outputRightChannel) / 2f;

            return output;
        }

        /// <summary>
        /// Sets the specified sample in the specified buffer from a float to the destination format.
        /// </summary>
        /// <param name="index">The index within the buffer - as if it was all floats.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="buffer">The buffer to set in.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SetSampleAsFloat(int index, float value, Span<byte> buffer)
        {
            switch (ConvFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    index /= 4;
                    buffer[index] = (byte) (value * byte.MaxValue);
                    break;
                case 16: // short
                    index /= 2;
                    Span<short> dataShort = MemoryMarshal.Cast<byte, short>(buffer);
                    if (value < 0)
                        dataShort[index] = (short) (-value * short.MinValue);
                    else
                        dataShort[index] = (short) (value * short.MaxValue);
                    break;
                case 32 when !ConvFormat.IsFloat: // int
                    Span<int> dataInt = MemoryMarshal.Cast<byte, int>(buffer);
                    if (value < 0)
                        dataInt[index] = (int) (-value * int.MinValue);
                    else
                        dataInt[index] = (int) (value * int.MaxValue);

                    break;
                case 32:
                    Span<float> dataFloat = MemoryMarshal.Cast<byte, float>(buffer);
                    dataFloat[index] = value;
                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat - {ConvFormat.BitsPerSample}", MessageSource.Audio);
                    break;
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
    }
}