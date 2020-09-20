#region Using

using Emotion.IO;
using Emotion.Standard.Audio;

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
        /// The timestamp is the time to finish fading in at.
        /// </summary>
        public float? FadeIn;

        /// <summary>
        /// Fade out time in seconds. Null for none.
        /// If the number is negative it is the track progress (0-1)
        /// The timestamp is the time to begin fading out at.
        /// </summary>
        public float? FadeOut;

        /// <summary>
        /// CrossFade this track into the next (if any). The timestamp is the time to
        /// begin cross fading at.
        /// </summary>
        public float? CrossFade;

        public AudioTrack(AudioAsset file) : base(file.Format, file.SoundData)
        {
            File = file;
        }
    }
}