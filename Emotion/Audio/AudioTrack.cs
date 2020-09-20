#region Using

using System;
using System.Diagnostics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack : AudioStreamer
    {
        /// <summary>
        /// The audio file this track is playing.
        /// </summary>
        public AudioAsset File { get; set; }

        /// <summary>
        /// The layer the track is playing on.
        /// </summary>
        public AudioLayer Layer { get; set; }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get => Progress * File.Duration;
        }

        /// <summary>
        /// Fade in time in seconds. Null for none.
        /// If the number is negative it is the track progress (0-1)
        /// </summary>
        public float? FadeIn;

        /// <summary>
        /// Fade out time in seconds. Null for none.
        /// If the number is negative it is the track progress (0-1)
        /// </summary>
        public float? FadeOut;

        /// <summary>
        /// CrossFade this track into the next (if any)
        /// </summary>
        public float? CrossFade;

        public AudioTrack(AudioAsset file) : base(file.Format, file.SoundData)
        {
            File = file;
        }

        protected override void SetSampleAsFloat(int trueIndex, int index, float sample, Span<byte> buffer)
        {
            float volume = Layer.Volume * Engine.Configuration.MasterVolume;

            // The member properties Progress and Playback aren't used here
            // as we can make a more precise measurement using the sample index
            float progress = (float) trueIndex / _dstLength;
            float playback = progress * File.Duration;
            Debug.Assert(progress >= 0.0f && progress <= 1.0f);

            if (FadeIn != null)
            {
                float val = FadeIn.Value;
                float fadeProgress;
                if (FadeIn < 0)
                    fadeProgress = progress / -val;
                else
                    fadeProgress = playback / val;
                volume *= Maths.Clamp01(fadeProgress);
            }

            if (FadeOut != null || CrossFade != null)
            {
                float val = FadeOut ?? CrossFade.Value;
                var fadeProgress = 1.0f;
                float fileDuration = File.Duration;

                if (val < 0.0) val = fileDuration * -val;
                float activationTimeStamp = fileDuration - val;
                if (playback >= activationTimeStamp)
                    fadeProgress = 1.0f - (playback - activationTimeStamp) / val;

                volume *= Maths.Clamp01(fadeProgress);
            }

            volume = MathF.Pow(volume, Engine.Configuration.AudioCurve);
            sample *= volume;

            base.SetSampleAsFloat(trueIndex, index, sample, buffer);
        }
    }
}