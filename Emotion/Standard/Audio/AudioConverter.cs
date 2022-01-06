#region Using

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Audio
{
    /// <summary>
    /// Converts audio of one audio format to another.
    /// Remaps channels, resizes sample size to floats, resamples sample rate.
    /// Optimized for feeding the sound device buffer audio data.
    /// Frame - Samples * Channels
    /// Sample - Sound, one for each channel
    /// </summary>
    public class AudioConverter
    {
        public int SourceSamples { get; protected set; }
        public int SourceSamplesPerChannel { get; protected set; }
        public float[] SoundData { get; protected set; }
        public AudioFormat SourceFormat { get; protected set; }

        public int ConvQuality { get; protected set; }
        protected int _convQuality2;

        public AudioConverter(AudioFormat srcFormat, float[] soundData, int quality = 10)
        {
            SourceFormat = srcFormat;
            SourceSamples = soundData.Length;
            SourceSamplesPerChannel = SourceSamples / srcFormat.Channels;
            SoundData = soundData;

            ConvQuality = quality;
            _convQuality2 = quality * 2;
        }

        #region Channel Remapping

        private static ConcurrentDictionary<int, sbyte[]> _cachedRemappingMaps = new();

        // https://en.wikipedia.org/wiki/Surround_sound#Channel_identification
        private const sbyte CHANNEL_REMAP_MONO = -101;
        private const sbyte CHANNEL_REMAP_SURROUND = -100;
        private const sbyte REMAP_COMBINE = sbyte.MinValue; // Combine left and right

        // Source mapping for surround
        private static sbyte[] _surroundChannelsMapping =
        {
            -1, // Front Left
            -2, // Front Right
            REMAP_COMBINE, // Center
            REMAP_COMBINE, // Subwoofer
            -1, // Side Left
            -2, // Side Right
            -1, // Left Alt
            -2, // Right Alt
        };

        private float[] _surroundResampleCache = new float[2];

        public static sbyte[] GetChannelRemappingMapFor(int srcChannels, int dstChannels)
        {
            int hash = Maths.GetCantorPair(srcChannels, dstChannels);
            if (_cachedRemappingMaps.TryGetValue(hash, out sbyte[] map)) return map;

            var newMap = new sbyte[dstChannels];
            // If destination has less channels than source, then either map all into one (mono) mode or use surround mode.
            bool mixDown = dstChannels < srcChannels;
            sbyte mixDownFlag = dstChannels == 1 ? CHANNEL_REMAP_MONO : CHANNEL_REMAP_SURROUND;
            for (var i = 0; i < dstChannels; i++)
            {
                newMap[i] = mixDown ? mixDownFlag : (sbyte) (i % srcChannels);
            }

            _cachedRemappingMaps.TryAdd(hash, newMap);
            return newMap;
        }

        #endregion

        /// <summary>
        /// Returns how many samples this audio would have in this format.
        /// </summary>
        public int GetSampleCountInFormat(AudioFormat format)
        {
            int dstChannels = format.Channels;
            int srcChannels = SourceFormat.Channels;
            int srcSampleRate = SourceFormat.SampleRate;
            int sourceSamples = SourceSamples;
            int dstSampleRate = format.SampleRate;
            return (int) (sourceSamples * dstChannels / srcChannels * ((float) dstSampleRate / srcSampleRate));
        }

        /// <summary>
        /// Converts audio frames from the source format to the specified format.
        /// Samples are in the 32f format though.
        /// </summary>
        /// <param name="dstFormat">The format to convert to. Channels, sample size, and sample rate is converted.</param>
        /// <param name="dstSampleIdxStart">
        /// The index of the sample to start from, relative to the total samples this audio would
        /// have in the dstFormat.
        /// </param>
        /// <param name="frameCount">The number of frames (in the dstFormat) to convert.</param>
        /// <param name="buffer">The buffer to fill with converted samples.</param>
        /// <returns>How many frames were actually converted.</returns>
        public int GetConvertedSamplesAt(AudioFormat dstFormat, int dstSampleIdxStart, int frameCount, Span<float> buffer)
        {
            if (dstFormat.UnsupportedBitsPerSample())
                Engine.Log.Warning($"Unsupported bits per sample format by DestinationFormat - {dstFormat.BitsPerSample}", MessageSource.Audio);

            int dstChannels = dstFormat.Channels;
            int srcChannels = SourceFormat.Channels;
            sbyte[] channelRemap = GetChannelRemappingMapFor(srcChannels, dstChannels);

            // Optimization stuff
            bool useChannelCache = dstChannels > 2;
            bool getSamplesDirectly = dstChannels == srcChannels;
            int convQuality = ConvQuality;
            int sourceSamplesPerChannel = SourceSamplesPerChannel;
            float[] soundData = SoundData;

            int srcSampleRate = SourceFormat.SampleRate;
            int sourceSamples = SourceSamples;
            int dstSampleRate = dstFormat.SampleRate;
            var convSamples = (int) (sourceSamples * dstChannels / srcChannels * ((float) dstSampleRate / srcSampleRate));

            double resampleStep = (double) srcSampleRate / dstSampleRate;
            int requestedSamples = frameCount * dstChannels; // rename
            double srcStartIdx = dstSampleIdxStart / dstChannels * resampleStep;

            // Nyquist half of destination sampleRate
            const double maxDivSr = 0.5f;
            const double rG = 2 * maxDivSr;

            // Snap if more samples requested than left.
            if (dstSampleIdxStart + requestedSamples >= convSamples) requestedSamples = convSamples - dstSampleIdxStart;

            // Verify that the number of samples will fit in the buffer.
            if (buffer.Length < requestedSamples)
            {
                Engine.Log.Warning($"The provided buffer to the audio streamer is of invalid size {buffer.Length} while {requestedSamples} were requested.", MessageSource.Audio);
                requestedSamples = buffer.Length;
            }

            // Resample from dstSampleIdx to the requested samples.
            for (int dstSampleIdx = dstSampleIdxStart; dstSampleIdx < convSamples; dstSampleIdx += dstChannels)
            {
                // Check if gotten enough samples.
                if (dstSampleIdx + dstChannels - dstSampleIdxStart > requestedSamples) return requestedSamples;

                // Reset cache.
                if (useChannelCache)
                {
                    _surroundResampleCache[0] = float.NaN;
                    _surroundResampleCache[1] = float.NaN;
                }

                int targetBufferIdx = dstSampleIdx - dstSampleIdxStart;
                for (var c = 0; c < dstChannels; c++)
                {
                    // Multichannels will resample 1 and 2 multiple times.
                    if (useChannelCache)
                    {
                        sbyte convChannel = channelRemap[c];
                        if (convChannel >= 0 && convChannel <= 1 && !float.IsNaN(_surroundResampleCache[convChannel]))
                        {
                            buffer[targetBufferIdx + c] = _surroundResampleCache[convChannel];
                            continue;
                        }
                    }

                    var rY = 0.0;
                    for (int tau = -convQuality; tau < convQuality; tau++)
                    {
                        var inputSampleIdx = (int) (srcStartIdx + tau);
                        double relativeIdx = inputSampleIdx - srcStartIdx;

                        // Hann Window. Scale and calculate sinc
                        double rW = 0.5 - 0.5 * Math.Cos(Maths.TWO_PI_DOUBLE * (0.5 + relativeIdx / _convQuality2));
                        double rA = Maths.TWO_PI_DOUBLE * relativeIdx * maxDivSr;
                        var rSnc = 1.0;
                        if (rA != 0) rSnc = Math.Sin(rA) / rA;
                        if (inputSampleIdx < 0 || inputSampleIdx >= sourceSamplesPerChannel) continue;

                        float sample = getSamplesDirectly ? soundData[inputSampleIdx * srcChannels] : GetChannelConvertedSample(inputSampleIdx * srcChannels, c, channelRemap);
                        rY += rG * rW * rSnc * sample;
                    }

                    float value = Maths.Clamp((float) rY, -1f, 1f);
                    buffer[targetBufferIdx + c] = value;
                    if (useChannelCache) _surroundResampleCache[channelRemap[c]] = value;
                }

                srcStartIdx += resampleStep;
            }

            return convSamples - dstSampleIdxStart;
        }

        /// <summary>
        /// Converts the specified source sample to a destination sample for the specified channel.
        /// </summary>
        /// <param name="srcSampleIdx">The index of the sample within the source buffer.</param>
        /// <param name="dstChannel">The destination channel sampling for.</param>
        /// <param name="channelMap">The channel remapping map. Specifies which channel maps to which channel.</param>
        /// <returns>The specified sample as a float.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetChannelConvertedSample(int srcSampleIdx, int dstChannel, sbyte[] channelMap)
        {
            sbyte conversion = channelMap[dstChannel];
            if (conversion >= 0) return SoundData[srcSampleIdx + (byte) conversion];

            // Merge all source channels.
            float sampleAccum = 0;
            int sourceChannels = SourceFormat.Channels;
            if (conversion == CHANNEL_REMAP_MONO)
            {
                for (var i = 0; i < sourceChannels; i++)
                {
                    float channelSample = SoundData[srcSampleIdx + i];
                    sampleAccum += channelSample;
                }

                return sampleAccum / sourceChannels;
            }

            // Else merge channels conditionally based on the map.
            int destinationMultiMapConverted = -dstChannel - 1;
            for (var i = 0; i < sourceChannels; i++)
            {
                // Check if this source channel will sample to this dst channel.
                sbyte srcFlag = i < _surroundChannelsMapping.Length ? _surroundChannelsMapping[i] : REMAP_COMBINE;
                if (srcFlag != REMAP_COMBINE && srcFlag != destinationMultiMapConverted) continue;
                float channelSample = SoundData[srcSampleIdx + i];
                sampleAccum += channelSample;
            }

            return Maths.Clamp(sampleAccum, -1, 1);
        }

        #region Static API

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

        #endregion
    }
}