#region Using

using System;
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
        public void SetConvertFormat(AudioFormat dstFormat, int quality = 10)
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
        public int GetNextFrames(int frameCount, out byte[] data)
        {
            // Gets the resampled samples.
            int sampleCount = frameCount * SourceFormat.Channels;
            int convertedSamples = PartialResample(ref _srcResume, ref _dstResume, sampleCount, out Span<float> convData);
            if (convertedSamples == 0)
            {
                data = new byte[0];
                return 0;
            }

            // Convert channels.
            switch (SourceFormat.Channels)
            {
                case 1 when ConvFormat.Channels == 2:
                    AudioUtil.MonoToStereo(ref convData);
                    break;
                case 2 when ConvFormat.Channels == 1:
                    AudioUtil.StereoToMono(ref convData);
                    break;
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
        private int PartialResample(ref double x, ref int i, int getSamples, out Span<float> samples)
        {
            var dstLength = (int) (SourceSamples * ResampleRatio);
            if (i + getSamples >= dstLength) getSamples = dstLength - i;

            samples = new Span<float>(new float[getSamples]);
            double dx = (double) SourceSamples / dstLength;

            // nyqist half of destination sampleRate
            const double fMaxDivSr = 0.5f;
            const double rG = 2 * fMaxDivSr;

            int wndWidth2 = ConvQuality;
            int wndWidth = ConvQuality * 2;

            int iStart = i;
            for (; i < dstLength; i++)
            {
                var rY = 0.0;
                int tau;
                for (tau = -wndWidth2; tau < wndWidth2; tau++)
                {
                    // input sample index.
                    var j = (int) (x + tau);

                    // Hann Window. Scale and calculate sinc
                    double rW = 0.5 - 0.5 * Math.Cos(2 * Math.PI * (0.5 + (j - x) / wndWidth));
                    double rA = 2 * Math.PI * (j - x) * fMaxDivSr;
                    var rSnc = 1.0;
                    if (rA != 0) rSnc = Math.Sin(rA) / rA;
                    if (j < 0 || j >= SourceSamples) continue;
                    rY += rG * rW * rSnc * GetSourceSampleAsFloat(j);
                }

                samples[i - iStart] = (float) rY;
                x += dx;

                // Check if gotten enough samples for the partial resampling.
                if (i + 1 - iStart < getSamples) continue;
                i++;
                return getSamples;
            }

            return dstLength - iStart;
        }

        /// <summary>
        /// Returns the specified sample from the source as a float.
        /// </summary>
        /// <param name="sampleIdx">The sample index to return.</param>
        /// <returns>The specified sample as a float.</returns>
        private unsafe float GetSourceSampleAsFloat(int sampleIdx)
        {
            switch (SourceFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    return (float) SoundData[sampleIdx] / byte.MaxValue;
                case 16: // short
                    fixed (void* dataPtr = &SoundData[sampleIdx * 2])
                    {
                        var dataShort = new Span<short>(dataPtr, 1);
                        if (dataShort[0] < 0)
                            return (float) -dataShort[0] / short.MinValue;
                        return (float) dataShort[0] / short.MaxValue;
                    }
                case 32 when !SourceFormat.IsFloat: // int
                    fixed (void* dataPtr = &SoundData[sampleIdx * 4])
                    {
                        var dataInt = new Span<int>(dataPtr, 1);
                        if (dataInt[0] < 0)
                            return (float) -dataInt[0] / int.MinValue;
                        return (float) dataInt[0] / int.MaxValue;
                    }
                case 32: // float
                    fixed (void* dataPtr = &SoundData[sampleIdx * 4])
                    {
                        var dataFloat = new Span<float>(dataPtr, 1);
                        return dataFloat[0];
                    }
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat  - {SourceFormat.BitsPerSample}", MessageSource.Audio);
                    return 0;
            }
        }

        public void Reset()
        {
            _dstResume = 0;
            _srcResume = 0;
        }
    }
}