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
            if (mergeChannels) PostProcessForceMono(framesOutput, memory, channels);

            // Apply base volume modulation.
            float baseVolume = Volume * Engine.Configuration.MasterVolume;
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

            // Apply fading, if needed. For instance we don't want additional fading while crossfading.
            if (!applyFading) return framesOutput;

            // If cross fading check if there is another track afterward.
            bool crossfade = track.CrossFade.HasValue;
            if (crossfade)
            {
                AudioTrack nextTrack = null;
                if (LoopingCurrent)
                    nextTrack = _loopCount > 0 ? track : null; // Dont crossfade on first play of loop.
                else
                    lock (_playlist)
                    {
                        if (_playlist.Count > 1) nextTrack = _playlist[1];
                    }

                if (nextTrack != null)
                {
                    PostProcessCrossFade(format, track, nextTrack, startingFrame, framesOutput, memory);
                    return framesOutput;
                }
            }

            // Apply fading. If the current track doesn't have a crossfade active.
            PostProcessApplyFading(format, track, startingFrame, framesOutput, channels, memory);
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

        private void PostProcessApplyFading(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            if (currentTrack.FadeIn != null && (!currentTrack.FadeInOnlyFirstLoop || _loopCount == 0))
                ApplyFadeIn(format, currentTrack, frameStart, frameCount, channels, soundData);
            if (currentTrack.FadeOut != null) ApplyFadeOut(format, currentTrack, frameStart, frameCount, channels, soundData);
        }

        private void ApplyFadeIn(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            // Calculate fade start.
            float currentTrackDuration = currentTrack.File.Duration;
            float fadeInDuration = currentTrack.FadeIn!.Value;
            if (fadeInDuration < 0)
                fadeInDuration = currentTrackDuration * -fadeInDuration;

            // How many frames of fade in.
            var fadeInFrames = (int)MathF.Floor(fadeInDuration * format.SampleRate);

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
                float volume = (float)totalFrameIdx / fadeInFrames; // Volume is equal to how many frames into the fade. Linear curve 0-1.
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

        private void ApplyFadeOut(AudioFormat format, AudioTrack currentTrack, int frameStart, int frameCount, int channels, float[] soundData)
        {
            // Refer to FadeIn comments.
            float currentTrackDuration = currentTrack.File.Duration;
            float fadeOutDuration = currentTrack.FadeOut!.Value;
            if (fadeOutDuration < 0)
                fadeOutDuration = currentTrackDuration * -fadeOutDuration;

            var fadeOutFrames = (int)MathF.Floor(fadeOutDuration * format.SampleRate);
            int fadeOutFrameStart = _totalSamplesConv / channels - fadeOutFrames;
            if (frameStart <= fadeOutFrameStart) return;

            var localFrame = 0;
            while (localFrame < frameCount)
            {
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
                int frameIdx = frameStart + localFrame;
                float volume = (float)(frameIdx - fadeOutFrameStart) / fadeOutFrames;
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
        private void PostProcessCrossFade(AudioFormat format, AudioTrack currentTrack, AudioTrack nextTrack, int startingFrame, int frameCount, float[] soundData)
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
            int crossFadeFrames = (int)MathF.Floor(crossFadeDuration * format.SampleRate * channels) / channels;
            int crossFadeStartAtFrame = _totalSamplesConv / channels - crossFadeFrames;
            if (crossFadeStartAtFrame > startingFrame) return;

            // Get data from the next track.
            int nextTrackFrameCount = GetProcessedFramesFromTrack(format, nextTrack, frameCount, _internalBufferCrossFade, ref _crossFadePlayHead, false);
            Debug.Assert(nextTrackFrameCount == frameCount);

            // Consistent power cross fade
            var localFrame = 0;
            while (localFrame < frameCount)
            {
                int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
                int frameIdx = startingFrame + localFrame;
                float frameT = (float)(frameIdx - crossFadeStartAtFrame) / crossFadeFrames;
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
                        float sampleNextTrack = _internalBufferCrossFade[sampleIdx] * volumeNex;
                        soundData[sampleIdx] = sampleCurrentTrack + sampleNextTrack;
                    }
                }

                localFrame += frames;
            }
        }
    }
}