#region Using

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
    /// <summary>
    /// Converts audio of one audio format to another.
    /// Remaps channels, resizes sample size to floats, resamples sample rate.
    /// Optimized for feeding the sound device buffer audio data.
    /// Frame - Samples * Channels
    /// Sample - Sound, one for each channel
    /// </summary>
    public sealed class AudioConverter
    {
        public int SourceFrames { get; private set; }
        public int SourceSamplesPerChannel { get; private set; }
        public float[] SoundData { get; private set; }
        public AudioFormat SourceFormat { get; private set; }

        public AudioConverter(AudioFormat srcFormat, float[] soundData)
        {
            SourceFormat = srcFormat;
            SourceFrames = soundData.Length;
            SourceSamplesPerChannel = SourceFrames / srcFormat.Channels;
            SoundData = soundData;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleCountInFormat(AudioFormat format)
        {
            int dstChannels = format.Channels;
            int srcChannels = SourceFormat.Channels;
            int srcSampleRate = SourceFormat.SampleRate;
            int dstSampleRate = format.SampleRate;

            float sampleDiff = (float) dstSampleRate / srcSampleRate;
            return (int) (SourceFrames / srcChannels * sampleDiff) * dstChannels;
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
        /// <param name="requestedFrames">The number of frames (in the dstFormat) to convert.</param>
        /// <param name="buffer">The buffer to fill with converted samples.</param>
        /// <returns>How many frames were actually converted.</returns>
        public int GetResamplesFrames(AudioFormat dstFormat, int dstSampleIdxStart, int requestedFrames, Span<float> buffer)
        {
            switch (AudioResampleQuality)
            {
                case AudioResampleQuality.LowCubic:
                    requestedFrames = CubicResample(dstFormat, dstSampleIdxStart, requestedFrames, buffer);
                    break;
                case AudioResampleQuality.MediumHermite:
                    requestedFrames = CatMullResample(dstFormat, dstSampleIdxStart, requestedFrames, buffer);
                    break;
                case AudioResampleQuality.HighHann:
                    requestedFrames = HannResample(dstFormat, dstSampleIdxStart, requestedFrames, buffer);
                    break;
            }

            return requestedFrames;
        }

        private int CubicResample(AudioFormat dstFormat, int dstSampleIdxStart, int requestedFrames, Span<float> buffer)
        {
            int dstChannels = dstFormat.Channels;
            int srcChannels = SourceFormat.Channels;
            int srcSampleRate = SourceFormat.SampleRate;
            int dstSampleRate = dstFormat.SampleRate;

            // Snap if more samples requested than left.
            int totalSamples = GetSampleCountInFormat(dstFormat);
            if (dstSampleIdxStart + requestedFrames * dstChannels >= totalSamples) requestedFrames = (totalSamples - dstSampleIdxStart) / dstChannels;

            // For remapping sound channels we use a cache.
            sbyte[] channelRemap = GetChannelRemappingMapFor(srcChannels, dstChannels);
            bool useChannelCache = dstChannels > 2;
            bool getSamplesDirectly = dstChannels == srcChannels;

            float resampleStep = (float) srcSampleRate / dstSampleRate;

            float[] soundData = SoundData;

            // Resample from dstSampleIdx to the requested samples.
            int dstFrameIdxStart = dstSampleIdxStart / dstChannels;
            for (int dstFrameIdx = dstFrameIdxStart; dstFrameIdx < dstFrameIdxStart + requestedFrames; dstFrameIdx++)
            {
                // Reset channel resample cache.
                if (useChannelCache)
                {
                    _surroundResampleCache[0] = float.NaN;
                    _surroundResampleCache[1] = float.NaN;
                }

                float srcFrame = dstFrameIdx * resampleStep;

                var previousFrameIdx = (int) MathF.Floor(srcFrame);
                var nextFrameIdx = (int) MathF.Ceiling(srcFrame);

                float fraction = srcFrame - previousFrameIdx;
                fraction *= fraction; // Cubic

                int previousFrameFirstSampleIdx = Math.Min(previousFrameIdx, SourceSamplesPerChannel - 1) * srcChannels;
                int nextFrameFirstSampleIdx = Math.Min(nextFrameIdx, SourceSamplesPerChannel - 1) * srcChannels;

                int targetBufferIdx = (dstFrameIdx - dstFrameIdxStart) * dstChannels;
                for (var c = 0; c < dstChannels; c++)
                {
                    // Multichannel mapping can skip some resample cases as it knows the result will match.
                    if (useChannelCache)
                    {
                        sbyte convChannel = channelRemap[c];
                        if (convChannel >= 0 && convChannel <= 1 && !float.IsNaN(_surroundResampleCache[convChannel]))
                        {
                            buffer[targetBufferIdx + c] = _surroundResampleCache[convChannel];
                            continue;
                        }
                    }

                    float previousSample = getSamplesDirectly ? soundData[previousFrameFirstSampleIdx + c] : GetChannelConvertedSample(previousFrameFirstSampleIdx, c, channelRemap);
                    float nextSample = getSamplesDirectly ? soundData[nextFrameFirstSampleIdx + c] : GetChannelConvertedSample(nextFrameFirstSampleIdx, c, channelRemap);

                    float value = Maths.FastLerp(previousSample, nextSample, fraction);
                    buffer[targetBufferIdx + c] = value;
                    if (useChannelCache) _surroundResampleCache[channelRemap[c]] = value;
                }
            }

            return requestedFrames;
        }

        private int CatMullResample(AudioFormat dstFormat, int dstSampleIdxStart, int requestedFrames, Span<float> buffer)
        {
            int dstChannels = dstFormat.Channels;
            int srcChannels = SourceFormat.Channels;
            int srcSampleRate = SourceFormat.SampleRate;
            int dstSampleRate = dstFormat.SampleRate;

            // Snap if more samples requested than left.
            int totalSamples = GetSampleCountInFormat(dstFormat);
            if (dstSampleIdxStart + requestedFrames * dstChannels >= totalSamples) requestedFrames = (totalSamples - dstSampleIdxStart) / dstChannels;

            // For remapping sound channels we use a cache.
            sbyte[] channelRemap = GetChannelRemappingMapFor(srcChannels, dstChannels);
            bool useChannelCache = dstChannels > 2;
            bool getSamplesDirectly = dstChannels == srcChannels;

            float resampleStep = (float) srcSampleRate / dstSampleRate;

            float[] soundData = SoundData;

            // Resample from dstSampleIdx to the requested samples.
            int dstFrameIdxStart = dstSampleIdxStart / dstChannels;
            for (int dstFrameIdx = dstFrameIdxStart; dstFrameIdx < dstFrameIdxStart + requestedFrames; dstFrameIdx++)
            {
                // Reset channel resample cache.
                if (useChannelCache)
                {
                    _surroundResampleCache[0] = float.NaN;
                    _surroundResampleCache[1] = float.NaN;
                }

                float srcFrame = dstFrameIdx * resampleStep;

                var previousFrameIdx = (int) MathF.Floor(srcFrame);
                var nextFrameIdx = (int) MathF.Ceiling(srcFrame);

                float fraction = srcFrame - previousFrameIdx;
                float fractionSquared = fraction * fraction;
                float fractionCubed = fractionSquared * fraction;

                int previousFrameFirstSampleIdx = Math.Min(previousFrameIdx, SourceSamplesPerChannel - 1) * srcChannels;
                int nextFrameFirstSampleIdx = Math.Min(nextFrameIdx, SourceSamplesPerChannel - 1) * srcChannels;

                // Get Catmull control points
                int previousControlPointIdx = Math.Min(previousFrameIdx - 1, SourceSamplesPerChannel - 1) * srcChannels;
                if (previousControlPointIdx < 0) previousControlPointIdx = 0;
                int nextControlPointIdx = Math.Min(nextFrameIdx + 1, SourceSamplesPerChannel - 1) * srcChannels;

                int targetBufferIdx = (dstFrameIdx - dstFrameIdxStart) * dstChannels;
                for (var c = 0; c < dstChannels; c++)
                {
                    // Multichannel mapping can skip some resample cases as it knows the result will match.
                    if (useChannelCache)
                    {
                        sbyte convChannel = channelRemap[c];
                        if (convChannel >= 0 && convChannel <= 1 && !float.IsNaN(_surroundResampleCache[convChannel]))
                        {
                            buffer[targetBufferIdx + c] = _surroundResampleCache[convChannel];
                            continue;
                        }
                    }

                    float previousSample = getSamplesDirectly ? soundData[previousFrameFirstSampleIdx + c] : GetChannelConvertedSample(previousFrameFirstSampleIdx, c, channelRemap);
                    float nextSample = getSamplesDirectly ? soundData[nextFrameFirstSampleIdx + c] : GetChannelConvertedSample(nextFrameFirstSampleIdx, c, channelRemap);

                    float previousControlPoint = getSamplesDirectly ? soundData[previousControlPointIdx + c] : GetChannelConvertedSample(previousControlPointIdx, c, channelRemap);
                    float nextControlPoint = getSamplesDirectly ? soundData[nextControlPointIdx + c] : GetChannelConvertedSample(nextControlPointIdx, c, channelRemap);

                    float v0 = previousControlPoint;
                    float v1 = previousSample;
                    float v2 = nextSample;
                    float v3 = nextControlPoint;

                    // The CatMull in Maths uses doubles and will not reuse fractions for each channel,
                    // so instead of using it, we just write it out.
                    float val = 0.5f * (2.0f * v1 +
                                        (v2 - v0) * fraction +
                                        (2.0f * v0 - 5.0f * v1 + 4.0f * v2 - v3) * fractionSquared +
                                        (3.0f * v1 - v0 - 3.0f * v2 + v3) * fractionCubed);

                    buffer[targetBufferIdx + c] = val;
                    if (useChannelCache) _surroundResampleCache[channelRemap[c]] = val;
                }
            }

            return requestedFrames;
        }

        private int HannResample(AudioFormat dstFormat, int dstSampleIdxStart, int requestedFrames, Span<float> buffer)
        {
            int dstChannels = dstFormat.Channels;
            int srcChannels = SourceFormat.Channels;
            int srcSampleRate = SourceFormat.SampleRate;
            int dstSampleRate = dstFormat.SampleRate;

            // Snap if more samples requested than left.
            int totalSamples = GetSampleCountInFormat(dstFormat);
            if (dstSampleIdxStart + requestedFrames * dstChannels >= totalSamples) requestedFrames = (totalSamples - dstSampleIdxStart) / dstChannels;

            // For remapping sound channels we use a cache.
            sbyte[] channelRemap = GetChannelRemappingMapFor(srcChannels, dstChannels);
            bool useChannelCache = dstChannels > 2;
            bool getSamplesDirectly = dstChannels == srcChannels;

            float resampleStep = (float) srcSampleRate / dstSampleRate;

            // Nyquist half of destination sampleRate
            const double maxDivSr = 0.5f;
            const double rG = 2 * maxDivSr;
            const int convQuality = 10;
            const int convQuality2 = 10 * 2;
            float[] soundData = SoundData;
            int samplesPerChannel = SourceSamplesPerChannel;

            // Resample from dstSampleIdx to the requested samples.
            int dstFrameIdxStart = dstSampleIdxStart / dstChannels;
            for (int dstFrameIdx = dstFrameIdxStart; dstFrameIdx < dstFrameIdxStart + requestedFrames; dstFrameIdx++)
            {
                // Reset channel resample cache.
                if (useChannelCache)
                {
                    _surroundResampleCache[0] = float.NaN;
                    _surroundResampleCache[1] = float.NaN;
                }

                float srcFrame = dstFrameIdx * resampleStep;

                int targetBufferIdx = (dstFrameIdx - dstFrameIdxStart) * dstChannels;
                for (var c = 0; c < dstChannels; c++)
                {
                    // Multichannel mapping can skip some resample cases as it knows the result will match.
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
                        var inputSampleIdx = (int) (srcFrame + tau);
                        double relativeIdx = inputSampleIdx - srcFrame;

                        // Hann Window. Scale and calculate sinc
                        double rW = 0.5 - 0.5 * Math.Cos(Maths.TWO_PI_DOUBLE * (0.5 + relativeIdx / convQuality2));
                        double rA = Maths.TWO_PI_DOUBLE * relativeIdx * maxDivSr;
                        var rSnc = 1.0;
                        if (rA != 0) rSnc = Math.Sin(rA) / rA;
                        if (inputSampleIdx < 0 || inputSampleIdx >= samplesPerChannel) continue;

                        float sample = getSamplesDirectly ? soundData[inputSampleIdx * srcChannels + c] : GetChannelConvertedSample(inputSampleIdx * srcChannels, c, channelRemap);
                        rY += rG * rW * rSnc * sample;
                    }

                    float value = Maths.Clamp((float) rY, -1f, 1f);
                    buffer[targetBufferIdx + c] = value;
                    if (useChannelCache) _surroundResampleCache[channelRemap[c]] = value;
                }
            }

            return requestedFrames;
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

        /// <summary>
        /// The quality of the audio resampler.
        /// </summary>
        public static AudioResampleQuality AudioResampleQuality { get; private set; } = AudioResampleQuality.LowCubic;

        /// <summary>
        /// Set the quality of the audio resampler.
        /// </summary>
        public static void SetResamplerQuality(AudioResampleQuality quality = AudioResampleQuality.Auto)
        {
            if (quality == AudioResampleQuality.Auto)
            {
                // Use cores to gauge cpu power, technically not correct but serviceable.
                int cpuCores = Environment.ProcessorCount;
                switch (cpuCores)
                {
                    case 1:
                    case 2:
                        AudioResampleQuality = AudioResampleQuality.LowCubic;
                        break;
                    //case 3:
                    //case 4:
                    default:
                        AudioResampleQuality = AudioResampleQuality.MediumHermite;
                        break;
                    // Note: I can't perceive this as better and it is slower than Hermite...
                    //default:
                    //    AudioResampleQuality = AudioResampleQuality.HighHann;
                    //    break;
                }
            }
            else
            {
                AudioResampleQuality = quality;
            }

            Engine.Log.Info($"Set audio resample quality to {AudioResampleQuality}", MessageSource.Audio);
        }

        #endregion
    }
}