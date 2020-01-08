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
    /// An interface handling animated textures/sprites.
    /// </summary>
    public class AnimatedTexture
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

        #region Info Only

        /// <summary>
        /// The index of the current frame within the total frame count. Zero indexed.
        /// </summary>
        public int CurrentFrameIndex { get; protected set; }

        /// <summary>
        /// The total number of frames in the spritesheet. Zero indexed. Inclusive.
        /// </summary>
        public int TotalFrames
        {
            get => Frames?.Length - 1 ?? 0;
        }

        /// <summary>
        /// The texture corresponding to the current frame.
        /// </summary>
        public Rectangle CurrentFrame
        {
            get => GetFrameBounds(CurrentFrameIndex);
        }

        /// <summary>
        /// The progress towards the next frame. 0-1
        /// </summary>
        public float Progress
        {
            get => MathF.Min(1, _timePassed / TimeBetweenFrames);
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

        protected bool _inReverse;
        protected Rectangle[] _frames = new Rectangle[0];
        protected Vector2[] _anchors = new Vector2[0];
        protected float _timePassed;

        /// <summary>
        /// Create a new animated texture object.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frames">List of frame uvs..</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
        public AnimatedTexture(Texture texture, Rectangle[] frames, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1)
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
        /// Create a new animated texture object whose frames are a grid inside the texture. Which will animate from the first
        /// frame to the last.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="columns">The number of columns of frames.</param>
        /// <param name="rows">The number of rows of frames.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
        public AnimatedTexture(Texture texture, int columns, int rows, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1)
        {
            Texture = texture;
            var frames = new Rectangle[columns * rows];
            var w = (int) texture.Size.X;
            var h = (int) texture.Size.Y;

            var i = 0;
            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < columns; x++)
                {
                    frames[i].X = w / rows * x;
                    frames[i].Y = h / columns * y;
                    frames[i].Width = w / rows;
                    frames[i].Height = h / columns;
                    i++;
                }
            }

            _frames = frames;

            LoopType = loopType;
            StartingFrame = startingFrame;
            EndingFrame = endingFrame;
            TimeBetweenFrames = timeBetweenFrames;

            Reset();
        }

        /// <summary>
        /// Create a new animated texture object whose frames are a grid inside the texture.
        /// </summary>
        /// <param name="texture">The spritesheet texture.</param>
        /// <param name="frameSize">The size of frames within the texture. It is assumed that all frames are of the same size.</param>
        /// <param name="loopType">The type of loop to apply to the animation.</param>
        /// <param name="timeBetweenFrames">The time between frames in milliseconds.</param>
        /// <param name="startingFrame">The frame index to start from. Inclusive. Zero indexed.</param>
        /// <param name="endingFrame">The frame index to end on from the total frame count. Zero indexed. Inclusive.</param>
        public AnimatedTexture(Texture texture, Vector2 frameSize, AnimationLoopType loopType, int timeBetweenFrames, int startingFrame = 0, int endingFrame = -1) : this(texture, frameSize,
            Vector2.Zero, loopType, timeBetweenFrames, startingFrame, endingFrame)
        {
        }

        /// <summary>
        /// Create a new animated texture object whose frames are a grid inside the texture.
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

            Vector2 frameC = texture.Size / (frameSize + spacing);
            var frameCount = (int) (frameC.X * frameC.Y);
            var frames = new Rectangle[frameCount];
            for (var i = 0; i < frames.Length; i++)
            {
                frames[i] = GetGridFrameBounds(texture.Size, frameSize, spacing, i);
            }

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
        public AnimatedTexture(AnimatedTexture copy)
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
            ClampCurrentFrame();

            _timePassed += frameTime;

            // Timer.
            if (!(_timePassed >= TimeBetweenFrames)) return;
            _timePassed -= TimeBetweenFrames;

            // Set frame to next frame.
            CurrentFrameIndex = NextFrame(out bool looped, out bool reversed);
            if (looped) LoopCount++;
            _inReverse = reversed;
        }

        /// <summary>
        /// Iterate to the next frame.
        /// </summary>
        public virtual int NextFrame(out bool looped, out bool reversed)
        {
            looped = false;
            reversed = false;

            switch (LoopType)
            {
                case AnimationLoopType.None:
                    if (LoopCount > 0) return EndingFrame;
                    // If the global frame is the last frame.
                    if (CurrentFrameIndex == EndingFrame)
                    {
                        looped = true;
                        return EndingFrame;
                    }
                    else
                    {
                        return CurrentFrameIndex + 1;
                    }

                case AnimationLoopType.Normal:
                    // If the global frame is the last frame.
                    if (CurrentFrameIndex == EndingFrame)
                    {
                        looped = true;
                        return StartingFrame;
                    }
                    else
                    {
                        return CurrentFrameIndex + 1;
                    }
                case AnimationLoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if (CurrentFrameIndex == EndingFrame && !_inReverse ||
                        CurrentFrameIndex == StartingFrame && _inReverse)
                    {
                        // Change the reverse flag.
                        reversed = !_inReverse;
                        looped = true;

                        // Depending on the direction set the frame to be the appropriate one.
                        return reversed ? EndingFrame - 1 : StartingFrame + 1;
                    }
                    else
                    {
                        // Modify the current frame depending on the direction we are going in.
                        if (_inReverse)
                            return CurrentFrameIndex - 1;
                        return CurrentFrameIndex + 1;
                    }
                case AnimationLoopType.Reverse:
                    // If the global frame is the first frame.
                    if (CurrentFrameIndex == StartingFrame)
                    {
                        looped = true;
                        return EndingFrame;
                    }
                    else
                    {
                        return CurrentFrameIndex - 1;
                    }
                case AnimationLoopType.NoneReverse:
                    if (LoopCount > 0) return StartingFrame;
                    // If the global frame is the first frame.
                    if (CurrentFrameIndex == StartingFrame)
                    {
                        looped = true;
                        return StartingFrame;
                    }
                    else
                    {
                        return CurrentFrameIndex - 1;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Reset the animation.
        /// </summary>
        public virtual void Reset()
        {
            if (Frames != null && _anchors.Length != Frames.Length) Array.Resize(ref _anchors, Frames.Length);
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
        public Rectangle GetFrameBounds(int frameId)
        {
            return Frames == null || frameId > Frames.Length ? Rectangle.Empty : Frames[frameId];
        }

        /// <summary>
        /// Returns the bounds of a frame within a spritesheet texture.
        /// </summary>
        /// <param name="textureSize">The size of the spritesheet texture.</param>
        /// <param name="frameSize">The size of individual frames.</param>
        /// <param name="spacing">The spacing between frames.</param>
        /// <param name="frameId">The index of the frame we are looking for. 0 based.</param>
        /// <returns>The bounds of a frame within a spritesheet texture.</returns>
        public static Rectangle GetGridFrameBounds(Vector2 textureSize, Vector2 frameSize, Vector2 spacing, int frameId)
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

        /// <summary>
        /// Copy the animation data.
        /// </summary>
        /// <returns>A copy of this animated texture.</returns>
        public virtual AnimatedTexture Copy()
        {
            return new AnimatedTexture
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

        /// <summary>
        /// Returns a serializable animation description file.
        /// </summary>
        /// <param name="textureName">The spritesheet texture's name within the asset loader.</param>
        /// <returns>A serializable animation description file.</returns>
        public virtual AnimatedTextureDescription GetDescription(string textureName = null)
        {
            return new AnimatedTextureDescription
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