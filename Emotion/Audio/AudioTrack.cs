#region Using

using System;
using System.Collections.Generic;
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

        public override float GetSampleAsFloat(int sampleIdx, bool trueIndex = false)
        {
            float volume = Layer.Volume * Engine.Configuration.MasterVolume;
            float sample = base.GetSampleAsFloat(sampleIdx, trueIndex);

            // The member properties Progress and Playback aren't used here
            // as we can make a more precise measurement using the sample index
            float progress = (float) sampleIdx / _sourceConvLength;
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

                if (val > 0.0)
                {
                    float activationTimeStamp = File.Duration - val;
                    if (playback >= activationTimeStamp)
                        fadeProgress = 1.0f - (playback - activationTimeStamp) / val;
                }
                else
                {
                    float activationProgress = 1.0f + val;
                    if (progress >= activationProgress)
                        fadeProgress = 1.0f - (progress - activationProgress) / -val;
                }

                volume *= Maths.Clamp01(fadeProgress);
            }

            volume = MathF.Pow(volume, Engine.Configuration.AudioCurve);
            sample *= volume;
            return sample;
        }
    }
}