#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public abstract partial class AudioLayer
    {
        [Flags]
        protected enum PostProcParam
        {
            None,
            FromCrossFade = 0x1,
            UseSecondaryPlayHead = 0x10
        }

        protected static int GetProcessedFramesFromTrack(
            AudioFormat format,
            AudioTrack track,
            int frames,
            float[] memory,
            float baseVolume,
            AudioTrack nextTrack,
            float[] crossFadeMemory,
            PostProcParam param = PostProcParam.None
        )
        {
            // Set the conversion format to the requested one - if it doesn't match.
            track.EnsureAudioFormat(format);

            // Record where these frames are gotten from.
            int startingFrame = track.SampleIndex / format.Channels;

            // Get data.
            int framesOutput = track.GetNextFrames(frames, memory, param.HasFlag(PostProcParam.UseSecondaryPlayHead));
            Debug.Assert(framesOutput <= frames);

            // Force mono post process.
            // Dont apply force mono on tracks while the resampler applied mono to.
            int channels = format.Channels;
            bool mergeChannels = Engine.Configuration.ForceMono && channels != 1 && track.File.Format.Channels != 1;
            if (mergeChannels) PostProcessForceMono(framesOutput, memory, channels);

            // Apply base volume modulation.
            baseVolume = MathF.Pow(baseVolume, Engine.Configuration.AudioCurve);
            for (var i = 0; i < frames; i++)
            {
                int frameIdx = i * channels;
                for (var c = 0; c < channels; c++)
                {
                    int sampleIdx = frameIdx + c;
                    memory[sampleIdx] *= baseVolume;
                }
            }

            // Apply fading. (Only if not sampling for a cross-fade)
            if (param.HasFlag(PostProcParam.FromCrossFade)) return framesOutput;

            bool noCrossFade = nextTrack == null;
            if (noCrossFade)
                PostProcessApplyFading(format, track, startingFrame, framesOutput, channels, memory);
            else
                PostProcessCrossFade(format, track, nextTrack, startingFrame, framesOutput, baseVolume, memory, crossFadeMemory);

            return framesOutput;
        }


        /// <summary>
        /// Check if forcing mono sound. This matters only if both the source and destination formats are stereo.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PostProcessForceMono(int framesOutput, float[] soundData, int srcChannels)
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

        private static void PostProcessApplyFading(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            if (currentTrack.FadeIn != null) ApplyFadeIn(format, currentTrack, frameStart, frameCount, channels, soundData);
            if (currentTrack.FadeOut != null) ApplyFadeOut(format, currentTrack, frameStart, frameCount, channels, soundData);
        }

        private static void ApplyFadeIn(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            // Calculate fade start.
            float currentTrackDuration = currentTrack.File.Duration;
            float fadeInDuration = currentTrack.FadeIn!.Value;
            if (fadeInDuration < 0)
                fadeInDuration = currentTrackDuration * -fadeInDuration;

            // How many frames of fade in.
            var fadeInFrames = (int) MathF.Floor(fadeInDuration * format.SampleRate);

            // Check if in fade in zone.
            if (frameStart >= fadeInFrames) return;

            // Snap frame count. It's possible to have gotten frames outside fade zone.
            frameCount = Math.Min(fadeInFrames - frameStart, frameCount);

            // Go through frame data in granularity steps.
            var localFrame = 0;
            while (localFrame < frameCount)
            {
                // Calculate volume for each granularity step based on the frame index.
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame); // Frames in step.
                int totalFrameIdx = frameStart + localFrame; // The index of the frame within the total count.
                float volume = (float) totalFrameIdx / fadeInFrames; // Volume is equal to how many frames into the fade. Linear curve 0-1.
                volume = MathF.Pow(volume, Engine.Configuration.AudioCurve);

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
        }

        private static void ApplyFadeOut(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            // Refer to FadeIn comments.
            float currentTrackDuration = currentTrack.File.Duration;
            float fadeOutDuration = currentTrack.FadeOut!.Value;
            if (fadeOutDuration < 0)
                fadeOutDuration = currentTrackDuration * -fadeOutDuration;

            var fadeOutFrames = (int) MathF.Floor(fadeOutDuration * format.SampleRate);
            int fadeOutFrameStart = currentTrack.TotalSamples / channels - fadeOutFrames;
            if (frameStart <= fadeOutFrameStart) return;

            var localFrame = 0;
            while (localFrame < frameCount)
            {
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
                int frameIdx = frameStart + localFrame;
                float volume = (float) (frameIdx - fadeOutFrameStart) / fadeOutFrames;
                volume = MathF.Pow(1f - volume, Engine.Configuration.AudioCurve);

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
        }

        /// <summary>
        /// Apply cross fading between two tracks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PostProcessCrossFade(AudioFormat format, AudioTrack currentTrack, AudioTrack nextTrack, int startingFrame, int frameCount, float baseVolume, float[] soundData,
            float[] crossFadeMemory)
        {
            int channels = format.Channels;
            float currentTrackDuration = currentTrack.File.Duration;
            float nextTrackDuration = nextTrack.File.Duration;
            float crossFadeDuration = currentTrack.CrossFade!.Value;

            if (crossFadeDuration < 0.0f) crossFadeDuration = currentTrackDuration - currentTrackDuration * -crossFadeDuration;
            // Make sure there is enough duration in the next track to cross fade into. Leave at least one second so we don't exhaust the track.
            crossFadeDuration = Math.Min(crossFadeDuration, nextTrackDuration - 1);

            // Add a fade in to the next track (if none). Makes the cross fade better.
            // The current track already has a fade out applied in post processing.
            int crossFadeFrames = (int) MathF.Floor(crossFadeDuration * format.SampleRate * channels) / channels;
            int crossFadeStartAtFrame = currentTrack.TotalSamples / channels - crossFadeFrames;
            if (crossFadeStartAtFrame > startingFrame) return;

            // Get data from the next track.
            var param = PostProcParam.FromCrossFade;
            if (nextTrack == currentTrack) param |= PostProcParam.UseSecondaryPlayHead;
            int nextTrackFrameCount = GetProcessedFramesFromTrack(format, nextTrack, frameCount, crossFadeMemory, baseVolume, null, null, param);
            Debug.Assert(nextTrackFrameCount == frameCount);

            // Consistent power cross fade
            var localFrame = 0;
            while (localFrame < frameCount)
            {
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
                int frameIdx = startingFrame + localFrame;
                float frameT = (float) (frameIdx - crossFadeStartAtFrame) / crossFadeFrames;
                frameT = frameT * 2.0f - 1.0f;

                float volumeNex = MathF.Sqrt(0.5f * (1f + frameT));
                float volumeCur = MathF.Sqrt(0.5f * (1f - frameT));
                for (var i = 0; i < frames; i++)
                {
                    int frameInDataIdx = (localFrame + i) * channels;
                    for (var c = 0; c < channels; c++)
                    {
                        int sampleIdx = frameInDataIdx + c;
                        float sampleCurrentTrack = soundData[sampleIdx] * volumeCur;
                        float sampleNextTrack = crossFadeMemory[sampleIdx] * volumeNex;
                        soundData[sampleIdx] = sampleCurrentTrack + sampleNextTrack;
                    }
                }

                localFrame += frames;
            }
        }
    }
}