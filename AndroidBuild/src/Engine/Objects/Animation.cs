using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
#if !ANDROID
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
    class Animation
    {
    #region "Declarations"
        //Public Fields
        public Texture textureAtlas = new Texture(); //The spritesheet to be cut into individual frames.
        public List<Texture2D> Frames
        {
            get
            {
                return frames;
            }
        } //The list of frames.

        //Settings
        public LoopType Loop; //The type of loop to use.
        public enum LoopType //Types of loops.
        {
            None, //Disabled looping, animation will play once.
            Normal, //Animation will loop normally, after the last frame is the first frame.
            NormalThenReverse //The animation will play in reverse after reaching then last frame.
        }

        //Events
        public Action onFrameChange; //The event for when the frame changes.
        public Action onFinished; //The event for when the animation has finished.
        public Action onLoop; //The event for when the animation loops back.

        //State
        public AnimationState State //The state of the timer, readonly for user reading.
        {
            get
            {
                return _State;
            }
        }
        private AnimationState _State = AnimationState.None; //The state of the timer, private for editing within the object. 
        public enum AnimationState  //The possible states.
        {
            None, //Not setup.
            Paused, //For when the animation is paused.
            Playing, //For when the animation is playing.
            PlayingReverse, //For when the animation is playing in reverse.
            WaitingForEvent, //When waiting to execute the onFinished event.
            Done //When the animation is done. For instance when loop is false and it has played once.
        }

        //Public Accessors
        public int Frame //The current frame's number, counting the first frame as zero.
        {
            get
            {
                return _Frame - startingFrame;
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
                AnimationTimer.Delay = 1000f / (float)value; 
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
        private Timer AnimationTimer = new Timer(); //The timer that handles frame timing.

        //Atlas Variables
        private int fWidth; //The width of each individual frame. This is inputed.
        private int fHeight; //The height of each individual frame. This is inputed.
        private int fRows; //The number of rows of frames the spritesheet contains. This is calculated.
        private int fColumns; //The number of columns of frames the spritesheet contains. This is calculated.
        private List<Texture2D> frames = new List<Texture2D>(); //All the frames from the atlas cut.
    #endregion

        //The constructor.
        public Animation(Texture2D SpriteSheet, int frameWidth, int frameHeight, int startingFrame = 0, int endingFrame = -1, LoopType AnimationLoop = LoopType.Normal, int FPS = 10, bool Reverse = false)
        {
            //Assign variables
            textureAtlas.Image = SpriteSheet;
            fWidth = frameWidth;
            fHeight = frameHeight;
            Loop = AnimationLoop;

            //Calculate columns and rows.
            fColumns = textureAtlas.Image.Width / fWidth;
            fRows = textureAtlas.Image.Height / fHeight;

            //Setup the timer.
            AnimationTimer = new Timer();
            AnimationTimer.onTick = TimerTick;

            //Assign frame variables
            FramesCountTotal = fColumns * fRows;
            FramesCount = endingFrame - startingFrame;
            this.FPS = FPS;

            //Setup the starting and ending frames.
            this.startingFrame = startingFrame;
            this.endingFrame = endingFrame;
            _Frame = startingFrame - 1; //We remove one because we are going to load a single tick to get the first frame loaded.

            //Check if playing in reverse.
            if(Reverse == true && Loop == LoopType.None)
            {
                _State = AnimationState.PlayingReverse;
                _Frame = endingFrame;
            }
            else
            {
                _State = AnimationState.Playing;
            }

            //Split frames.
            SplitSheet();

            TimerTick(); //We run one frame here.

            Pause();
        }

        //Is run every frame.
        public void Run()
        {
            //Check the animation's state.
            switch (_State)
            {
                case AnimationState.None:
                case AnimationState.Done:
                case AnimationState.Paused:
                    return;
                case AnimationState.WaitingForEvent:
                    _State = AnimationState.Done;
                    onFinished?.Invoke();
                    return;
            }

            //Update the timer.
            AnimationTimer.Run();           
        }
        //Get the texture of the current frame.
        public Texture2D GetFrameTexture()
        {
            //Check if the frame we are trying to access is beyond the cut frames.
            if(_Frame > frames.Count)
            {
                return Core.missingimg;
            }

            return frames[_Frame];
        }

        public void SplitSheet()
        {
            for (int i = 0; i < FramesCountTotal; i++)
            {
                //Calculate the location of the current sprite within the image.
                int Row = (int)(i / (float)fColumns);
                int Column = i % fColumns;

                //Transform into a rectangle.
                Rectangle FrameRect = new Rectangle(fWidth * Column, fHeight * Row, fWidth, fHeight);

                //Create a new texture object with the dimensions of the sprite.
                Texture2D curFrame = new Texture2D(Core.graphics.GraphicsDevice, FrameRect.Width, FrameRect.Height);
                //Create a color array to hold the color data of the sprite.
                Color[] FrameData = new Color[FrameRect.Width * FrameRect.Height];
                //Get the color data from the spritesheet.
                textureAtlas.Image.GetData(0, FrameRect, FrameData, 0, FrameData.Length);
                //Set the colors we extracted to the texture object.
                curFrame.SetData<Color>(FrameData);
                //Add the texture to the array.
                frames.Add(curFrame);
            }
        }

        //Is run on timer tick.
        private void TimerTick()
        {

            //If animation state is playing then we continue with the code below.

            if (_Frame + 1 > endingFrame && _State != AnimationState.PlayingReverse) //Check if this is the last frame.
            {
                //If looping start from the first frame.
                if(Loop == LoopType.Normal)
                {
                     onLoop?.Invoke(); //Call on loop event.
                    _Frame = startingFrame;
                }
                else if(Loop == LoopType.NormalThenReverse)
                {
                    onLoop?.Invoke(); //Call on loop event.
                    _Frame--; //Start counting back.
                    _State = AnimationState.PlayingReverse; //Start playing in reverse.
                }
                else
                {
                    WaitForEvent();
                }
            }
            else if(_State == AnimationState.PlayingReverse) //If playing in reverse.
            {
                //Check if this is the first frame.
                if(_Frame - 1 < startingFrame)
                {
                    if(Loop == LoopType.NormalThenReverse) //Check if reversed by a loop.
                    {
                        onLoop?.Invoke(); //Call on loop event.
                        _State = AnimationState.Playing; //Start playing towards the end.
                        _Frame++; //Start counting forward.
                    }
                    else
                    {
                        WaitForEvent();
                    }
                }
                else
                {
                    _Frame--; //Subtract from the current frame variable.
                }
            }
            else
            {
                
                _Frame++; //Increment the current frame variable.
            }

            //Call on frame change event.
            onFrameChange?.Invoke();
        }

        //The play state preserved when paused.
        private AnimationState pausedStateHolder = AnimationState.None; 
        //Pause if running.
        public void Pause()
        {
            if (_State == AnimationState.Playing || _State == AnimationState.PlayingReverse)
            {
                pausedStateHolder = _State;
                _State = AnimationState.Paused;
                AnimationTimer.Pause();
            }
        }
        //Start if pausing.
        public void Play()
        {
            if(_State == AnimationState.Paused)
            {
                _State = pausedStateHolder;
                AnimationTimer.Start();
            }
        }
        //Resets the animation.
        public void Reset()
        {
            if(pausedStateHolder == AnimationState.PlayingReverse)
            {
                _State = pausedStateHolder;
                _Frame = endingFrame;
                Pause();
            }
            if(pausedStateHolder == AnimationState.Playing)
            {
                _State = pausedStateHolder;
                _Frame = startingFrame;
                Pause();
            }
        }
        //Changes to waiting for the event.
        private void WaitForEvent()
        {
            pausedStateHolder = _State;
            _State = AnimationState.WaitingForEvent;
        }
        //Change the current frame to a specified relative frame.
        public void SetCurrentFrame(int relativeFrame)
        {
            _Frame = startingFrame + relativeFrame;
        }
    }
#endif
#if ANDROID
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // Used for animating textures from a spritesheet.                          //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Animation
    {
        #region "Declarations"
        //Public Fields
        public Texture textureAtlas = new Texture(); //The spritesheet to be cut into individual frames.
        public List<Texture2D> Frames
        {
            get
            {
                return frames;
            }
        } //The list of frames.

        //Settings
        public LoopType Loop; //The type of loop to use.
        public enum LoopType //Types of loops.
        {
            None, //Disabled looping, animation will play once.
            Normal, //Animation will loop normally, after the last frame is the first frame.
            NormalThenReverse //The animation will play in reverse after reaching then last frame.
        }

        //Events
        public Action onFrameChange; //The event for when the frame changes.
        public Action onFinished; //The event for when the animation has finished.
        public Action onLoop; //The event for when the animation loops back.

        //State
        public AnimationState State //The state of the timer, readonly for user reading.
        {
            get
            {
                return _State;
            }
        }
        private AnimationState _State = AnimationState.None; //The state of the timer, private for editing within the object. 
        public enum AnimationState  //The possible states.
        {
            None, //Not setup.
            Paused, //For when the animation is paused.
            Playing, //For when the animation is playing.
            PlayingReverse, //For when the animation is playing in reverse.
            WaitingForEvent, //When waiting to execute the onFinished event.
            Done //When the animation is done. For instance when loop is false and it has played once.
        }

        //Public Accessors
        public int Frame //The current frame's number, counting the first frame as zero.
        {
            get
            {
                return _Frame - startingFrame;
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
                AnimationTimer.Delay = 1000f / (float)value;
                _FPS = value;
            }
        }

        //Private Workings
        private int startingFrame = 0; //The frame to start from, from all the frames in the spritesheet.
        private int endingFrame //The frame to end at, from all the frames in the spritesheet. This is the accessor that will return the last frame when the ending frame is -1.
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
        private int _endingFrame = 0; //The frame to end at, from all the frames in the spritesheet.
        private int _Frame = 0; //The current frame's number, from the total frame count.

        private int FramesCount = 0; //The total number of frames in the animation. This is calculated.
        private int FramesCountTotal = 0; //The total number of frames on the spritesheet.

        //Timing
        private int _FPS = 0; //The frames per second of the animation. This is inputed.
        private Timer AnimationTimer = new Timer(); //The timer that handles frame timing.

        //Atlas Variables
        private int fWidth; //The width of each individual frame. This is inputed.
        private int fHeight; //The height of each individual frame. This is inputed.
        private int fRows; //The number of rows of frames the spritesheet contains. This is calculated.
        private int fColumns; //The number of columns of frames the spritesheet contains. This is calculated.
        private List<Texture2D> frames = new List<Texture2D>(); //All the frames from the atlas cut.
        #endregion

        //The constructor.
        public Animation(Texture2D SpriteSheet, int frameWidth, int frameHeight, int startingFrame = 0, int endingFrame = -1, LoopType AnimationLoop = LoopType.Normal, int FPS = 10, bool Reverse = false)
        {
			//Assign variables
			textureAtlas.Image = SpriteSheet;
			fWidth = frameWidth;
			fHeight = frameHeight;
			Loop = AnimationLoop;

			//Calculate columns and rows.
			fColumns = textureAtlas.Image.Width / fWidth;
			fRows = textureAtlas.Image.Height / fHeight;

			//Setup the timer.
			AnimationTimer = new Timer();
			AnimationTimer.onTick = TimerTick;

			//Assign frame variables
			FramesCountTotal = fColumns * fRows;
			FramesCount = endingFrame - startingFrame;
			this.FPS = FPS;

			//Setup the starting and ending frames.
			this.startingFrame = startingFrame;
			this.endingFrame = endingFrame;
			_Frame = startingFrame - 1; //We remove one because we are going to load a single tick to get the first frame loaded.

			//Check if playing in reverse.
			if (Reverse == true && Loop == LoopType.None)
			{
				_State = AnimationState.PlayingReverse;
				_Frame = endingFrame;
			}
			else
			{
				_State = AnimationState.Playing;
			}

			//Split frames.
			SplitSheet();
        }

        //Is run every frame.
        public void Run()
        {

        }
        //Get the texture of the current frame.
        public Texture2D GetFrameTexture()
        {
            return Core.missingimg;
        }

        public void SplitSheet()
        {
			for (int i = 0; i < FramesCountTotal; i++)
			{
				//Calculate the location of the current sprite within the image.
				int Row = (int)(i / (float)fColumns);
				int Column = i % fColumns;

				//Transform into a rectangle.
				Rectangle FrameRect = new Rectangle(fWidth * Column, fHeight * Row, fWidth, fHeight);

				//Create a new texture object with the dimensions of the sprite.
				Texture2D curFrame = new Texture2D(Core.graphics.GraphicsDevice, FrameRect.Width, FrameRect.Height);
				//Create a color array to hold the color data of the sprite.
				Color[] FrameData = new Color[FrameRect.Width * FrameRect.Height];
				//Get the color data from the spritesheet.
				//textureAtlas.Image.GetData(0, FrameRect, FrameData, 0, FrameData.Length);
				//Set the colors we extracted to the texture object.
				curFrame.SetData<Color>(FrameData);
				//Add the texture to the array.
				frames.Add(curFrame);
			}
        }

        //Is run on timer tick.
        private void TimerTick()
        {

        }

        //The play state preserved when paused.
        private AnimationState pausedStateHolder = AnimationState.None;
        //Pause if running.
        public void Pause()
        {

        }
        //Start if pausing.
        public void Play()
        {

        }
        //Resets the animation.
        public void Reset()
        {

        }
        //Changes to waiting for the event.
        private void WaitForEvent()
        {

        }
    }
#endif
}
