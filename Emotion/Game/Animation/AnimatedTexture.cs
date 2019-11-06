#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// Handles spritesheets with equal sized frames.
    /// Name is non-specific for legacy reasons.
    /// </summary>
    public sealed class AnimatedTexture : AnimatedTextureBase
    {
        #region Properties

        /// <summary>
        /// The size of the individual frames.
        /// </summary>
        public Vector2 FrameSize
        {
            get => _frameSize;
            set
            {
                _frameSize = value;
                Reset();
            }
        }

        /// <summary>
        /// Spacing between individual frames.
        /// </summary>
        public Vector2 Spacing
        {
            get => _spacing;
            set
            {
                _spacing = value;
                Reset();
            }
        }

        #endregion

        /// <inheritdoc />
        public override int TotalFrames
        {
            get => (int) (Texture.Size.X / _frameSize.X * Texture.Size.Y / _frameSize.Y - 1);
        }

        private Vector2 _frameSize;
        private Vector2 _spacing;

        private bool _inReverse;

        /// <summary>
        /// Create a new animated texture object. Which will animate from the first frame to the last.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="columns">The number of columns of frames.</param>
        /// <param name="rows">The number of rows of frames.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        public AnimatedTexture(Texture texture, int columns, int rows, AnimationLoopType loopType, int timeBetweenFrames)
            : this(texture, new Vector2(texture.Size.X / columns, texture.Size.Y / rows), loopType, timeBetweenFrames)
        {
        }

        /// <summary>
        /// Create a new animated texture object. Which will animate from the first frame to the last.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="columns">The number of columns of frames.</param>
        /// <param name="rows">The number of rows of frames.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
        public AnimatedTexture(Texture texture, int columns, int rows, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1)
            : this(texture, new Vector2(texture.Size.X / columns, texture.Size.Y / rows), loopType, timeBetweenFrames, startingFrame, endingFrame)
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
        public AnimatedTexture(Texture texture, Vector2 frameSize, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1) : this(texture, frameSize,
            Vector2.Zero,
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
        public AnimatedTexture(Texture texture, Vector2 frameSize, Vector2 spacing, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1)
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

        /// <inheritdoc />
        public override void Update(float frameTime)
        {
            if (_frameSize == Vector2.Zero) return;

            // Clamp frame parameters.
            ClampFrameParameters();
            ClampCurrentFrame();

            _timePassed += frameTime;

            // Timer.
            if (!(_timePassed >= TimeBetweenFrames)) return;
            _timePassed -= TimeBetweenFrames;
            NextFrame();
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

        /// <inheritdoc />
        public override Rectangle GetFrameBounds(int frameId)
        {
            return GetFrameBounds(Texture.Size, _frameSize, _spacing, frameId);
        }

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
            var columns = (int) (textureSize.X / frameSize.X);

            // If invalid number of columns this means the texture size is larger than the frame size.
            if (columns == 0)
            {
                Engine.Log.Trace($"Invalid frame size of [{frameSize}] for image of size [{textureSize}].", MessageSource.Other);
                return new Rectangle(Vector2.Zero, textureSize);
            }

            // Get the current row and column.
            var row = (int) (frameId / (float) columns);
            int column = frameId % columns;

            // Find the frame we are looking for.
            return new Rectangle((int) (frameSize.X * column + spacing.X * (column + 1)),
                (int) (frameSize.Y * row + spacing.Y * (row + 1)), (int) frameSize.X, (int) frameSize.Y);
        }

        private AnimatedTexture()
        {
            // no-op, used for copy
        }

        /// <inheritdoc />
        public override AnimatedTextureBase Copy()
        {
            return new AnimatedTexture
            {
                CurrentFrameIndex = CurrentFrameIndex,
                EndingFrame = EndingFrame,
                _frameSize = FrameSize,
                _spacing = Spacing,
                LoopCount = LoopCount,
                LoopType = LoopType,
                StartingFrame = StartingFrame,
                Texture = Texture,
                TimeBetweenFrames = TimeBetweenFrames
            };
        }

        /// <inheritdoc />
        public override AnimationDescriptionBase GetDescription(string textureName = null)
        {
            return new AnimatedTextureDescription
            {
                SpriteSheetName = textureName,
                StartingFrame = StartingFrame,
                EndingFrame = EndingFrame,
                TimeBetweenFrames = TimeBetweenFrames,
                LoopType = LoopType,
                FrameSize = FrameSize,
                Spacing = Spacing
            };
        }
    }
}