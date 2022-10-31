#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
    public abstract partial class AudioLayer
    {
        /// <summary>
        /// Converts a volume 0-1 to a float to multiply 32f audio samples by to achieve gain.
        /// </summary>
        private static float VolumeToMultiplier(float volume)
        {
            volume = 20 * MathF.Log10(volume / 1f);
            volume = MathF.Pow(10, volume / 20f);
            return volume;
        }

        protected int GetProcessedFramesFromTrack(AudioFormat format, AudioTrack track, int frames, float[] memory, ref int playhead, bool applyFading = true)
        {
            // Record where these frames are gotten from.
            int channels = format.Channels;
            int startingFrame = playhead / channels;

            // Get data.
            int samples = track.File.AudioConverter.GetConvertedSamplesAt(format, playhead, frames, memory);
            playhead += samples;
            int framesOutput = samples / channels;
            Debug.Assert(framesOutput <= frames);

            // Force mono post process.
            // Dont apply force mono on tracks while the resampler applied mono to.
            bool mergeChannels = Engine.Configuration.ForceMono && channels != 1 && track.File.Format.Channels != 1;
            if (mergeChannels) PostProcessConvertToMono(framesOutput, memory, channels);

            // Apply fading, if needed. For instance we don't want additional fading while crossfading.
            if (!applyFading) return framesOutput;

            // If cross fading check if there is another track afterward.
            var modulatedUpToFrame = 0;

            if (_currentCrossFade != null)
            {
                modulatedUpToFrame = PostProcessCrossFade(format, _currentCrossFade, startingFrame, framesOutput, memory);
                if (modulatedUpToFrame == 0) _currentCrossFade = null;
            }

            // Apply fading. If the current track doesn't have a crossfade active.
            if (modulatedUpToFrame == 0)
                modulatedUpToFrame = PostProcessApplyFading(format, track, startingFrame, framesOutput, channels, memory);

            // Apply base volume modulation to the rest of the samples.
            float baseVolume = Volume * Engine.Configuration.MasterVolume;
            baseVolume = VolumeToMultiplier(baseVolume);
            for (int i = modulatedUpToFrame * format.Channels; i < samples; i++)
            {
                memory[i] *= baseVolume;
            }

            return framesOutput;
        }

        /// <summary>
        /// Convert multi-channel samples to mono.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PostProcessConvertToMono(int framesOutput, float[] soundData, int srcChannels)
        {
            for (var i = 0; i < framesOutput; i++)
            {
                int sampleIdx = i * srcChannels;
                float sampleAccum = 0;
                for (var c = 0; c < srcChannels; c++)
                {
                    sampleAccum += soundData[sampleIdx + c];
                }

                for (var c = 0; c < srcChannels; c++)
                {
                    soundData[sampleIdx + c] = sampleAccum / srcChannels;
                }
            }
        }

        // Replace with a SetVolume(val, time) API
        private int PostProcessApplyFading(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            if (_currentFadeOutAndStop != null)
            {
                int modulatedUpTo = InnerApplyFadeOut(format, _currentFadeOutAndStop.PlayHead, _currentFadeOutAndStop.DurationSeconds,
                    frameStart, frameCount, channels, soundData, _currentFadeOutAndStop.VolumeStart);
                return modulatedUpTo;
            }

            if (currentTrack.FadeIn != null && (!currentTrack.FadeInOnlyFirstLoop || _loopCount == 0))
            {
                int modulatedUpTo = ApplyFadeIn(format, currentTrack, frameStart, frameCount, channels, soundData);
                return modulatedUpTo;
            }

            if (currentTrack.FadeOut != null)
            {
                int modulatedUpTo = ApplyFadeOut(format, currentTrack, frameStart, frameCount, channels, soundData);
                return modulatedUpTo;
            }

            return 0;
        }

        private int ApplyFadeIn(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            // Calculate fade start.
            float currentTrackDuration = currentTrack.File.Duration;
            float fadeInDuration = currentTrack.FadeIn!.Value;
            if (fadeInDuration < 0)
                fadeInDuration = currentTrackDuration * -fadeInDuration;

            // How many frames of fade in.
            var fadeInFrames = (int) MathF.Floor(fadeInDuration * format.SampleRate);

            // Check if in fade in zone.
            if (frameStart > fadeInFrames) return 0;

            // Snap frame count. It's possible to have gotten frames outside fade zone.
            frameCount = Math.Min(fadeInFrames - frameStart, frameCount);

            // Go through frame data in granularity steps.
            float baseVolume = Volume * Engine.Configuration.MasterVolume;
            var localFrame = 0;
            while (localFrame < frameCount)
            {
                // Calculate volume for each granularity step based on the frame index.
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame); // Frames in step.
                int totalFrameIdx = frameStart + localFrame; // The index of the frame within the total count.
                float volume = (float) totalFrameIdx / fadeInFrames; // Volume is equal to how many frames into the fade. Linear curve 0-1.
                volume *= volume; // Cubic
                volume = baseVolume * volume;
                volume = VolumeToMultiplier(volume);

                // Apply to frame array.
                for (var i = 0; i < frames; i++)
                {
                    int frameInDataIdx = (localFrame + i) * channels;
                    for (var c = 0; c < channels; c++)
                    {
                        int sampleIdx = frameInDataIdx + c;
                        soundData[sampleIdx] *= volume;
                    }
                }

                localFrame += frames;
            }

            return localFrame;
        }

        private int ApplyFadeOut(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            // Refer to FadeIn comments.
            float currentTrackDuration = currentTrack.File.Duration;
            float fadeOutDuration = currentTrack.FadeOut!.Value;
            if (fadeOutDuration < 0)
                fadeOutDuration = currentTrackDuration * -fadeOutDuration;

            var fadeOutFrames = (int) MathF.Floor(fadeOutDuration * format.SampleRate);
            int fadeOutFrameStart = _totalSamplesConv / channels - fadeOutFrames;
            if (frameStart < fadeOutFrameStart) return 0;

            return InnerApplyFadeOut(format, fadeOutFrameStart, fadeOutDuration, frameStart, frameCount, channels, soundData);
        }

        private int InnerApplyFadeOut(AudioFormat format, int fadeOutFrameStart, float fadeOutDuration, int frameStart, int frameCount, int channels, float[] soundData, float volumeStart = 1f)
        {
            var fadeOutFrames = (int) MathF.Floor(fadeOutDuration * format.SampleRate);

            if (frameStart < fadeOutFrameStart) return 0;

            float baseVolume = Volume * Engine.Configuration.MasterVolume;
            var localFrame = 0;
            while (localFrame < frameCount)
            {
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
                int frameIdx = frameStart + localFrame;
                float volume = (float) (frameIdx - fadeOutFrameStart) / fadeOutFrames;
                volume = 1f - volume;

                if (volume < 0) volume = 0;
                volume *= volume; // Cubic
                volume = baseVolume * volume;
                volume = VolumeToMultiplier(volume);
                volume *= volumeStart;

                for (var i = 0; i < frames; i++)
                {
                    int frameInDataIdx = (localFrame + i) * channels;
                    for (var c = 0; c < channels; c++)
                    {
                        int sampleIdx = frameInDataIdx + c;
                        soundData[sampleIdx] *= volume;
                    }
                }

                localFrame += frames;
            }

            return localFrame;
        }

        /// <summary>
        /// Apply cross fading between two tracks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PostProcessCrossFade(AudioFormat format, FadeEffectData crossFadeData, int startingFrame, int frameCount, float[] soundData)
        {
            // Assuming we crossfade from the start of the new track, the duration is all we need.
            var crossFadeDurationFrames = (int) MathF.Floor(crossFadeData.DurationSeconds * format.SampleRate);
            if (crossFadeDurationFrames <= startingFrame) return 0;

            // Clamp to wherever the crossfade will finish.
            if (startingFrame + frameCount > crossFadeDurationFrames) frameCount = crossFadeDurationFrames - startingFrame;
            Debug.Assert(frameCount > 0);

            // Get data from the track we were fading from.
            frameCount = GetProcessedFramesFromTrack(format, crossFadeData.Track, frameCount, _internalBufferCrossFade, ref crossFadeData.PlayHead, false);

            // Apply fade in increments.
            int channels = format.Channels;
            float baseVolume = Volume * Engine.Configuration.MasterVolume;
            var localFrame = 0;
            while (localFrame < frameCount)
            {
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
                int frameIdx = startingFrame + localFrame;
                float frameT = (float) frameIdx / crossFadeDurationFrames;
                frameT = Maths.Clamp01(frameT);
                frameT = frameT * frameT; // Cubic

                float volumeFadingInto = frameT;
                float volumeFadingFrom = 1.0f - frameT;

                volumeFadingInto = VolumeToMultiplier(baseVolume * volumeFadingInto);
                volumeFadingFrom = VolumeToMultiplier(baseVolume * volumeFadingFrom);

                for (var i = 0; i < frames; i++)
                {
                    int frameInDataIdx = (localFrame + i) * channels;
                    for (var c = 0; c < channels; c++)
                    {
                        int sampleIdx = frameInDataIdx + c;
                        float sampleCurrentTrack = soundData[sampleIdx] * volumeFadingInto;
                        float sampleNextTrack = _internalBufferCrossFade[sampleIdx] * volumeFadingFrom;
                        soundData[sampleIdx] = sampleCurrentTrack + sampleNextTrack;
                    }
                }

                localFrame += frames;
            }

            return localFrame;
        }
    }
}