using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breath.Primitives;
using OpenTK;
using Soul.Engine.Enums;

namespace Soul.Engine.ECS.Components
{
    public class AnimationData : ComponentBase
    {

        #region Frame Calculation Properties

        /// <summary>
        /// The size of individual frames.
        /// </summary>
        public Vector2 FrameSize
        {
            get { return _frameSize; }
            set
            {
                _frameSize = value;

                // Recalculate frames.
                HasUpdated = true;
            }
        }

        private Vector2 _frameSize;

        /// <summary>
        /// The spacing between frames.
        /// </summary>
        public Vector2 Spacing
        {
            get { return _spacing; }
            set
            {
                _spacing = value;

                // Recalculate frames.
                HasUpdated = true;
            }
        }

        private Vector2 _spacing;

        #endregion

        #region Animation Properties

        /// <summary>
        /// The frame to start from. Zero indexed.
        /// </summary>
        public int StartingFrame
        {
            get { return _startingFrame; }
            set
            {
                // Redundancy check.
                if (_startingFrame == value) return;

                _startingFrame = value;

                ResetFrame();
            }
        }

        private int _startingFrame = 0;

        /// <summary>
        /// The frame to loop at. Zero indexed.
        /// </summary>
        public int EndingFrame
        {
            get { return _endingFrame == -1 ? TotalFrames : _endingFrame; }
            set
            {
                // Redundancy check.
                if (_endingFrame == value) return;

                _endingFrame = value;

                ResetFrame();
            }
        }

        private int _endingFrame = -1;

        /// <summary>
        /// The animation looping type.
        /// </summary>
        public AnimationLoopType LoopType { get; set; } = AnimationLoopType.Normal;

        /// <summary>
        /// The time in milliseconds between each frame.
        /// </summary>
        public int FrameTime { get; set; }

        /// <summary>
        /// Whether the animation is playing.
        /// </summary>
        public bool Playing = true;

        #endregion

        #region Informational

        /// <summary>
        /// The index of the frame being displayed.
        /// </summary>
        public int CurrentFrame { get; internal set; }

        /// <summary>
        /// Whether the animation has finished.
        /// </summary>
        public bool Finished { get; internal set; }

        /// <summary>
        /// The id of the frame being displayed from the total amount.
        /// </summary>
        public int CurrentFrameScoped
        {
            get { return StartingFrame + CurrentFrame; }
        }

        /// <summary>
        /// The rectangle representing the current frame within the texture sheet.
        /// </summary>
        public Rectangle CurrentFrameRect
        {
            get
            {
                // Get the current row and column.
                int row = (int)(CurrentFrame / (float)_columns);
                int column = CurrentFrame % _columns;

                // Generate texture rectangle from the current frame.
                return new Rectangle((int)(FrameSize.X * column + Spacing.X * (column + 1)),
                    (int)(FrameSize.Y * row + Spacing.Y * (row + 1)), (int)FrameSize.X, (int)FrameSize.Y);
            }
        }

        /// <summary>
        /// The total number of frames.
        /// </summary>
        public int TotalFrames
        {
            get { return _rows * _columns - 1; }
        }

        /// <summary>
        /// The number of frames within the animation.
        /// </summary>
        public int AnimationFrames
        {
            get
            {
                // Return the range based on the loop type.
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
        }


        #endregion

        #region Calculated

        /// <summary>
        /// The number of rows the texture sheet has, of frames of the specified size.
        /// </summary>
        private int _rows;
        /// <summary>
        /// The number of columns the texture sheet has, of frames of the specified size.
        /// </summary>
        private int _columns;

        /// <summary>
        /// Whether the animation is going in reverse.
        /// </summary>
        internal bool InReverse;

        /// <summary>
        /// The timer tracking frame changes.
        /// </summary>
        internal float Timer;

        #endregion

        /// <summary>
        /// Calculates rows and columns from the frame size, spritesheet size, and spacing.
        /// </summary>
        /// <param name="spriteSheetSize">The total size of the spritesheet.</param>
        internal void CalculateFrames(Vector2 spriteSheetSize)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_frameSize == null || _spacing == null) return;

            // Check if the frame size is invalid.
            if (_frameSize.X == 0 || _frameSize.Y == 0) return;

            // Calculate columns and rows.
            _columns = (int)(spriteSheetSize.X / _frameSize.X);
            _rows = (int)(spriteSheetSize.Y / _frameSize.Y);

            // Reset the current frame.
            ResetFrame();
        }


        /// <summary>
        /// Resets the current frame.
        /// </summary>
        public void ResetFrame()
        {
            // Set the current frame based on the loop type.
            switch (LoopType)
            {
                case AnimationLoopType.None:
                case AnimationLoopType.Normal:
                case AnimationLoopType.NormalThenReverse:
                    CurrentFrame = StartingFrame;
                    break;
                case AnimationLoopType.Reverse:
                case AnimationLoopType.NoneReverse:
                    CurrentFrame = EndingFrame;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Force an update. This is checked within the Update().
            Timer = -1;
        }


    }
}
