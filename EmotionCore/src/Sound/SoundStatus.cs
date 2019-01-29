// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Sound
{
    /// <summary>
    /// The status of the sound layer.
    /// </summary>
    public enum SoundStatus
    {
        /// <summary>
        /// Initial status for when the layer is created but nothing has been done to it.
        /// </summary>
        Initial,
        /// <summary>
        /// The layer is playing a track.
        /// </summary>
        Playing,
        /// <summary>
        /// The layer is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// The layer has either finished playing or was stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// The layer was playing but the host lost focus so it was paused. Will resume when focus is regained. You should never see this.
        /// </summary>
        FocusLossPause
    }
}