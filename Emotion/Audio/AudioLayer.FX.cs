#region Using

using System;
using System.Runtime.CompilerServices;
using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Audio
{
    public abstract partial class AudioLayer
    {
        // The interval in seconds to apply volume modulation in.
        // Calculating it for every frame is expensive, so we chunk it.
        public const float VOLUME_MODULATION_INTERVAL = 0.05f;

        private class VolumeModulationEffect
        {
            public float StartVolume;
            public float EndVolume;

            public int StartSample;
            public int EndSample;

            public VolumeModulationEffect(float startVol, float endVol, int startSample, int endSample)
            {
                StartVolume = startVol;
                EndVolume = endVol;

                StartSample = startSample;
                EndSample = endSample;
            }

            public float GetVolumeAt(int sample)
            {
                if (sample < StartSample) return StartVolume;
                if (sample > EndSample) return EndVolume;

                float duration = EndSample - StartSample;
                float progress = (sample - StartSample) / duration;

                // Apply cubic in a way that is always in the out direction.
                if (StartVolume > EndVolume)
                {
                    progress = 1.0f - progress;
                    progress *= progress;
                    progress = 1.0f - progress;
                }
                else
                {
                    progress *= progress;
                }

                return Maths.FastLerp(StartVolume, EndVolume, progress);
            }

            public void FormatChanged(AudioTrack currentTrack, AudioFormat old, AudioFormat neu)
            {
                float totalSamplesWere = currentTrack.File.AudioConverter.GetSampleCountInFormat(old);
                float totalSamplesNow = currentTrack.File.AudioConverter.GetSampleCountInFormat(neu);

                float progressStartSample = StartSample / totalSamplesWere;
                StartSample = (int) MathF.Floor(progressStartSample * totalSamplesNow);

                float progressEndSample = EndSample / totalSamplesWere;
                EndSample = (int) MathF.Floor(progressEndSample * totalSamplesNow);
            }
        }

        private VolumeModulationEffect _currentVol;

        private VolumeModulationEffect? _fadeInVol;
        private VolumeModulationEffect? _fadeOutVol;

        public void SetVolume(float volumeGoal, int ms)
        {
            //float startVol = GetVolume();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetVolume(int atSample)
        {
            var mod = 1f;
            if (_fadeInVol != null) mod *= _fadeInVol.GetVolumeAt(atSample);
            if (_fadeOutVol != null) mod *= _fadeOutVol.GetVolumeAt(atSample);
            return mod;
        }

        private void FormatChangedRecalculateFX(AudioTrack track, AudioFormat oldFormat, AudioFormat newFormat)
        {
            _fadeInVol?.FormatChanged(track, oldFormat, newFormat);
            _fadeOutVol?.FormatChanged(track, oldFormat, newFormat);
        }

        private void TrackChangedFX(AudioTrack currentTrack)
        {
            _fadeInVol = null;
            _fadeOutVol = null;

            if (currentTrack.FadeIn.HasValue)
            {
                AudioFormat format = _streamingFormat;

                // Calculate fade in frames.
                float val = currentTrack.FadeIn.Value;
                if (val < 0) val = currentTrack.File.Duration * -val;
                int fadeInSamples = format.GetFrameCount(val) * format.Channels;

                _fadeInVol = new VolumeModulationEffect(0f, 1f, 0, fadeInSamples);
            }

            if (currentTrack.FadeOut.HasValue)
            {
                AudioFormat format = _streamingFormat;

                // Calculate fade in frames.
                float val = currentTrack.FadeOut.Value;
                if (val < 0) val = currentTrack.File.Duration * -val;
                val = currentTrack.File.Duration - val;
                int fadeOutSampleStart = format.GetFrameCount(val) * format.Channels;

                _fadeOutVol = new VolumeModulationEffect(1f, 0f, fadeOutSampleStart, _totalSamplesConv);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVolumeToFrames(int sampleStart, Span<float> frames, int framesInBuffer, float modifier)
        {
            int channels = _streamingFormat.Channels;
            int samplesInBuffer = framesInBuffer * channels;

            int finalSample = sampleStart + samplesInBuffer;
            int intervalInSamples = _streamingFormat.GetFrameCount(VOLUME_MODULATION_INTERVAL) * channels;

            var startingInterval = (int) MathF.Floor((float) sampleStart / intervalInSamples);
            var endingInterval = (int) MathF.Ceiling((float) finalSample / intervalInSamples);

            var sampleIdx = 0;
            for (int i = startingInterval; i < endingInterval; i++)
            {
                int sampleIdxOfInterval = i * intervalInSamples;
                float intervalMod = modifier * GetVolume(sampleIdxOfInterval);
                intervalMod = VolumeToMultiplier(intervalMod);
                _testChe = intervalMod;

                int sampleAmount = Math.Min(intervalInSamples, samplesInBuffer - sampleIdx);
                int sampleAmountStart = sampleIdx;
                int sampleEnd = sampleAmountStart + sampleAmount;
                for (int s = sampleAmountStart; s < sampleEnd; s++)
                {
                    frames[s] *= intervalMod;
                }
                sampleIdx += sampleAmount;
            }
        }

        /// <summary>
        /// Called every request to apply any track defined fades.
        /// </summary>
        private void CheckTrackFades(AudioTrack track, int startFrame)
        {
            if (track.FadeIn.HasValue && (!track.FadeInOnlyFirstLoop || _loopCount == 0))
            {
            }

            //if (track.FadeOut.HasValue) SetVolume(0f, (int) (track.FadeOut.Value * 1000f));

            //if (track.CrossFade.HasValue) SetVolume(0f, (int) (track.CrossFade.Value * 1000f));
        }

        private float _testChe;

        /// <summary>
        /// Get the volume at the current playhead position.
        /// Use for debugging and visualization.
        /// </summary>
        public float GetCurrentVolume()
        {
            return _testChe;
        }

        #region API

        private void CrossFadeIntoNext()
        {
        }

        private void StartFadeOut()
        {
        }

        private void StartFadeIn(bool setVolumeZero, float seconds)
        {
            //if (setVolumeZero) ;
        }

        #endregion
    }
}