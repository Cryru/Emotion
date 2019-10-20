#region Using

using System;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Platform.Implementation
{
    public abstract class AudioContext
    {
        public abstract void PlayAudioTest(WaveSoundAsset wav);

        #region Convert

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
        /// Converts a specified sound format to another sound format.
        /// </summary>
        /// <param name="srcBps">The bits per sample of the source data.</param>
        /// <param name="srcFloat">Whether the source data is of the float format.</param>
        /// <param name="srcSampleRate">The sample rate of the source data.</param>
        /// <param name="srcChannels">The channels of the source data.</param>
        /// <param name="dstBps">The bits per sample to convert to.</param>
        /// <param name="dstFloat">Whether to convert to float data.</param>
        /// <param name="dstSampleRate">The sample rate to convert to.</param>
        /// <param name="dstChannels">The channels to convert to.</param>
        /// <param name="data">The data to convert, as bytes.</param>
        public static unsafe void ConvertFormat(int srcBps, bool srcFloat, int srcSampleRate, int srcChannels, int dstBps, bool dstFloat, int dstSampleRate, int dstChannels, ref byte[] data)
        {
            // Convert to float. All internal conversions are done in the float format as it is the most flexible and most common.
            Span<float> temp;

            // Normalize all values in a float copy of the data.
            switch (srcBps)
            {
                case 8: // ubyte (C# byte)
                    temp = new Span<float>(new float[data.Length]);
                    for (var i = 0; i < data.Length; i++)
                    {
                        temp[i] = (float)data[i] / byte.MaxValue;
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
                            {
                                temp[i] = (float) -dataShort[i] / short.MinValue;
                            } 
                            else
                            {
                                temp[i] = (float) dataShort[i] / short.MaxValue;
                            }
                        }
                    }

                    break;
                case 32 when !srcFloat: // int
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataInt = new Span<int>(dataPtr, data.Length / 4);
                        temp = new Span<float>(new float[dataInt.Length]);
                        for (var i = 0; i < dataInt.Length; i++)
                        {
                            if (dataInt[i] < 0)
                            {
                                temp[i] = (float) -dataInt[i] / int.MinValue;
                            } 
                            else
                            {
                                temp[i] = (float) dataInt[i] / int.MaxValue;
                            }
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
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat  - {srcBps}", MessageSource.Audio);
                    return;
            }

            // Check if that was all.
            if (srcChannels == dstChannels && srcSampleRate == dstSampleRate && dstBps == 32 && dstFloat)
            {
                Array.Resize(ref data, temp.Length * 4);
                fixed (void* dataFPtr = &temp[0])
                {
                    new Span<byte>(dataFPtr, data.Length).CopyTo(data);
                }

                return;
            }

            // Convert channels.
            if (srcChannels == 1 && dstChannels == 2) MonoToStereo(ref temp);
            if (dstChannels == 1 && srcChannels == 2) StereoToMono(ref temp);

            // Resample.
            if (srcSampleRate != dstSampleRate) Resample(ref temp, srcSampleRate, dstSampleRate);

            // Check if a data conversion is not needed.
            if (dstBps == 32 && dstFloat)
            {
                Array.Resize(ref data, temp.Length * 4);
                fixed (void* dataFPtr = &temp[0])
                {
                    new Span<byte>(dataFPtr, data.Length).CopyTo(data);
                }

                return;
            }

            // Convert data.
            switch (dstBps)
            {
                case 8: // ubyte (C# byte)
                    Array.Resize(ref data, temp.Length);
                    for (var i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(temp[i] * byte.MaxValue);
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
                            {
                                dataShort[i] = (short) (-temp[i] * short.MinValue);
                            }
                            else
                            {
                                dataShort[i] = (short) (temp[i] * short.MaxValue);
                            }
                        }
                    }

                    break;
                case 32 when !dstFloat: // int
                    Array.Resize(ref data, temp.Length * 4);
                    fixed (void* dataPtr = &data[0])
                    {
                        var dataInt = new Span<int>(dataPtr, data.Length / 4);
                        for (var i = 0; i < dataInt.Length; i++)
                        {
                            if (temp[i] < 0)
                            {
                                dataInt[i] = (int) (-temp[i] * int.MinValue);
                            }
                            else
                            {
                                dataInt[i] = (int) (temp[i] * int.MaxValue);
                            }
                        }
                    }

                    break;
                default:
                    Engine.Log.Warning($"Unsupported source bits per sample format by ConvertFormat - {dstBps}", MessageSource.Audio);
                    return;
            }
        }

        /// <summary>
        /// Resamples the provided 32bit float sound data. using sinc based resampling.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <param name="srcSamples">The sample rate of the source data.</param>
        /// <param name="dstSamples">The sample rate of the destination data.</param>
        /// <param name="quality">The scrolling window size. 10 by default.</param>
        public static void Resample(ref Span<float> data, int srcSamples, int dstSamples, int quality = 10)
        {
            var dstLength = (int) (data.Length * ((float) dstSamples / srcSamples));
            var samples = new Span<float>(new float[dstLength]);
            double dx = (double) data.Length / dstLength;

            // nyqist half of destination sampleRate
            const double fMaxDivSr = 0.5f;
            const double rG = 2 * fMaxDivSr;

            int wndWidth2 = quality;
            int wndWidth = quality * 2;

            double x = 0;
            for (var i = 0; i < dstLength; i++)
            {
                var rY = 0.0;
                int tau;
                for (tau=-wndWidth2;tau < wndWidth2;tau++)
                {
                    // input sample index.
                    var j = (int) (x + tau);

                    // Hann Window. Scale and calculate sinc
                    double rW = 0.5 - 0.5 * Math.Cos(2*Math.PI*(0.5 + (j-x)/wndWidth));
                    double rA = 2*Math.PI*(j-x)*fMaxDivSr;
                    var rSnc = 1.0;
                    if (rA != 0) rSnc = Math.Sin(rA) / rA;
                    if ((j < 0) || (j >= data.Length)) continue;
                    rY += rG * rW * rSnc * data[j];
                }

                samples[i] = (float) rY;
                x += dx;
            }

            data = samples;
        }

        #endregion
    }
}