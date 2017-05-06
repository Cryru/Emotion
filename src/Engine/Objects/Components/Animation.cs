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
        #region "Frame Information"
        /// <summary>
        /// Returns the texture of the current frame.
        /// </summary>
        public Texture2D FrameTexture
        {
            get
            {
                if (Frames.Count == 0) return AssetManager.MissingTexture;

                return Frames[FrameIndex] as Texture2D;
            }
        }
        /// <summary>
        /// The frame to start from, or end at if in reverse, from all the frames in the spritesheet.
        /// </summary>
        public int StartingFrame
        {
            get
            {
                return _startingframe;
            }
            set
            {
                _startingframe = value;
                //Reset the current frame.
                Reset();
            }
        }
        /// <summary>
        /// The frame to end at, or start at if in reverse, from all the frames in the spritesheet. Set to -1 for last frame.
        /// </summary>
        public int EndingFrame
        {
            get
            {
                if (_endingFrame == -1)
                {
                    return Frames.Count - 1;
                }

                return _endingFrame;
            }
            set
            {
                _endingFrame = value;
                //Reset the current frame.
                Reset();
            }
        }
        /// <summary>
        /// The number of the current frame from the range of the starting and ending frame.
        /// </summary>
        public int Frame
        {
            get
            {
                return _frameindex - StartingFrame;
            }
            set
            {
                _frameindex = StartingFrame + value;
            }
        }
        /// <summary>
        /// The index of the current frame from the total frames.
        /// </summary>
        public int FrameIndex
        {
            get
            {
                return _frameindex;
            }
        }
        /// <summary>
        /// The total number of frames used by the animation.
        /// </summary>
        public int FramesCount
        {
            get
            {
                return EndingFrame - StartingFrame;
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
                clock.Delay = 1000f / value;
                _FPS = value;
            }
        }
        #endregion
        #region "Spritesheet Information"
        /// <summary>
        /// The frames that make up the animation cut from the spritesheet.
        /// </summary>
        public List<RenderTarget2D> Frames
        {
            get
            {
                return frames;
            }
        }
        /// <summary>
        /// The size of individual frames within the spritesheet.
        /// </summary>
        public Vector2 FrameSize
        {
            get
            {
                return _frameSize;
            }
            set
            {
                _frameSize = value;
                //Recut frames.
                Setup();
            }
        }
        /// <summary>
        /// The space between frames.
        /// </summary>
        public Vector2 Spacing = Vector2.Zero;
        #endregion
        #region "Loop"
        /// <summary>
        /// The type of loop.
        /// </summary>
        public LoopType Loop
        {
            get
            {
                return _loop;
            }
            set
            {
                _loop = value;
                Reset();
            }
        }
        /// <summary>
        /// Used for NormalThenReverse, is true when reversing.
        /// </summary>
        private bool flagReverse = false;
        #endregion
        #region "Privates"
        private int _startingframe = 0;
        private int _endingFrame = 0;
        private int _frameindex = 0;
        private int _FPS = 0;
        private List<RenderTarget2D> frames = new List<RenderTarget2D>();
        private Vector2 _frameSize = new Vector2();
        private LoopType _loop;
        /// <summary>
        /// Whether to cut the frames in the next compose.
        /// </summary>
        private bool cutFrames = false;
        /// <summary>
        /// The internal time keeping object.
        /// </summary>
        private Ticker clock;
        #endregion
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when the animation finishes.
        /// </summary>
        public static event EventHandler<EventArgs> OnFinish;
        #endregion

        /// <summary>
        /// Initialize an animation component. The ActiveTexture's texture will be used as a spritesheet.
        /// </summary>
        /// <param name="FrameSize">The size of individual frames within the spritesheet.</param>
        /// <param name="StartingFrame">The frame to start from, or end on if in reverse, from all the frames in the spritesheet.</param>
        /// <param name="EndingFrame"> The frame to end at, or start on if in reverse, from all the frames in the spritesheet. </param>
        /// <param name="Loop">The type of loop.</param>
        /// <param name="FPS">The frames per second of the animation, essentially the speed.</param>
        /// <param name="Spacing">The spacing between frames in the picture.</param>
        public Animation(Vector2 FrameSize, int StartingFrame = 0, int EndingFrame = -1, LoopType Loop = LoopType.Normal, int FPS = 10, Vector2 Spacing = new Vector2())
        {
            //Setup clock.
            clock = new Ticker(1, -1, true);
            clock.Start();
            clock.OnTick += NextFrame;

            //Assign variables
            this.FPS = FPS;
            this.Loop = Loop;
            this.Spacing = Spacing;

            //Assign to privates to prevent triggering Setup() before initialization.
            _frameSize = FrameSize;
            _startingframe = StartingFrame;
            _endingFrame = EndingFrame;
        }

        /// <summary>
        /// Initializes the component. Is automatically called when adding the component to an object.
        /// </summary>
        public override void Initialize()
        {
            //Check for ActiveTexture depedency and hook to the texture changed event.
            if (!attachedObject.HasComponent<ActiveTexture>()) throw new Exception("Cannot attach an Animation component when there is no ActiveTexture component.");

            attachedObject.Component<ActiveTexture>().OnTextureChanged += ActiveTexture_OnTextureChanged;

            //Cut frames and setup animation.
            Setup();
        }

        /// <summary>
        /// Used to detect a texture change in which case we must recut the frames.
        /// </summary>
        private void ActiveTexture_OnTextureChanged(object sender, EventArgs e)
        {
            if (attachedObject.Component<ActiveTexture>().TextureMode == TextureMode.Animate) Setup();
        }

        /// <summary>
        /// Recuts the frames and reinitializes the animation.
        /// </summary>
        private void Setup()
        {
            //Clear old frames.
            for (int i = 0; i < frames.Count; i++)
            {
                frames[i].Dispose();
            }
            frames.Clear();

            //Get the spritesheet.
            Texture2D Spritesheet = attachedObject.Component<ActiveTexture>().ActualTexture;

            //Calculate the number of frames we will cut.
            int fColumns = Spritesheet.Width / (int) FrameSize.X;
            int fRows = Spritesheet.Height / (int) FrameSize.Y;
            int FrameCount = fColumns * fRows;

            //Generate frame textures for each frame.
            for (int i = 0; i < FrameCount; i++)
            {
                RenderTarget2D tempFrame = new RenderTarget2D(Context.Graphics, (int)FrameSize.X, (int)FrameSize.Y);
                tempFrame.Name = Spritesheet.Name + "_Frame" + i;
                tempFrame.Tag = "notready";

                frames.Add(tempFrame);
            }

            //Reset the frame counter.
            Reset();

            //Request frame cutting.
            cutFrames = true;
        }

        /// <summary>
        /// Cuts the individual frames if they aren't.
        /// </summary>
        public override void Compose()
        {
            //If we are not supposed to cut frames then do nothing.
            if (!cutFrames) return;

            //Get the spritesheet.
            Texture2D Spritesheet = attachedObject.Component<ActiveTexture>().ActualTexture;

            //Calculate columns and rows.
            int fColumns = Spritesheet.Width / (int)FrameSize.X;
            int fRows = Spritesheet.Height / (int)FrameSize.Y;

            for (int i = 0; i < Frames.Count; i++)
            {
                //Calculate the location of the current sprite within the image.
                int Row = (int)(i / (float)fColumns);
                int Column = i % fColumns;

                //Determine which part of the spritesheet we want to cut out next.
                Rectangle FrameRect = new Rectangle((int) FrameSize.X * Column + (int)(Spacing.X * (Column + 1)), 
                    (int) FrameSize.Y * Row + (int)(Spacing.Y * (Row + 1)), (int) FrameSize.X, (int) FrameSize.Y);

                //Switch over to the composer.
                Context.ink.StartRenderTarget(frames[i]);

                //Cut the part we need out.
                Context.ink.Draw(Spritesheet, new Rectangle(0, 0, FrameRect.Width, FrameRect.Height), FrameRect, Color.White);

                //Clear tag.
                frames[i].Tag = null;

                //Stop drawing on the composer and offload the frame to the list.
                Context.ink.EndRenderTarget();
            }
        }

        /// <summary>
        /// Switches over to the next frame.
        /// </summary>
        private void NextFrame(object sender, EventArgs e)
        {
            switch (Loop)
            {
                case LoopType.None:
                    //If the global frame is the last frame.
                    if (_frameindex == EndingFrame)
                    {
                        OnFinish?.Invoke(this, null);
                        clock.Pause();
                    }
                    else
                    {
                        //Increment the frame.
                        _frameindex++;
                    }
                    break;
                case LoopType.Normal:
                    //If the global frame is the last frame.
                    if (_frameindex == EndingFrame)
                    {
                        //Loop to the starting frame.
                        _frameindex = StartingFrame;
                    }
                    else
                    {
                        //Increment the frame.
                        _frameindex++;
                    }
                    break;
                case LoopType.NormalThenReverse:
                    //If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if ((_frameindex == EndingFrame && flagReverse == false) || (_frameindex == StartingFrame && flagReverse == true))
                    {
                        //Change the reverse flag.
                        flagReverse = !flagReverse;

                        //Depending on the direction set the frame to be the appropriate one.
                        if (flagReverse)
                            _frameindex = EndingFrame;
                        else
                            _frameindex = StartingFrame;
                    }
                    else
                    {
                        //Modify the current frame depending on the direction we are going in.
                        if (flagReverse)
                            _frameindex--;
                        else
                            _frameindex++;
                    }
                    break;
                case LoopType.Reverse:
                    //If the global frame is the first frame.
                    if (_frameindex == StartingFrame)
                    {
                        //Loop to the ending frame.
                        _frameindex = EndingFrame;
                    }
                    else
                    {
                        //Otherwise decrement the frame, as we are going in reverse.
                        _frameindex--;
                    }
                    break;
                case LoopType.NoneReverse:
                    //If the global frame is the first frame.
                    if (_frameindex == StartingFrame)
                    {
                        OnFinish?.Invoke(this, null);
                        clock.Pause();
                    }
                    else
                    {
                        //Decrement the frame, as we are going in reverse.
                        _frameindex--;
                    }
                    break;
            }
        }

        #region "Clock Control"
        /// <summary>
        /// Resume animation if paused.
        /// </summary>
        public void Resume()
        {
            clock.Start();
        }

        /// <summary>
        /// Pause animation if playing.
        /// </summary>
        public void Pause()
        {
            clock.Pause();
        }

        /// <summary>
        /// Restarts the animation.
        /// </summary>
        public void Restart()
        {
            Resume();
            Reset();
        }
        #endregion

        #region "Helpers"
        /// <summary>
        /// Resets the frame to the first one depending on the loop type.
        /// </summary>
        private void Reset()
        {
            //Check if playing in reverse.
            if (Loop == LoopType.None || Loop == LoopType.Normal || Loop == LoopType.NormalThenReverse)
            {
                _frameindex = StartingFrame;
            }
            else if (Loop == LoopType.Reverse || Loop == LoopType.NoneReverse)
            {
                _frameindex = EndingFrame;
            }
        }
        #endregion

        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Free resources.
                    for (int i = 0; i < frames.Count; i++)
                    {
                        frames[i].Dispose();
                    }
                    frames = null;
                    clock.Dispose();
                }

                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}
