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
        None = 0,

        /// <summary>
        /// Animation will loop normally, after the last frame is the first frame.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// The animation will play in reverse after reaching then last frame.
        /// </summary>
        NormalThenReverse = 2,

        /// <summary>
        /// The animation will play in reverse.
        /// </summary>
        Reverse = 3,

        /// <summary>
        /// The animation will play once, in reverse.
        /// </summary>
        NoneReverse = 4
    }
}