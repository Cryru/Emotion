#region Using

using Emotion.Graphics.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// An interface handling animated textures/sprites.
    /// </summary>
    public abstract class AnimatedTextureBase
    {
        #region Properties

        /// <summary>
        /// The type of animation loop to apply.
        /// </summary>
        public AnimationLoopType LoopType { get; set; }

        /// <summary>
        /// The frame index to start from. Inclusive. Zero indexed.
        /// </summary>
        public int StartingFrame { get; set; }

        /// <summary>
        /// The frame index to end on from the total frame count. Zero indexed. Inclusive. Set to -1 to animate to end.
        /// </summary>
        public int EndingFrame { get; set; }

        /// <summary>
        /// The time between frames in milliseconds.
        /// </summary>
        public int TimeBetweenFrames { get; set; }

        #endregion

        #region Info Only

        /// <summary>
        /// The index of the current frame within the total frame count. Zero indexed.
        /// </summary>
        public int CurrentFrameIndex { get; protected set; }

        /// <summary>
        /// The total number of frames in the spritesheet. Zero indexed. Inclusive.
        /// </summary>
        public abstract int TotalFrames { get; }

        /// <summary>
        /// The texture corresponding to the current frame.
        /// </summary>
        public Rectangle CurrentFrame
        {
            get => GetFrameBounds(CurrentFrameIndex);
        }

        /// <summary>
        /// The number of frames the configured animation has in total. Zero indexed.
        /// </summary>
        public int AnimationFrames
        {
            get => (EndingFrame == -1 ? TotalFrames : EndingFrame) - StartingFrame;
        }

        /// <summary>
        /// The spritesheet texture associated with the animation.
        /// </summary>
        public Texture Texture { get; protected set; }

        /// <summary>
        /// The number of times the animation looped.
        /// </summary>
        public int LoopCount { get; protected set; }

        #endregion

        protected float _timePassed;

        /// <summary>
        /// Advance time for the animation.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update.</param>
        public abstract void Update(float frameTime);

        /// <summary>
        /// Reset the animation.
        /// </summary>
        public virtual void Reset()
        {
            _timePassed = 0;
            ClampFrameParameters();
            CurrentFrameIndex = GetStartingFrame();
            LoopCount = 0;
        }

        /// <summary>
        /// Set the current frame to the specified frame.
        /// Animation will continue from there.
        /// If the index is invalid it will either be set to the starting or ending frame.
        /// </summary>
        /// <param name="index">The index to set the current frame to.</param>
        public virtual void SetFrame(int index)
        {
            CurrentFrameIndex = index;
            ClampFrameParameters();
            ClampCurrentFrame();
        }

        /// <summary>
        /// Get the bounds of the specified frame.
        /// </summary>
        /// <param name="frameId">The frame to get the bounds of.</param>
        /// <returns>The bounds of the requested frame.</returns>
        public abstract Rectangle GetFrameBounds(int frameId);

        /// <summary>
        /// Copy the animation data.
        /// </summary>
        /// <returns>A copy of this animated texture.</returns>
        public abstract AnimatedTextureBase Copy();

        /// <summary>
        /// Returns a serializable animation description file.
        /// </summary>
        /// <param name="textureName">The spritesheet texture's name within the asset loader.</param>
        /// <returns>A serializable animation description file.</returns>
        public abstract AnimationDescriptionBase GetDescription(string textureName = null);

        #region Helpers

        /// <summary>
        /// Clamp the starting and ending frames.
        /// </summary>
        protected void ClampFrameParameters()
        {
            if (StartingFrame < 0)
            {
                StartingFrame = 0;
                CurrentFrameIndex = GetStartingFrame();
            }

            if (EndingFrame > TotalFrames || EndingFrame == -1) EndingFrame = TotalFrames;
            if (StartingFrame > EndingFrame)
            {
                StartingFrame = 0;
                CurrentFrameIndex = GetStartingFrame();
            }

            if (EndingFrame < StartingFrame) EndingFrame = TotalFrames;
        }

        /// <summary>
        /// Clamp the current frame.
        /// </summary>
        protected void ClampCurrentFrame()
        {
            // Clamp frame within range.
            if (CurrentFrameIndex < StartingFrame) CurrentFrameIndex = StartingFrame;
            if (CurrentFrameIndex > EndingFrame) CurrentFrameIndex = EndingFrame;
        }

        /// <summary>
        /// Returns the starting frame for the current loop type.
        /// </summary>
        /// <returns>Returns the first frame for the current loop type.</returns>
        protected int GetStartingFrame()
        {
            switch (LoopType)
            {
                case AnimationLoopType.None:
                case AnimationLoopType.Normal:
                case AnimationLoopType.NormalThenReverse:
                    return StartingFrame;
                case AnimationLoopType.Reverse:
                case AnimationLoopType.NoneReverse:
                    return EndingFrame;
            }

            return -1;
        }

        #endregion
    }
}