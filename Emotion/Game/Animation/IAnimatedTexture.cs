#region Using

using Emotion.Graphics.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// An interface handling animated textures/sprites.
    /// </summary>
    public interface IAnimatedTexture
    {
        #region Properties

        /// <summary>
        /// The type of animation loop to apply.
        /// </summary>
        AnimationLoopType LoopType { get; set; }

        /// <summary>
        /// The frame index to start from. Inclusive. Zero indexed.
        /// </summary>
        int StartingFrame { get; set; }

        /// <summary>
        /// The frame index to end on from the total frame count. Zero indexed. Inclusive. Set to -1 to animate to end.
        /// </summary>
        int EndingFrame { get; set; }

        /// <summary>
        /// The time between frames in milliseconds.
        /// </summary>
        int TimeBetweenFrames { get; set; }

        #endregion

        #region Info Only

        /// <summary>
        /// The index of the current frame within the total frame count. Zero indexed.
        /// </summary>
        int CurrentFrameIndex { get; }

        /// <summary>
        /// The total number of frames in the spritesheet. Zero indexed. Inclusive.
        /// </summary>
        int TotalFrames { get; }

        /// <summary>
        /// The texture corresponding to the current frame.
        /// </summary>
        Rectangle CurrentFrame
        {
            get => GetFrameBounds(CurrentFrameIndex);
        }

        /// <summary>
        /// The number of frames the configured animation has in total. Zero indexed.
        /// </summary>
        int AnimationFrames
        {
            get => (EndingFrame == -1 ? TotalFrames : EndingFrame) - StartingFrame;
        }

        /// <summary>
        /// The spritesheet texture associated with the animation.
        /// </summary>
        Texture Texture { get; }

        /// <summary>
        /// The number of times the animation looped.
        /// </summary>
        int LoopCount { get; }

        #endregion

        /// <summary>
        /// Advance time for the animation.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update.</param>
        void Update(float frameTime);

        /// <summary>
        /// Reset the animation.
        /// </summary>
        void Reset();

        /// <summary>
        /// Set the current frame to the specified frame.
        /// Animation will continue from there.
        /// If the index is invalid it will either be set to the starting or ending frame.
        /// </summary>
        /// <param name="index">The index to set the current frame to.</param>
        void SetFrame(int index);

        /// <summary>
        /// Get the bounds of the specified frame.
        /// </summary>
        /// <param name="frameId">The frame to get the bounds of.</param>
        /// <returns>The bounds of the requested frame.</returns>
        Rectangle GetFrameBounds(int frameId);

        /// <summary>
        /// Copy the animation data.
        /// </summary>
        /// <returns>A copy of this animated texture.</returns>
        IAnimatedTexture Copy();

        /// <summary>
        /// Returns a serializable animation description file.
        /// </summary>
        /// <param name="textureName">The spritesheet texture's name within the asset loader.</param>
        /// <returns>A serializable animation description file.</returns>
        AnimationDescriptionBase GetDescription(string textureName = null);
    }
}