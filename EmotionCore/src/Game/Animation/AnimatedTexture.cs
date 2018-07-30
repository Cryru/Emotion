// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.Utils;

#endregion

namespace Emotion.Game.Animation
{
    public class AnimatedTexture
    {
        #region Properties

        public Texture Target { get; private set; }

        public int LoopCount { get; private set; }
        public AnimationLoopType LoopType { get; set; }
        public int StartingFrame { get; set; }
        public int EndingFrame { get; set; }

        public float TimeBetweenFrames { get; set; }

        public int CurrentFrame { get; private set; }

        public int AnimationFrames
        {
            get => GetTotalFrames();
        }

        #endregion

        private Vector2 _frameSize;
        private Vector2 _spacing;

        private float _timePassed;
        private bool _inReverse;

        public AnimatedTexture(Texture target, Vector2 frameSize, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame, int endingFrame) : this(target, frameSize, loopType,
            startingFrame, timeBetweenFrames, endingFrame, Vector2.Zero)
        {
        }

        public AnimatedTexture(Texture target, Vector2 frameSize, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame, int endingFrame, Vector2 spacing)
        {
            Target = target;
            _frameSize = frameSize;
            _spacing = spacing;

            LoopType = loopType;
            StartingFrame = startingFrame;
            EndingFrame = endingFrame;
            TimeBetweenFrames = timeBetweenFrames;

            Reset();
        }

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

        public Rectangle GetCurrentFrame()
        {
            return Helpers.GetFrameBounds(Target.Size, _frameSize, _spacing, CurrentFrame);
        }

        public void Reset()
        {
            _timePassed = 0;
            CurrentFrame = GetStartingFrame();
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
                    if (CurrentFrame == EndingFrame)
                        LoopCount++;
                    else
                        CurrentFrame++;
                    break;
                case AnimationLoopType.Normal:
                    // If the global frame is the last frame.
                    if (CurrentFrame == EndingFrame)
                    {
                        CurrentFrame = StartingFrame;
                        LoopCount++;
                    }
                    else
                    {
                        CurrentFrame++;
                    }

                    break;
                case AnimationLoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if (CurrentFrame == EndingFrame && _inReverse == false ||
                        CurrentFrame == StartingFrame && _inReverse)
                    {
                        // Change the reverse flag.
                        _inReverse = !_inReverse;
                        LoopCount++;

                        // Depending on the direction set the frame to be the appropriate one.
                        CurrentFrame =
                            _inReverse ? EndingFrame - 1 : StartingFrame + 1;
                    }
                    else
                    {
                        // Modify the current frame depending on the direction we are going in.
                        if (_inReverse)
                            CurrentFrame--;
                        else
                            CurrentFrame++;
                    }

                    break;
                case AnimationLoopType.Reverse:
                    // If the global frame is the first frame.
                    if (CurrentFrame == StartingFrame)
                    {
                        CurrentFrame = EndingFrame;
                        LoopCount++;
                    }
                    else
                    {
                        CurrentFrame--;
                    }

                    break;
                case AnimationLoopType.NoneReverse:
                    if (LoopCount > 0) return;
                    // If the global frame is the first frame.
                    if (CurrentFrame == StartingFrame)
                        LoopCount++;
                    else
                        CurrentFrame--;
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
                    if (CurrentFrame < StartingFrame) CurrentFrame = StartingFrame;
                    if (CurrentFrame > EndingFrame) CurrentFrame = EndingFrame;
                    break;
                case AnimationLoopType.NoneReverse:
                case AnimationLoopType.Reverse:
                case AnimationLoopType.NormalThenReverse when _inReverse:
                    if (CurrentFrame > StartingFrame) CurrentFrame = StartingFrame;
                    if (CurrentFrame < EndingFrame) CurrentFrame = EndingFrame;
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