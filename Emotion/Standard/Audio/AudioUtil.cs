#region Using

using System;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Standard.Audio
{
    public static class AudioUtil
    {
        /// <summary>
        /// Converts 32bit float sound data from stereo to mono.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        public static void StereoToMono(ref Span<float> data)
        {
            if (data.Length % 2 != 0)
            {
                Engine.Log.Warning("StereoToMono converter input doesn't seem to be stereo.", MessageSource.Audio);
                return;
            }

            var output = new Span<float>(new float[data.Length / 2]);
            for (int i = 0, w = 0; i < data.Length; i += 2, w++)
            {
                float avg = (data[i] + data[i + 1]) / 2f;
                output[w] = avg;
            }

            data = output;
        }

        /// <summary>
        /// Converts 32bit float sound data from mono to stereo.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        public static void MonoToStereo(ref Span<float> data)
        {
            var output = new Span<float>(new float[data.Length * 2]);
            for (int i = 0, w = 0; i < data.Length; i++, w += 2)
            {
                output[w] = data[i];
                output[w + 1] = data[i];
            }

            data = output;
        }

        /// <summary>
        /// Converts the specified audio data to the float format.
        /// </summary>
        /// <param name="srcFormat">The format of the audio data.</param>
        /// <param name="data">The audio data to convert.</param>
        /// <returns>The audio data as a float.</returns>
        public static unsafe Span<float> ConvertToFloat(AudioFormat srcFormat, Span<byte> data)
        {
            Span<float> temp;

            // Normalize all values in a float copy of the data.
            switch (srcFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    temp = new Span<float>(new float[data.Length]);
                    for (var i = 0; i < data.Length; i++)
                    {
                        temp[i] = (float) data[i] / byte.MaxValue;
                    }

                    break;
                case 16: // short
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataShort = new Span<short>(dataPtr, data.Length / 2);
                        temp = new Span<float>(new float[dataShort.Length]);
                        for (var i = 0; i < dataShort.Length; i++)
                        {
                            if (dataShort[i] < 0)
                                temp[i] = (float) -dataShort[i] / short.MinValue;
                            else
                                temp[i] = (float) dataShort[i] / short.MaxValue;
                        }
                    }

                    break;
                case 32 when !srcFormat.IsFloat: // int
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataInt = new Span<int>(dataPtr, data.Length / 4);
                        temp = new Span<float>(new float[dataInt.Length]);
                        for (var i = 0; i < dataInt.Length; i++)
                        {
                            if (dataInt[i] < 0)
                                temp[i] = (float) -dataInt[i] / int.MinValue;
                            else
                                temp[i] = (float) dataInt[i] / int.MaxValue;
                        }
                    }

                    break;
                case 32: // float
                    fixed (void* dataPtr = &data[0])
                    {
                        temp = new Span<float>(new float[data.Length / 4]);
                        new Span<float>(dataPtr, data.Length / 4).CopyTo(temp);
                    }

                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat  - {srcFormat.BitsPerSample}", MessageSource.Audio);
                    return null;
            }

            return temp;
        }

        /// <summary>
        /// Converts a specified sound format to another sound format.
        /// </summary>
        /// <param name="srcFormat">The format of the input data.</param>
        /// <param name="dstFormat">The format to convert to.</param>
        /// <param name="data">The data to convert, as bytes.</param>
        /// <param name="resampleQuality">The quality of resampling.</param>
        public static unsafe void ConvertFormat(AudioFormat srcFormat, AudioFormat dstFormat, ref byte[] data, int resampleQuality = 10)
        {
            // Convert to float. All internal conversions are done in the float format as it is the most flexible and most common.
            Span<float> temp;

            // Normalize all values in a float copy of the data.
            switch (srcFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    temp = new Span<float>(new float[data.Length]);
                    for (var i = 0; i < data.Length; i++)
                    {
                        temp[i] = (float) data[i] / byte.MaxValue;
                    }

                    break;
                case 16: // short
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataShort = new Span<short>(dataPtr, data.Length / 2);
                        temp = new Span<float>(new float[dataShort.Length]);
                        for (var i = 0; i < dataShort.Length; i++)
                        {
                            if (dataShort[i] < 0)
                                temp[i] = (float) -dataShort[i] / short.MinValue;
                            else
                                temp[i] = (float) dataShort[i] / short.MaxValue;
                        }
                    }

                    break;
                case 32 when !srcFormat.IsFloat: // int
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataInt = new Span<int>(dataPtr, data.Length / 4);
                        temp = new Span<float>(new float[dataInt.Length]);
                        for (var i = 0; i < dataInt.Length; i++)
                        {
                            if (dataInt[i] < 0)
                                temp[i] = (float) -dataInt[i] / int.MinValue;
                            else
                                temp[i] = (float) dataInt[i] / int.MaxValue;
                        }
                    }

                    break;
                case 32: // float
                    fixed (void* dataPtr = &data[0])
                    {
                        temp = new Span<float>(new float[data.Length / 4]);
                        new Span<float>(dataPtr, data.Length / 4).CopyTo(temp);
                    }

                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat  - {srcFormat.BitsPerSample}", MessageSource.Audio);
                    return;
            }

            // Check if that was all.
            if (srcFormat.Channels == dstFormat.Channels && srcFormat.SampleRate == dstFormat.SampleRate && dstFormat.BitsPerSample == 32 && dstFormat.IsFloat)
            {
                Array.Resize(ref data, temp.Length * 4);
                fixed (void* dataFPtr = &temp[0])
                {
                    new Span<byte>(dataFPtr, data.Length).CopyTo(data);
                }

                return;
            }

            // Convert channels.
            if (srcFormat.Channels == 1 && dstFormat.Channels == 2) MonoToStereo(ref temp);
            if (dstFormat.Channels == 1 && srcFormat.Channels == 2) StereoToMono(ref temp);
            srcFormat = srcFormat.Copy(); // Copy as not to mutate output.
            srcFormat.Channels = dstFormat.Channels;

            // Resample.
            if (srcFormat.SampleRate != dstFormat.SampleRate) Resample(ref temp, srcFormat, dstFormat, resampleQuality);

            // Check if a data conversion is not needed.
            if (dstFormat.BitsPerSample == 32 && dstFormat.IsFloat)
            {
                Array.Resize(ref data, temp.Length * 4);
                fixed (void* dataFPtr = &temp[0])
                {
                    new Span<byte>(dataFPtr, data.Length).CopyTo(data);
                }

                return;
            }

            // Convert data.
            switch (dstFormat.BitsPerSample)
            {
                case 8: // ubyte (C# byte)
                    Array.Resize(ref data, temp.Length);
                    for (var i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte) (temp[i] * byte.MaxValue);
                    }

                    break;
                case 16: // short
                    Array.Resize(ref data, temp.Length * 2);
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataShort = new Span<short>(dataPtr, data.Length / 2);
                        for (var i = 0; i < dataShort.Length; i++)
                        {
                            if (temp[i] < 0)
                                dataShort[i] = (short) (-temp[i] * short.MinValue);
                            else
                                dataShort[i] = (short) (temp[i] * short.MaxValue);
                        }
                    }

                    break;
                case 32 when !dstFormat.IsFloat: // int
                    Array.Resize(ref data, temp.Length * 4);
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataInt = new Span<int>(dataPtr, data.Length / 4);
                        for (var i = 0; i < dataInt.Length; i++)
                        {
                            if (temp[i] < 0)
                                dataInt[i] = (int) (-temp[i] * int.MinValue);
                            else
                                dataInt[i] = (int) (temp[i] * int.MaxValue);
                        }
                    }

                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat - {dstFormat.BitsPerSample}", MessageSource.Audio);
                    return;
            }
        }

        /// <summary>
        /// Resamples the provided 32bit float sound data using sinc based resampling.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <param name="srcFormat">The format to sample from.</param>
        /// <param name="dstFormat">The format to resample to.</param>
        /// <param name="quality">The quality to convert at - the scrolling window size. 10 by default.</param>
        public static void Resample(ref Span<float> data, AudioFormat srcFormat, AudioFormat dstFormat, int quality = 10)
        {
            if (srcFormat.Channels != dstFormat.Channels)
            {
                Engine.Log.Warning("Cannot resample between formats with different channel count!", MessageSource.Audio);
                return;
            }

            int channels = srcFormat.Channels;

            float resampleRatio = (float) dstFormat.SampleRate / srcFormat.SampleRate;
            var dstLength = (int) (data.Length * resampleRatio);
            var samples = new Span<float>(new float[dstLength]);
            double dx = (double) (data.Length / channels) / (dstLength / channels);

            // Nyquist half of destination sampleRate
            const double fMaxDivSr = 0.5f;
            const double rG = 2 * fMaxDivSr;

            int wndWidth2 = quality;
            int wndWidth = quality * 2;

            double x = 0;
            for (var i = 0; i < dstLength; i += channels)
            {
                for (var c = 0; c < channels; c++)
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
                        if (j < 0 || j >= data.Length / channels) continue;
                        rY += rG * rW * rSnc * data[j * channels + c];
                    }

                    samples[i + c] = MathF.Min(MathF.Max(-1, (float) rY), 1);
                }

                x += dx;
            }

            data = samples;
        }
    }
}