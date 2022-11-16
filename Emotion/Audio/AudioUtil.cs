#region Using

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Audio
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe short AlignedToInt16(ReadOnlySpan<byte> data, int idx)
        {
            fixed (byte* pByte = &data[idx])
            {
                return *(short*) pByte;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int AlignedToInt32(ReadOnlySpan<byte> data, int idx)
        {
            fixed (byte* pByte = &data[idx])
            {
                return *(int*) pByte;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe float AlignedToFloat(ReadOnlySpan<byte> data, int idx)
        {
            fixed (byte* pByte = &data[idx])
            {
                return *(float*) pByte;
            }
        }

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
                    short dataShort = AlignedToInt16(srcData, sampleIdx * 2);
                    if (dataShort < 0)
                        output = (float) -dataShort / short.MinValue;
                    else
                        output = (float) dataShort / short.MaxValue;
                    break;
                case 32 when !srcFormat.IsFloat: // int
                    int dataInt = AlignedToInt32(srcData, sampleIdx * 4);
                    if (dataInt < 0)
                        output = (float) -dataInt / int.MinValue;
                    else
                        output = (float) dataInt / int.MaxValue;
                    break;
                case 32: // float
                    output = AlignedToFloat(srcData, sampleIdx * 4);
                    break;
                default:
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
                    break;
            }
        }

        /// <summary>
        /// Convert a buffer of floats to another format specified by an audio format.
        /// </summary>
        public static void SetBufferOfSamplesAsFloat(Span<float> floatSrc, Span<byte> dstData, AudioFormat dstFormat)
        {
            switch (dstFormat.BitsPerSample)
            {
                case 8:
                    const short byteMax = byte.MaxValue;
                    for (var i = 0; i < floatSrc.Length; i++)
                    {
                        float value = floatSrc[i];
                        dstData[i] = (byte) (value * byteMax);
                    }

                    return;
                case 16:
                    Span<short> dataShort = MemoryMarshal.Cast<byte, short>(dstData);
                    const short shortMin = short.MinValue;
                    const short shortMax = short.MaxValue;
                    for (var i = 0; i < floatSrc.Length; i++)
                    {
                        float value = floatSrc[i];
                        dataShort[i] = (short) (value < 0 ? -value * shortMin : value * shortMax);
                    }

                    break;
                case 32 when !dstFormat.IsFloat:
                    Span<int> dataInt = MemoryMarshal.Cast<byte, int>(dstData);
                    const int intMin = int.MinValue;
                    const int intMax = int.MaxValue;
                    for (var i = 0; i < floatSrc.Length; i++)
                    {
                        float value = floatSrc[i];
                        dataInt[i] = (int) (value < 0 ? -value * intMin : value * intMax);
                    }

                    break;
                case 32:
                    Span<float> dataFloat = MemoryMarshal.Cast<byte, float>(dstData);
                    floatSrc.CopyTo(dataFloat);
                    break;
            }
        }

        /// <summary>
        /// Converts a volume 0-1 to a float to multiply 32f audio samples by to achieve gain.
        /// </summary>
        public static float VolumeToMultiplier(float volume)
        {
            //volume = 20 * MathF.Log10(volume / 1f);
            //volume = MathF.Pow(10, volume / 20f);
            volume = MathF.Pow(volume, 2.718f);
            //volume = volume * volume;
            return volume;
        }
    }
}