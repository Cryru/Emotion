// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// The way a animation will loop.
    /// </summary>
    public enum AnimationLoopType
    {
        /// <summary>
        /// The animation will play once.
        /// </summary>
        None,

        /// <summary>
        /// Animation will loop normally, after the last frame is the first frame.
        /// </summary>
        Normal,

        /// <summary>
        /// The animation will play in reverse after reaching then last frame.
        /// </summary>
        NormalThenReverse,

        /// <summary>
        /// The animation will play in reverse.
        /// </summary>
        Reverse,

        /// <summary>
        /// The animation will play once, in reverse.
        /// </summary>
        NoneReverse
    }
}