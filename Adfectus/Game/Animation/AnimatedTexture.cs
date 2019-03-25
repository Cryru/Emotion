#region Using

using System;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Logging;
using Adfectus.Primitives;

#endregion

namespace Adfectus.Game.Animation
{
    /// <summary>
    /// A class for animating a texture.
    /// </summary>
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
        /// The frame index to end on from the total frame count. Zero indexed. Inclusive.
        /// </summary>
        public int EndingFrame { get; set; }

        /// <summary>
        /// The time between frames in milliseconds.
        /// </summary>
        public float TimeBetweenFrames { get; set; }

        /// <summary>
        /// The index of the current frame within the total frame count. Zero indexed.
        /// </summary>
        public int CurrentFrameIndex { get; private set; }

        /// <summary>
        /// The texture corresponding to the current frame.
        /// </summary>
        public Rectangle CurrentFrame
        {
            get => GetFrameBounds(Texture.Size, _frameSize, _spacing, CurrentFrameIndex);
        }

        /// <summary>
        /// The number of frames the configured animation has in total. Zero indexed.
        /// </summary>
        public int AnimationFrames
        {
            get => EndingFrame - StartingFrame;
        }

        /// <summary>
        /// The total number of frames in the spritesheet. Zero indexed. Inclusive.
        /// </summary>
        public int TotalFrames
        {
            get => (int) (Texture.Size.X / _frameSize.X * Texture.Size.Y / _frameSize.Y - 1);
        }

        #endregion

        private Vector2 _frameSize;
        private Vector2 _spacing;

        private float _timePassed;
        private bool _inReverse;

        /// <summary>
        /// Create a new animated texture object. Which will animate from the first frame to the last.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frameSize">The size of frames within the texture. It is assumed that all frames are of the same size.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        public AnimatedTexture(Texture texture, Vector2 frameSize, AnimationLoopType loopType, int timeBetweenFrames)
            : this(texture, frameSize, loopType, timeBetweenFrames, 0, (int) (texture.Size.X / frameSize.X * texture.Size.Y / frameSize.Y - 1))
        {
        }

        /// <summary>
        /// Create a new animated texture object.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frameSize">The size of frames within the texture. It is assumed that all frames are of the same size.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
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
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
        public AnimatedTexture(Texture texture, Vector2 frameSize, Vector2 spacing, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame, int endingFrame)
        {
            Texture = texture;
            _frameSize = frameSize;
            _spacing = spacing;

            LoopType = loopType;
            StartingFrame = startingFrame;
            EndingFrame = endingFrame;
            TimeBetweenFrames = timeBetweenFrames;

            ClampFrameParameters();
            Reset();
        }

        /// <summary>
        /// Advance time for the animation.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update.</param>
        public void Update(float frameTime)
        {
            // Clamp frame parameters.
            ClampFrameParameters();

            // Clamp frame within range.
            if (CurrentFrameIndex < StartingFrame) CurrentFrameIndex = StartingFrame;
            if (CurrentFrameIndex > EndingFrame) CurrentFrameIndex = EndingFrame;

            _timePassed += frameTime;

            // Timer.
            if (!(_timePassed >= TimeBetweenFrames)) return;
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
                    if (CurrentFrameIndex == EndingFrame && !_inReverse ||
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

        #endregion

        #region Helpers

        /// <summary>
        /// Clamp the starting and ending frames.
        /// </summary>
        private void ClampFrameParameters()
        {
            if (StartingFrame < 0)
            {
                StartingFrame = 0;
                CurrentFrameIndex = GetStartingFrame();
            }

            if (EndingFrame > TotalFrames) EndingFrame = TotalFrames;
            if (StartingFrame > EndingFrame)
            {
                StartingFrame = 0;
                CurrentFrameIndex = GetStartingFrame();
            }

            if (EndingFrame < StartingFrame) EndingFrame = TotalFrames;
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

        /// <summary>
        /// Returns the bounds of a frame within a spritesheet texture.
        /// </summary>
        /// <param name="textureSize">The size of the spritesheet texture.</param>
        /// <param name="frameSize">The size of individual frames.</param>
        /// <param name="spacing">The spacing between frames.</param>
        /// <param name="frameId">The index of the frame we are looking for. 0 based.</param>
        /// <returns>The bounds of a frame within a spritesheet texture.</returns>
        public static Rectangle GetFrameBounds(Vector2 textureSize, Vector2 frameSize, Vector2 spacing, int frameId)
        {
            // Get the total number of columns.
            int columns = (int) (textureSize.X / frameSize.X);

            // If invalid number of columns this means the texture size is larger than the frame size.
            if (columns == 0)
            {
                Engine.Log.Warning($"Invalid frame size of [{frameSize}] for image of size [{textureSize}].", MessageSource.Game);
                return new Rectangle(Vector2.Zero, textureSize);
            }

            // Get the current row and column.
            int row = (int) (frameId / (float) columns);
            int column = frameId % columns;

            // Find the frame we are looking for.
            return new Rectangle((int) (frameSize.X * column + spacing.X * (column + 1)),
                (int) (frameSize.Y * row + spacing.Y * (row + 1)), (int) frameSize.X, (int) frameSize.Y);
        }
    }
}