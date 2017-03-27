using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;

namespace SoulEngine.Objects.Components.Helpers
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Data about animating a texture.
    /// </summary>
    public class AnimationData
    {
        #region "Declarations"
        #region "Frame Information"
        /// <summary>
        /// The frame to start from, or end on if in reverse, from all the frames in the spritesheet.
        /// </summary>
        public int StartingFrame = 0;
        /// <summary>
        /// The frame to end at, or start on if in reverse, from all the frames in the spritesheet. 
        /// Internal Accessor.
        /// </summary>
        public int EndingFrame
        {
            get
            {
                if (_endingFrame == -1)
                {
                    return FramesCountTotal - 1;
                }

                return _endingFrame;
            }
            set
            {
                _endingFrame = value;
            }
        }
        /// <summary>
        /// The index of the current frame.
        /// </summary>
        public int Frame
        {
            get
            {
                return _Frame - StartingFrame;
            }
            set
            {
                _Frame = StartingFrame + value;
            }
        }
        /// <summary>
        /// The total number of frames for the animation.
        /// </summary>
        public int FramesTotal
        {
            get
            {
                return FramesCount;
            }
        }
        /// <summary>
        /// The frames per second of the animation, essentially the speed.
        /// </summary>
        public int FPS
        {
            get
            {
                return _FPS;
            }
            set
            {
                //The delay (since it's in miliseconds) is equal to the fps divided by a second, or 1000 miliseconds.
                delay = 1000f / (float)value;
                _FPS = value;
            }
        }
        /// <summary>
        /// The calculated time between frames.
        /// </summary>
        public float Delay
        {
            get
            {
                return delay;
            }
        }
        private float delay = 1;
        #endregion
        #region "Private Frame Information"
        /// <summary>
        /// The frame to end at, or start on if in reverse, from all the frames in the spritesheet. 
        /// </summary>
        private int _endingFrame = 0;
        /// <summary>
        /// The current frame, from the total frame count.
        /// </summary>
        private int _Frame = 0;
        /// <summary>
        /// The current frame, from the range of frames defined by the first and last frame.
        /// </summary>
        private int FramesCount = 0;
        /// <summary>
        /// The total amount of frames on the spritesheet.
        /// </summary>
        private int FramesCountTotal = 0;
        /// <summary>
        /// The frames per second of the animation. Private holder.
        /// </summary>
        private int _FPS = 0;
        #endregion
        #region "Spritesheet Information"
        /// <summary>
        /// The width of each frame the spritesheet contains.
        /// </summary>
        public int FrameWidth;
        /// <summary>
        /// The height of each frame the spritesheet contains.
        /// </summary>
        public int FrameHeight;
        /// <summary>
        /// The rows of frames the spritesheet contains.
        /// This is calculated.
        /// </summary>
        private int Rows;
        /// <summary>
        /// The columns of frames the spritesheet contains.
        /// This is calculated.
        private int Columns;
        #endregion
        #region "Loop"
        /// <summary>
        /// The type of loop.
        /// </summary>
        public LoopType Loop;
        /// <summary>
        /// Used for NormalThenReverse, is true once reverse.
        /// </summary>
        private bool flagReverse = false;
        #endregion
        #endregion



    }
}
