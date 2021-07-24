#region Using

using Emotion.IO;

#endregion

namespace Emotion.Audio
{
    /// <summary>
    /// Lightweight class which holds the playback settings for a file.
    /// </summary>
    public class AudioTrack
    {
        /// <summary>
        /// The audio file this track is playing.
        /// </summary>
        public AudioAsset File { get; }

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
        /// Whether to fade in only on the first loop.
        /// </summary>
        public bool FadeInOnlyFirstLoop = true;

        /// <summary>
        /// CrossFade this track into the next (if any). The timestamp is the time to
        /// begin cross fading at, and it will extend that much into the next track as well.
        /// Overrides the current track's fade out and the next track's fade in.
        /// Though if the fade in is longer than the crossfade, it will come in affect as soon as the crossfade finishes. (which
        /// you probably dont want)
        /// </summary>
        public float? CrossFade;

        /// <summary>
        /// Whether to set the layer's LoopingCurrent setting to true when played.
        /// </summary>
        public bool SetLoopingCurrent { get; set; }

        public AudioTrack(AudioAsset file)
        {
            File = file;
        }
    }
}