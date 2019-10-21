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
    /// A class for animating a texture using a lookup table of frames.
    /// </summary>
    public sealed class LookupAnimatedTexture : IAnimatedTexture
    {
        #region Properties

        /// <inheritdoc />
        public Texture Texture { get; private set; }

        /// <inheritdoc />
        public AnimationLoopType LoopType { get; set; }

        /// <inheritdoc />
        public int StartingFrame { get; set; }

        /// <inheritdoc />
        public int EndingFrame { get; set; }

        /// <inheritdoc />
        public int TimeBetweenFrames { get; set; }

        /// <summary>
        /// List of frames.
        /// </summary>
        public Rectangle[] Frames
        {
            get => _frames;
            set
            {
                _frames = value;
                Reset();
            }
        }

        #endregion

        #region Info Only

        /// <inheritdoc />
        public int LoopCount { get; private set; }

        /// <inheritdoc />
        public Rectangle CurrentFrame
        {
            get => GetFrameBounds(CurrentFrameIndex);
        }

        /// <inheritdoc />
        public int AnimationFrames
        {
            get => EndingFrame - StartingFrame;
        }

        /// <inheritdoc />
        public int TotalFrames
        {
            get => Frames?.Length ?? 0;
        }

        /// <inheritdoc />
        public int CurrentFrameIndex { get; private set; }

        #endregion

        private float _timePassed;
        private bool _inReverse;
        private Rectangle[] _frames;

        /// <summary>
        /// Create a new animated texture object.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frames">List of frame uvs..</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
        public LookupAnimatedTexture(Texture texture, Rectangle[] frames, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1)
        {
            Texture = texture;
            _frames = frames;

            LoopType = loopType;
            StartingFrame = startingFrame;
            EndingFrame = endingFrame;
            TimeBetweenFrames = timeBetweenFrames;

            Reset();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public LookupAnimatedTexture(AnimatedTexture copy)
        {
            CurrentFrameIndex = copy.CurrentFrameIndex;
            EndingFrame = copy.EndingFrame;
            LoopCount = copy.LoopCount;
            LoopType = copy.LoopType;
            StartingFrame = copy.StartingFrame;
            Texture = copy.Texture;
            TimeBetweenFrames = copy.TimeBetweenFrames;
        }

        /// <summary>
        /// Advance time for the animation.
        /// </summary>
        /// <param name="frameTime">The time passed since the last update.</param>
        public void Update(float frameTime)
        {
            if (Frames == null) return;

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
            ClampFrameParameters();
        }

        /// <summary>
        /// Set the current frame to the specified frame.
        /// Animation will continue from there.
        /// If the index is invalid it will either be set to the starting or ending frame.
        /// </summary>
        /// <param name="index">The index to set the current frame to.</param>
        public void SetFrame(int index)
        {
            CurrentFrameIndex = index;
            ClampFrameParameters();
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
        /// Get the bounds of the specified frame.
        /// </summary>
        /// <param name="frameId">The frame to get the bounds of.</param>
        /// <returns>The bounds of the requested frame.</returns>
        public Rectangle GetFrameBounds(int frameId)
        {
            return frameId > Frames.Length ? Rectangle.Empty : Frames[frameId];
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

        private LookupAnimatedTexture()
        {
            // no-op, used for copy
        }

        /// <inheritdoc />
        public IAnimatedTexture Copy()
        {
            return new LookupAnimatedTexture
            {
                CurrentFrameIndex = CurrentFrameIndex,
                EndingFrame = EndingFrame,
                Frames = Frames,
                LoopCount = LoopCount,
                LoopType = LoopType,
                StartingFrame = StartingFrame,
                Texture = Texture,
                TimeBetweenFrames = TimeBetweenFrames
            };
        }

        /// <inheritdoc />
        public AnimationDescriptionBase GetDescription(string textureName = null)
        {
            return new LookupAnimatedDescription
            {
                SpriteSheetName = textureName,
                StartingFrame = StartingFrame,
                EndingFrame = EndingFrame,
                TimeBetweenFrames = TimeBetweenFrames,
                LoopType = LoopType,
                Frames = Frames
            };
        }
    }
}