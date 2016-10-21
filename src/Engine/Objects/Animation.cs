using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects.Internal;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // Used for animating textures from a spritesheet.                          //
    // Can also be used to split a spritesheet into it's individual frames.     //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Animation : Timer
    {
    #region "Declarations"
        //Textures
        /// <summary>
        /// A list of all cut frames. This includes even the ones outside of the starting-ending frame scope.
        /// </summary>
        public List<Texture2D> Frames
        {
            get
            {
                return frames;
            }
        }
        /// <summary>
        /// A list of all frames that are part of the animation. This scope is defined by the starting and ending frame.
        /// </summary>
        public List<Texture2D> IncludedFrames
        {
            get
            {
                return frames.GetRange(startingFrame, endingFrame - 1);
            }
        }
        /// <summary>
        /// Returns the current frame's texture.
        /// </summary>
        public Texture2D FrameTexture
        {
            get
            {
                //Check if the frame we are trying to access is beyond the cut frames.
                if (_Frame > frames.Count)
                {
                    //In which case we return a missing image.
                    return Core.missingTexture.Image;
                }

                return frames[_Frame];
            }
        }
        /// <summary>
        /// The name of the spritesheet texture.
        /// </summary>
        public string SheetName = "";

        //Settings and Loop
        public LoopType Loop; //The type of loop to use.
        /// <summary>
        /// The type of animation loop.
        /// </summary>
        public enum LoopType 
        {
            /// <summary>
            /// Disabled looping, animation will play once.
            /// </summary>
            None,
            /// <summary>
            /// Animation will loop normally, after the last frame is the first frame.
            /// </summary>
            Normal,
            /// <summary>
            /// The animation will play in reverse after reaching then last frame.
            /// </summary>
            NormalThenReverse,
            /// <summary>
            /// The animation will play in reverse.
            /// </summary>
            Reverse,
            /// <summary>
            /// Disabled looping, animation will play once, in reverse.
            /// </summary>
            NoneReverse
        }
        private bool flagReverse = false; //Used for NormalThenReverse, is true once reverse.

        //Events
        public Event<Animation> onFrameChange = new Event<Animation>(); //The event for when the frame changes.
        public Event<Animation> onFinished = new Event<Animation>(); //The event for when the animation has finished.
        public Event<Animation> onLoop = new Event<Animation>(); //The event for when the animation loops back.

        //Public Accessors
        public int Frame //The current frame's number, counting the first frame as zero.
        {
            get
            {
                return _Frame - startingFrame;
            }
            set
            {
                _Frame = startingFrame + value;
            }
        }
        public int FramesTotal //The total number of frames for the animation.
        {
            get
            {
                return FramesCount;
            }
        }
        public int FPS //The frames per second of the animation, essentially the speed.
        {
            get
            {
                return _FPS;
            }
            set
            {
                //The delay (since it's in miliseconds) is equal to the fps divided by a second, or 1000 miliseconds.
                Delay = 1000f / (float)value; 
                _FPS = value;
            }
        }

        //Private Workings
        private int startingFrame = 0; //The frame to start from, from all the frames in the spritesheet.
        private int endingFrame //The frame to end at, from all the frames in the spritesheet. This is the accessor that will return the last frame when the ending frame is -1.
        {
            get
            {
                if(_endingFrame == -1)
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
        private int _endingFrame = 0; //The frame to end at, from all the frames in the spritesheet.
        private int _Frame = 0; //The current frame's number, from the total frame count.
        private int FramesCount = 0; //The total number of frames in the animation. This is calculated.
        private int FramesCountTotal = 0; //The total number of frames on the spritesheet.

        //Timing
        private int _FPS = 0; //The frames per second of the animation. This is inputed.

        //Atlas Variables
        private int fWidth; //The width of each individual frame. This is inputed.
        private int fHeight; //The height of each individual frame. This is inputed.
        private int fRows; //The number of rows of frames the spritesheet contains. This is calculated.
        private int fColumns; //The number of columns of frames the spritesheet contains. This is calculated.
        private List<Texture2D> frames = new List<Texture2D>(); //All the frames from the atlas cut.
        #endregion

        /// <summary>
        /// An animation object that automatically cuts a spritesheet into it's individual frames and animates them.
        /// </summary>
        /// <param name="SpriteSheet">The spritesheet that holds the animation frames.</param>
        /// <param name="frameWidth">The width of each individual frame.</param>
        /// <param name="frameHeight">The height of each individual frame.</param>
        /// <param name="startingFrame">The first frame to animate from.</param>
        /// <param name="endingFrame">The frame to end the animation at. Set to -1 to play until the last frame.</param>
        /// <param name="AnimationLoop">The type of animation, Reverse, NormalThenReverse etc.</param>
        /// <param name="FPS">The frames per second, AKA speed at which frames should be animated.</param>
        public Animation(Texture2D SpriteSheet, int frameWidth, int frameHeight, int startingFrame = 0, int endingFrame = -1, LoopType AnimationLoop = LoopType.Normal, int FPS = 10) : base()
        {
            //Assign variables
            fWidth = frameWidth;
            fHeight = frameHeight;
            this.FPS = FPS;
            SheetName = SpriteSheet.Name;

            //Setup the timer tick call.
            onTick.Add(Tick);

            //Split frames and assign variables.
            SplitSheet(SpriteSheet);
            
            //Setup the starting and ending frames.
            this.startingFrame = startingFrame;
            this.endingFrame = endingFrame;
            _Frame = startingFrame - 1; //We remove one because we are going to load a single tick to get the first frame loaded.

            //Set loop variables.
            Loop = AnimationLoop;
            InitLoop();

            //We run one frame here.
            //TimerTick(); 

        }
        #region "Initialization Functions"
        /// <summary>
        /// Initializes the loop type variables.
        /// </summary>
        private void InitLoop()
        {
            //Check if playing in reverse.
            if (Loop == LoopType.None)
            {
                Limit = -1;
                _Frame = startingFrame;
            }
            else if (Loop == LoopType.Normal || Loop == LoopType.NormalThenReverse)
            {
                Limit = -1;
                _Frame = startingFrame;
            }
            else if (Loop == LoopType.Reverse)
            {
                Limit = -1;
                _Frame = endingFrame;
            }
            else if(Loop == LoopType.NoneReverse)
            {
                Limit = -1;
                _Frame = endingFrame;  
            }

           // _State = TimerState.Paused;
        }
        /// <summary>
        /// Cuts the spritesheet into it's individual frames and stores them in the frames variable.
        /// Also sets all the frame variables based on the provided sheet.
        /// </summary>
        public void SplitSheet(Texture2D spriteSheet)
        {
            //Clear the frames list, just in case.
            frames.Clear();

            //Calculate columns and rows.
            fColumns = spriteSheet.Width / fWidth;
            fRows = spriteSheet.Height / fHeight;

            //Count frames.
            FramesCountTotal = fColumns * fRows;
            FramesCount = endingFrame - startingFrame;

            for (int i = 0; i < FramesCountTotal; i++)
            {
                //Calculate the location of the current sprite within the image.
                int Row = (int)(i / (float)fColumns);
                int Column = i % fColumns;

                //Transform into a rectangle.
                Rectangle FrameRect = new Rectangle(fWidth * Column, fHeight * Row, fWidth, fHeight);

                //Create a new texture object with the dimensions of the sprite.
                Texture2D curFrame = new Texture2D(Core.graphics.GraphicsDevice, FrameRect.Width, FrameRect.Height);

                //Define a render target.
                RenderTarget2D tempTarget = new RenderTarget2D(Core.graphics.GraphicsDevice, FrameRect.Width, FrameRect.Height);

                //Set the graphics device to the render target and clear it.
                Core.graphics.GraphicsDevice.SetRenderTarget(tempTarget);
                Core.graphics.GraphicsDevice.Clear(Color.Transparent);

                //Draw the part of the texture we need onto the target.
                Core.ink.Begin();
                Core.ink.Draw(spriteSheet, new Rectangle(0, 0, fWidth, fHeight), FrameRect, Color.White);
                Core.ink.End();

                //Assign the target to a texture.
                curFrame = tempTarget;

                //Return to the default render target.
                Core.graphics.GraphicsDevice.SetRenderTarget(null);

                //Add the frame to the list of frames.
                frames.Add(curFrame);
            }
        }
        #endregion

        //Is run on timer tick.
        private void Tick()
        {
            switch(Loop)
            {
                case LoopType.None:
                    if(_Frame == endingFrame) //If the global frame is the last frame.
                    {
                        _State = TimerState.WaitingForEvent; //Set the event to waiting for an event.
                        onFinished.Trigger(this); //Invoke the finished event.
                    }
                    else
                    {
                        _Frame++; //Increment the frame.
                        onFrameChange.Trigger(this); //Invoke the frame change event.
                    }
                    break;
                case LoopType.Normal:
                    if (_Frame == endingFrame) //If the global frame is the last frame.
                    {
                        _Frame = startingFrame; //Set the frame to the starting frame.
                        onLoop.Trigger(this); //Invoke the loop event.
                    }
                    else
                    {
                        _Frame++; //Increment the frame.
                        onFrameChange.Trigger(this); //Invoke the frame change event.
                    }
                    break;
                case LoopType.NormalThenReverse:
                    if ((_Frame == endingFrame && flagReverse == false) || (_Frame == startingFrame && flagReverse == true)) //If the global frame is the last frame.
                    {
                        flagReverse = !flagReverse; //Change the reverse flag.

                        if (flagReverse)
                            _Frame = endingFrame; //Set the frame to the last frame. It should be already, but w/e.
                        else
                            _Frame = startingFrame; //Set the frame to the start frame. It should be already, but w/e.

                        onLoop.Trigger(this); //Invoke the loop event.
                    }
                    else
                    {
                        if (flagReverse)
                            _Frame--; //Decrement the frame.
                        else
                            _Frame++; //Increment the frame.

                        onFrameChange.Trigger(this); //Invoke the frame change event.
                    }
                    break;
                case LoopType.Reverse:
                    if (_Frame == startingFrame) //If the global frame is the first frame.
                    {
                        _Frame = endingFrame; //Set the frame to the starting frame.
                        onLoop.Trigger(this); //Invoke the loop event.
                    }
                    else
                    {
                        _Frame--; //Increment the frame.
                        onFrameChange.Trigger(this); //Invoke the frame change event.
                    }
                    break;
                case LoopType.NoneReverse:
                    if (_Frame == startingFrame) //If the global frame is the first frame.
                    {
                        _State = TimerState.WaitingForEvent; //Set the event to waiting for an event.
                        onFinished.Trigger(this); //Invoke the finished event.
                    }
                    else
                    {
                        _Frame--; //Increment the frame.
                        onFrameChange.Trigger(this); //Invoke the frame change event.
                    }
                    break;
            }
        }

        //Resets the animation.
        public override void Reset()
        {
            //Reset the loop variables.
            InitLoop();
            //Execute the Timer's Reset method.
            base.Reset();
        }
    }
}
