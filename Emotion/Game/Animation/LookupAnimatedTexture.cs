#region Using

using System;
using System.Numerics;
using Emotion.Graphics.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// A class for animating a texture using a lookup table of frames.
    /// </summary>
    public sealed class LookupAnimatedTexture : AnimatedTextureBase
    {
        #region Properties

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

        /// <summary>
        /// List of anchor points per frame.
        /// </summary>
        public Vector2[] Anchors
        {
            get => _anchors;
            set
            {
                _anchors = value;
                Reset();
            }
        }

        #endregion

        /// <inheritdoc />
        public override int TotalFrames
        {
            get => Frames?.Length - 1 ?? 0;
        }

        private bool _inReverse;
        private Rectangle[] _frames = new Rectangle[0];
        private Vector2[] _anchors = new Vector2[0];

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
        public LookupAnimatedTexture(LookupAnimatedTexture copy)
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
        public override void Update(float frameTime)
        {
            if (Frames == null) return;

            // Clamp frame parameters.
            ClampFrameParameters();
            ClampCurrentFrame();

            _timePassed += frameTime;

            // Timer.
            if (!(_timePassed >= TimeBetweenFrames)) return;
            _timePassed -= TimeBetweenFrames;
            NextFrame();
        }

        /// <inheritdoc />
        public override void Reset()
        {
            if (Frames != null && _anchors.Length != Frames.Length) Array.Resize(ref _anchors, Frames.Length);
            base.Reset();
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

        /// <summary>
        /// Get the bounds of the specified frame.
        /// </summary>
        /// <param name="frameId">The frame to get the bounds of.</param>
        /// <returns>The bounds of the requested frame.</returns>
        public override Rectangle GetFrameBounds(int frameId)
        {
            return Frames == null || frameId > Frames.Length ? Rectangle.Empty : Frames[frameId];
        }

        private LookupAnimatedTexture()
        {
            // no-op, used for copy
        }

        /// <inheritdoc />
        public override AnimatedTextureBase Copy()
        {
            return new LookupAnimatedTexture
            {
                CurrentFrameIndex = CurrentFrameIndex,
                EndingFrame = EndingFrame,
                Frames = Frames,
                Anchors = Anchors,
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
            return new LookupAnimatedDescription
            {
                SpriteSheetName = textureName,
                StartingFrame = StartingFrame,
                EndingFrame = EndingFrame,
                TimeBetweenFrames = TimeBetweenFrames,
                LoopType = LoopType,
                Frames = Frames,
                Anchors = Anchors
            };
        }
    }
}