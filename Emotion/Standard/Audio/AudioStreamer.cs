#region Using

using System;
using System.Runtime.CompilerServices;
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
        public byte[] SoundData { get; protected set; }
        public AudioFormat SourceFormat { get; protected set; }

        public float ResampleRatio { get; protected set; }
        public int ConvQuality { get; protected set; } = 10;
        public AudioFormat ConvFormat { get; protected set; }

        private double _srcResume;
        private int _dstResume;

        public AudioStreamer(AudioFormat srcFormat, byte[] audioData)
        {
            SourceFormat = srcFormat;
            SourceSamples = audioData.Length / srcFormat.FrameSize;
            SoundData = audioData;
        }

        /// <summary>
        /// Sets the format the stream should convert to.
        /// </summary>
        /// <param name="dstFormat">The format to convert to.</param>
        /// <param name="quality">The conversion quality.</param>
        public virtual void SetConvertFormat(AudioFormat dstFormat, int quality = 10)
        {
            ConvFormat = dstFormat;
            ConvQuality = quality;
            ResampleRatio = (float) dstFormat.SampleRate / SourceFormat.SampleRate;
        }

        /// <summary>
        /// Get the next number of specified frames.
        /// </summary>
        /// <param name="frameCount">The frames to get.</param>
        /// <param name="data">The data with the samples.</param>
        /// <returns>How many frames were gotten.</returns>
        public virtual int GetNextFrames(int frameCount, out byte[] data)
        {
            // Gets the resampled samples.
            int sampleCount = frameCount * ConvFormat.Channels;
            int convertedSamples = PartialResample(ref _srcResume, ref _dstResume, sampleCount, out Span<float> convData);
            if (convertedSamples == 0)
            {
                data = new byte[0];
                return 0;
            }

            // Convert format.
            Span<byte> destFormatData = AudioUtil.ConvertFloatToBps(ConvFormat, convData);
            data = destFormatData.ToArray();

            return convertedSamples / ConvFormat.Channels;
        }

        /// <summary>
        /// Returns the specified amount of resampled samples.
        /// Note that resampling may reduce or increase the number of samples.
        /// </summary>
        /// <param name="x">The source sample to resume from/</param>
        /// <param name="i">The destination sample to resume from.</param>
        /// <param name="getSamples">The number of resampled samples to return.</param>
        /// <param name="samples">The resampled samples data.</param>
        /// <returns>How many samples were returned. Can not be more than the ones requested.</returns>
        protected int PartialResample(ref double x, ref int i, int getSamples, out Span<float> samples)
        {
            int channels = ConvFormat.Channels;
            int sourceConvLength = SourceSamples;
            switch (ConvFormat.Channels)
            {
                case 2 when SourceFormat.Channels == 1:
                    sourceConvLength *= 2;
                    break;
                case 1 when SourceFormat.Channels == 2:
                    sourceConvLength /= 2;
                    break;
            }

            var dstLength = (int) (sourceConvLength * ResampleRatio);
            if (i + getSamples >= dstLength) getSamples = dstLength - i;

            samples = new Span<float>(new float[getSamples]);
            double dx = (double) sourceConvLength / (dstLength / channels);

            // Nyquist half of destination sampleRate
            const double fMaxDivSr = 0.5f;
            const double rG = 2 * fMaxDivSr;

            int wndWidth2 = ConvQuality;
            int wndWidth = ConvQuality * 2;

            int iStart = i;
            for (; i < dstLength; i += channels)
            {
                for (var c = 0; c < channels; c++)
                {
                    var rY = 0.0;
                    int tau;
                    for (tau = -wndWidth2; tau < wndWidth2; tau++)
                    {
                        int channelTau = tau * channels + c;

                        // input sample index.
                        var j = (int) (x + channelTau);

                        // Hann Window. Scale and calculate sinc
                        double rW = 0.5 - 0.5 * Math.Cos(2 * Math.PI * (0.5 + (j - x) / wndWidth));
                        double rA = 2 * Math.PI * (j - x) * fMaxDivSr;
                        var rSnc = 1.0;
                        if (rA != 0) rSnc = Math.Sin(rA) / rA;
                        if (j < 0 || j >= sourceConvLength) continue;
                        rY += rG * rW * rSnc * GetSampleAsFloat(j);
                    }

                    samples[i - iStart + c] = MathF.Min(MathF.Max(-1, (float) rY), 1);
                }

                x += dx;

                // Check if gotten enough samples for the partial resampling.
                if (i + channels - iStart < getSamples) continue;
                i += channels;
                return getSamples;
            }

            return dstLength - iStart;
        }

        /// <summary>
        /// Returns the specified sample from the source as a float, converted into the output channel format.
        /// </summary>
        /// <param name="sampleIdx">The sample index (in the converted format) to return.</param>
        /// <param name="trueIndex">Whether the index is within the source buffer before channel conversion instead.</param>
        /// <returns>The specified sample as a float.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe float GetSampleAsFloat(int sampleIdx, bool trueIndex = false)
        {
            // Check if simulating stereo from mono.
            if (!trueIndex && SourceFormat.Channels == 1 && ConvFormat.Channels == 2) sampleIdx /= 2;

            // Check if simulating mono from stereo.
            if (!trueIndex && SourceFormat.Channels == 2 && ConvFormat.Channels == 1) sampleIdx *= 2;

            float output;

            switch (SourceFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    output = (float) SoundData[sampleIdx] / byte.MaxValue;
                    break;
                case 16: // short
                    fixed (void* dataPtr = &SoundData[sampleIdx * 2])
                    {
                        var dataShort = new Span<short>(dataPtr, 1);
                        if (dataShort[0] < 0)
                            output = (float) -dataShort[0] / short.MinValue;
                        else
                            output = (float) dataShort[0] / short.MaxValue;
                    }

                    break;
                case 32 when !SourceFormat.IsFloat: // int
                    fixed (void* dataPtr = &SoundData[sampleIdx * 4])
                    {
                        var dataInt = new Span<int>(dataPtr, 1);
                        if (dataInt[0] < 0)
                            output = (float) -dataInt[0] / int.MinValue;
                        else
                            output = (float) dataInt[0] / int.MaxValue;
                    }

                    break;
                case 32: // float
                    fixed (void* dataPtr = &SoundData[sampleIdx * 4])
                    {
                        var dataFloat = new Span<float>(dataPtr, 1);
                        output = dataFloat[0];
                    }

                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat  - {SourceFormat.BitsPerSample}", MessageSource.Audio);
                    return 0;
            }

            // If simulating mono from stereo get the other channel and average them.
            if (SourceFormat.Channels != 2 || ConvFormat.Channels != 1) return output;
            float outputRightChannel = GetSampleAsFloat(sampleIdx + 1, true);
            output = (output + outputRightChannel) / 2f;

            return output;
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