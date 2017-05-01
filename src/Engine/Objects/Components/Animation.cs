using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SoulEngine.Events;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An animation component. Requires an ActiveTexture to be drawn.
    /// </summary>
    public class Animation : Component
    {
        #region "Declarations"
        #region "Frame Animation Information"
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
        /// The index of the current frame within the range of the starting and ending frame.
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
        /// The index of the current frame within the total frames.
        /// </summary>
        public int FrameTotal
        {
            get
            {
                return _Frame;
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
        /// <summary>
        /// The space betweem frames.
        /// </summary>
        public Vector2 Spacing = Vector2.Zero;
        #endregion
        #region "Spritesheet Information"
        /// <summary>
        /// The sheet containing the frames.
        /// </summary>
        public Texture2D SpriteSheet;
        /// <summary>
        /// The frames that make up the animation cut from the spritesheet.
        /// </summary>
        public List<Texture2D> Frames
        {
            get
            {
                return frames;
            }
        }
        /// <summary>
        /// The width of each frame the spritesheet contains.
        /// </summary>
        public int FrameWidth;
        /// <summary>
        /// The height of each frame the spritesheet contains.
        /// </summary>
        public int FrameHeight;
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
        #region "Time Keeping"
        /// <summary>
        /// The time that has passed since the last frame switch.
        /// </summary>
        private float timePassed = 0;
        /// <summary>
        /// If animating is done.
        /// </summary>
        private bool finished = false;
        #endregion
        #region "Privates"
        private int _endingFrame = 0;
        private int _Frame = 0;
        private int FramesCount = 0;
        private int FramesCountTotal = 0;
        private int _FPS = 0;
        private List<Texture2D> frames = new List<Texture2D>();
        #endregion
        #region "Events"
        /// <summary>
        /// Triggered when the animation finishes.
        /// </summary>
        public static event EventHandler<EventArgs> OnFinish;
        #endregion
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override Component Initialize()
        {
            return this;
        }

        /// <summary>
        /// Initialize an animation component based on a texture to act as a spritesheet. Requires an ActiveTexture set to animation mode.
        /// </summary>
        /// <param name="SpriteSheet">The spritesheet that holds the animation frames.</param>
        /// <param name="FrameWidth">The width of each frame the spritesheet contains.</param>
        /// <param name="FrameHeight">The height of each frame the spritesheet contains.</param>
        /// <param name="StartingFrame">The frame to start from, or end on if in reverse, from all the frames in the spritesheet.</param>
        /// <param name="EndingFrame"> The frame to end at, or start on if in reverse, from all the frames in the spritesheet. </param>
        /// <param name="Loop">The type of loop.</param>
        /// <param name="FPS">The frames per second of the animation, essentially the speed.</param>
        /// <param name="Spacing">The spacing between frames in the picture.</param>
        public Component Initialize(Texture2D SpriteSheet, int FrameWidth, int FrameHeight, int StartingFrame = 0, int EndingFrame = -1, LoopType Loop = LoopType.Normal, int FPS = 10, Vector2 Spacing = new Vector2())
        {
            //Assign variables
            this.FrameWidth = FrameWidth;
            this.FrameHeight = FrameHeight;
            this.FPS = FPS;
            this.SpriteSheet = SpriteSheet;
            this.StartingFrame = StartingFrame;
            this.EndingFrame = EndingFrame;
            this.Loop = Loop;
            this.Spacing = Spacing;

            //TODO: Fix this, what's the point of standards if we work around them.
            Context.Core.__composeAllowed = true;
            SplitFrames();
            Context.Core.__composeAllowed = false;
            InitiateLoop();

            return this;
        }

        /// <summary>
        /// Initiates starting loop variables.
        /// </summary>
        public void InitiateLoop()
        {
            //Check if playing in reverse.
            if (Loop == LoopType.None || Loop == LoopType.Normal || Loop == LoopType.NormalThenReverse)
            {
                _Frame = StartingFrame;
            }
            else if (Loop == LoopType.Reverse || Loop == LoopType.NoneReverse)
            {
                _Frame = EndingFrame;
            }
        }

        /// <summary>
        /// Cuts the individual frames.
        /// </summary>
        public void SplitFrames()
        {
            //If the textures are cut, skip cutting.
            if (frames.Count != 0) return;

            //Calculate how many columns and rows the spritesheet has.
            int fColumns = SpriteSheet.Width / FrameWidth;
            int fRows = SpriteSheet.Height / FrameHeight;

            //Count frames.
            FramesCountTotal = fColumns * fRows;
            FramesCount = EndingFrame - StartingFrame;

            for (int i = 0; i < FramesCountTotal; i++)
            {
                //Calculate the location of the current sprite within the image.
                int Row = (int)(i / (float)fColumns);
                int Column = i % fColumns;

                //Determine which part of the spritesheet we want to cut out next.
                Rectangle FrameRect = new Rectangle(FrameWidth * Column + (int)(Spacing.X * (Column + 1)), FrameHeight * Row + (int)(Spacing.Y * (Row + 1)), FrameWidth, FrameHeight);

                //Switch over to the composer.
                RenderTarget2D TextureComposer = new RenderTarget2D(Context.Graphics, FrameWidth, FrameHeight);
                Context.ink.StartRenderTarget(ref TextureComposer, FrameRect.Width, FrameRect.Height, true);

                //Cut the part we need out.
                Context.ink.Draw(SpriteSheet, new Rectangle(0, 0, FrameWidth, FrameHeight), FrameRect, Color.White);

                //Stop drawing on the composer and offload the frame to the list.
                Context.ink.EndRenderTarget();

                TextureComposer.Name = SpriteSheet.Name + "_Frame" + i;
                frames.Add(TextureComposer as Texture2D);
            }
        }

        /// <summary>
        /// Runs the animation, should be called once every tick.
        /// </summary>
        public override void Update()
        {
            //Check if the animation is finished.
            if (finished) return;

            //Add the time passed to the inner variable and switch the frame if enough time has passed.
            timePassed += Context.Core.frameTime;
            if (timePassed > Delay)
            {
                timePassed -= Delay;
                NextFrame();
            }
        }

        /// <summary>
        /// Switches over to the next frame.
        /// </summary>
        private void NextFrame()
        {
            switch (Loop)
            {
                case LoopType.None:
                    //If the global frame is the last frame.
                    if (_Frame == EndingFrame)
                    {
                        OnFinish?.Invoke(this, null);
                        finished = true;
                    }
                    else
                    {
                        //Increment the frame.
                        _Frame++;
                    }
                    break;
                case LoopType.Normal:
                    //If the global frame is the last frame.
                    if (_Frame == EndingFrame)
                    {
                        //Loop to the starting frame.
                        _Frame = StartingFrame;
                    }
                    else
                    {
                        //Increment the frame.
                        _Frame++;
                    }
                    break;
                case LoopType.NormalThenReverse:
                    //If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if ((_Frame == EndingFrame && flagReverse == false) || (_Frame == StartingFrame && flagReverse == true))
                    {
                        //Change the reverse flag.
                        flagReverse = !flagReverse;

                        //Depending on the direction set the frame to be the appropriate one.
                        if (flagReverse)
                            _Frame = EndingFrame;
                        else
                            _Frame = StartingFrame;
                    }
                    else
                    {
                        //Modify the current frame depending on the direction we are going in.
                        if (flagReverse)
                            _Frame--;
                        else
                            _Frame++;
                    }
                    break;
                case LoopType.Reverse:
                    //If the global frame is the first frame.
                    if (_Frame == StartingFrame)
                    {
                        //Loop to the ending frame.
                        _Frame = EndingFrame;
                    }
                    else
                    {
                        //Otherwise decrement the frame, as we are going in reverse.
                        _Frame--;
                    }
                    break;
                case LoopType.NoneReverse:
                    //If the global frame is the first frame.
                    if (_Frame == StartingFrame)
                    {
                        OnFinish?.Invoke(this, null);
                        finished = true;
                    }
                    else
                    {
                        //Decrement the frame, as we are going in reverse.
                        _Frame--;
                    }
                    break;
            }
        }
    }
}
