// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utils;

#endregion

namespace Emotion.Game.Animation
{
    public sealed class AnimatedTexture
    {
        #region Properties

        /// <summary>
        /// The spritesheet texture associated with the animation.
        /// </summary>
        public Texture Texture { get; private set; }

        /// <summary>
        /// The number of times to loop the animation.
        /// </summary>
        public int LoopCount { get; private set; }

        /// <summary>
        /// The type of animation loop to apply.
        /// </summary>
        public AnimationLoopType LoopType { get; set; }

        /// <summary>
        /// The frame index to start from. Inclusive. Zero indexed.
        /// </summary>
        public int StartingFrame { get; set; }

        /// <summary>
        /// The frame index to end on from the total frame count. Inclusive.
        /// </summary>
        public int EndingFrame { get; set; }

        /// <summary>
        /// The time between frames in milliseconds.
        /// </summary>
        public float TimeBetweenFrames { get; set; }

        /// <summary>
        /// The index of the current frame within the total frame count.
        /// </summary>
        public int CurrentFrameIndex { get; private set; }

        /// <summary>
        /// The texture corresponding to the current frame.
        /// </summary>
        public Rectangle CurrentFrame
        {
            get => Helpers.GetFrameBounds(Texture.Size, _frameSize, _spacing, CurrentFrameIndex);
        }

        /// <summary>
        /// The number of frames the configured animation has in total.
        /// </summary>
        public int AnimationFrames
        {
            get => GetTotalFrames();
        }

        #endregion

        private Vector2 _frameSize;
        private Vector2 _spacing;

        private float _timePassed;
        private bool _inReverse;

        /// <summary>
        /// Create a new animated texture object.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frameSize">The size of frames within the texture. It is assumed that all frames are of the same size.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Inclusive.</param>
        public AnimatedTexture(Texture texture, Vector2 frameSize, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame, int endingFrame) : this(texture, frameSize, Vector2.Zero,
            loopType, timeBetweenFrames, startingFrame, endingFrame)
        {
        }

        /// <summary>
        /// Create a new animated texture object.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frameSize">The size of frames within the texture. It is assumed that all frames are of the same size.</param>
        /// <param name="spacing">The spacing between frames in the spritesheet. Also applied as an outermost border.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Inclusive.</param>
        public AnimatedTexture(Texture texture, Vector2 frameSize, Vector2 spacing, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame, int endingFrame)
        {
            Texture = texture;
            _frameSize = frameSize;
            _spacing = spacing;

            LoopType = loopType;
            StartingFrame = startingFrame;
            EndingFrame = endingFrame;
            TimeBetweenFrames = timeBetweenFrames;

            Reset();
        }

        /// <summary>
        /// Advance time for the animation.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update.</param>
        public void Update(float frameTime)
        {
            // Error checkers.
            AssureFrameInRange();

            _timePassed += frameTime;

            // Timer.
            if (!(_timePassed > TimeBetweenFrames)) return;
            _timePassed -= TimeBetweenFrames;
            NextFrame();
        }

        /// <summary>
        /// Reset the animation.
        /// </summary>
        public void Reset()
        {
            _timePassed = 0;
            CurrentFrameIndex = GetStartingFrame();
            LoopCount = 0;
        }

        #region Logic

        /// <summary>
        /// Iterate to the next frame.
        /// </summary>
        private void NextFrame()
        {
            switch (LoopType)
            {
                case AnimationLoopType.None:
                    if (LoopCount > 0) return;
                    // If the global frame is the last frame.
                    if (CurrentFrameIndex == EndingFrame)
                        LoopCount++;
                    else
                        CurrentFrameIndex++;
                    break;
                case AnimationLoopType.Normal:
                    // If the global frame is the last frame.
                    if (CurrentFrameIndex == EndingFrame)
                    {
                        CurrentFrameIndex = StartingFrame;
                        LoopCount++;
                    }
                    else
                    {
                        CurrentFrameIndex++;
                    }

                    break;
                case AnimationLoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if (CurrentFrameIndex == EndingFrame && _inReverse == false ||
                        CurrentFrameIndex == StartingFrame && _inReverse)
                    {
                        // Change the reverse flag.
                        _inReverse = !_inReverse;
                        LoopCount++;

                        // Depending on the direction set the frame to be the appropriate one.
                        CurrentFrameIndex =
                            _inReverse ? EndingFrame - 1 : StartingFrame + 1;
                    }
                    else
                    {
                        // Modify the current frame depending on the direction we are going in.
                        if (_inReverse)
                            CurrentFrameIndex--;
                        else
                            CurrentFrameIndex++;
                    }

                    break;
                case AnimationLoopType.Reverse:
                    // If the global frame is the first frame.
                    if (CurrentFrameIndex == StartingFrame)
                    {
                        CurrentFrameIndex = EndingFrame;
                        LoopCount++;
                    }
                    else
                    {
                        CurrentFrameIndex--;
                    }

                    break;
                case AnimationLoopType.NoneReverse:
                    if (LoopCount > 0) return;
                    // If the global frame is the first frame.
                    if (CurrentFrameIndex == StartingFrame)
                        LoopCount++;
                    else
                        CurrentFrameIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AssureFrameInRange()
        {
            switch (LoopType)
            {
                case AnimationLoopType.None:
                case AnimationLoopType.Normal:
                case AnimationLoopType.NormalThenReverse when !_inReverse:
                    if (CurrentFrameIndex < StartingFrame) CurrentFrameIndex = StartingFrame;
                    if (CurrentFrameIndex > EndingFrame) CurrentFrameIndex = EndingFrame;
                    break;
                case AnimationLoopType.NoneReverse:
                case AnimationLoopType.Reverse:
                case AnimationLoopType.NormalThenReverse when _inReverse:
                    if (CurrentFrameIndex > StartingFrame) CurrentFrameIndex = StartingFrame;
                    if (CurrentFrameIndex < EndingFrame) CurrentFrameIndex = EndingFrame;
                    break;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the number of frames in the animation.
        /// </summary>
        /// <returns>The number of frames the animation range contains.</returns>
        private int GetTotalFrames()
        {
            switch (LoopType)
            {
                case AnimationLoopType.None:
                case AnimationLoopType.Normal:
                case AnimationLoopType.NormalThenReverse:
                    return EndingFrame - StartingFrame;
                case AnimationLoopType.Reverse:
                case AnimationLoopType.NoneReverse:
                    return StartingFrame - EndingFrame;
            }

            return -1;
        }

        /// <summary>
        /// Returns the starting frame for the current loop type.
        /// </summary>
        /// <returns>Returns the first frame for the current loop type.</returns>
        private int GetStartingFrame()
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