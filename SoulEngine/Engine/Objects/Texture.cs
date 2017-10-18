// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Raya.Graphics;
using Raya.Primitives;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Objects
{
    public class Texture : GameObject
    {
        #region Properties

        /// <summary>
        /// The texture's color.
        /// </summary>
        public Color Color
        {
            get { return _nativeObject.Color; }
            set { _nativeObject.Color = value; }
        }

        /// <summary>
        /// The currently running animation.
        /// </summary>
        public AnimationData Animation;

        /// <summary>
        /// Whether the texture is attached to a basic shape.
        /// </summary>
        private bool _shapeMode;

        #endregion

        #region Raya API

        /// <summary>
        /// The sprite object inside the Raya API.
        /// </summary>
        private Sprite _nativeObject;

        /// <summary>
        /// The name of the loaded texture.
        /// </summary>
        private Raya.Graphics.Texture _texture;
        #endregion

        /// <summary>
        /// Create a new texture component.
        /// </summary>
        /// <param name="textureName">The loaded texture's name. If it isn't loaded it will be.</param>
        public Texture(string textureName)
        {
            _texture = AssetLoader.GetTexture(textureName);
        }

        /// <summary>
        /// Initializes the texture.
        /// </summary>
        public override void Initialize()
        {
            // Check if attached to a shape.
            BasicShape shape = Parent as BasicShape;
            if (shape != null)
            {
                _shapeMode = true;

                shape.Texture = _texture;

                return;
            }

            _nativeObject = new Sprite(_texture);
            Position = new Vector2(0, 0);
            Size = new Vector2(0, 0);

            // Attach to parent events.
            ((GameObject) Parent).onSizeChanged += UpdateSize;
            ((GameObject) Parent).onPositionChanged += UpdatePosition;
            ((GameObject) Parent).onRotationChanged += UpdateRotation;

            // Attach to own events.
            onSizeChanged += UpdateSize;
            onPositionChanged += UpdatePosition;
            onRotationChanged += UpdateRotation;

            // Update everything.
            UpdateSize();
            UpdatePosition();
            UpdateRotation();
        }

        /// <summary>
        /// Draw an area of the texture instead of the whole texture.
        /// </summary>
        /// <param name="area">The area to draw, relative to the texture.</param>
        public void DrawArea(Rectangle area)
        {
            // Check if in an animation.
            if (Animation != null && Animation.Playing)
            {
                Animation.Playing = false;
                Debugger.DebugMessage(DebugMessageSource.Execution, "Stopping animation to draw area.");
            }

            _nativeObject.TextureRect = area;

            // Update origin and scaling.
            UpdateSize();
        }

        #region Animation

        public void Animate(Vector2 frameSize, int speed = 20, LoopType loopType = LoopType.Normal, int startingFrame = 0,
            int endingFrame = -1, Vector2? spacing = null)
        {
            // Create the animation object.
            Animation = new AnimationData
            {
                SpriteSheetSize = _texture.Size,
                FrameSize = frameSize,
                LoopType = loopType,
                StartingFrame = startingFrame,
                EndingFrame = endingFrame,
                Spacing = spacing != null ? spacing.GetValueOrDefault() : new Vector2(0, 0),
                Speed = speed
            };

            // Start the animation right away.
            UpdateAnimationFrame();
        }

        #endregion

        #region Syncronization Functions

        private void UpdateSize()
        {
            // Set origin to the center.
            _nativeObject.Origin = new Vector2f(_nativeObject.TextureRect.Size.X / 2,
                _nativeObject.TextureRect.Size.Y / 2);

            Vector2f combined = (Vector2f) (((GameObject) Parent).Size + Size);
            _nativeObject.Scale = combined / (Vector2f) _nativeObject.TextureRect.Size;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector2f combinedSize = (Vector2f) (((GameObject) Parent).Size + Size);
            Vector2f combined = (Vector2f) (((GameObject) Parent).Position + Position);

            // Set the position and offset by the origin.
            _nativeObject.Position = combined + combinedSize / 2;
        }

        private void UpdateRotation()
        {
            float combined = ((GameObject) Parent).RotationDegree + RotationDegree;
            _nativeObject.Rotation = combined;
        }

        private void UpdateAnimationFrame()
        {
            if (_shapeMode)
            {
                // Set the shape's texture rect to the first frame.
                ((BasicShape)Parent).TextureRect = Animation.CurrentFrameRect;
            }
            else
            {
                // Set the current texture to the first frame.
                _nativeObject.TextureRect = Animation.CurrentFrameRect;
                UpdateSize();
            }
        }

        #endregion

        public void Destroy()
        {
            // Detach from events if not in shape mode.
            if (!_shapeMode)
            {
                ((GameObject)Parent).onSizeChanged -= UpdateSize;
                ((GameObject)Parent).onPositionChanged -= UpdatePosition;
                ((GameObject)Parent).onRotationChanged -= UpdateRotation;
            }

            // Remove texture reference.
            _texture = null;

            // Unhook from parent.
            Parent.RemoveChild(this);

            // Dispose the native object.
            _nativeObject.Dispose();
        }

        public override void Update()
        {
            // Check if an animation is running.
            if (Animation != null)
                if (Animation.Update())
                {
                    // Update the animation frame if required.
                    UpdateAnimationFrame();
                }

            // Draw if not in shape mode, in which the shape will draw the texture.
            if (!_shapeMode)
            {
                // Draw the Raya sprite.
                Core.Draw(_nativeObject);
            }
        }
    }

    /// <summary>
    /// A handler for animation data within the Texture object.
    /// </summary>
    public class AnimationData
    {
        #region FrameSplit Properties

        /// <summary>
        /// The size of the spritesheet.
        /// </summary>
        public Vector2 SpriteSheetSize
        {
            get { return _spriteSheetSize; }
            set
            {
                _spriteSheetSize = value;

                // Recalculate rows and columns.
                CalculateFrames();
            }
        }

        private Vector2 _spriteSheetSize;

        /// <summary>
        /// The size of individual frames.
        /// </summary>
        public Vector2 FrameSize
        {
            get { return _frameSize; }
            set
            {
                _frameSize = value;

                // Recalculate rows and columns.
                CalculateFrames();
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

                // Recalculate rows and columns.
                CalculateFrames();
            }
        }

        private Vector2 _spacing;

        #endregion

        #region Animation Properties

        /// <summary>
        /// The animation starting frame.
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

        private int _startingFrame;

        /// <summary>
        /// The animation ending frame.
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

        private int _endingFrame;

        /// <summary>
        /// The animation looping type.
        /// </summary>
        public LoopType LoopType { get; set; }

        /// <summary>
        /// The speed at which the frames should swap
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// The current frame being displayed.
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// Whether the animation is playing.
        /// </summary>
        public bool Playing = true;

        #endregion

        #region Informational

        /// <summary>
        /// The current frame from the total frame count.
        /// </summary>
        public int CurrentFrameTotal
        {
            get { return StartingFrame + CurrentFrame; }
        }

        /// <summary>
        /// The bounding rectangle of the current frame.
        /// </summary>
        public Rectangle CurrentFrameRect
        {
            get
            {
                // Get the current row and column.
                int row = (int) (CurrentFrame / (float) _columns);
                int column = CurrentFrame % _columns;

                // Generate texture rectangle from the current frame.
                return new Rectangle(FrameSize.X * column + Spacing.X * (column + 1),
                    FrameSize.Y * row + Spacing.Y * (row + 1), FrameSize.X, FrameSize.Y);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when the animation ends, or loops.
        /// </summary>
        public Action<AnimationData> OnFinish;

#endregion

        #region Calculated

        private int _rows;
        private int _columns;

        /// <summary>
        /// The total number of frames.
        /// </summary>
        public int TotalFrames
        {
            get { return (_rows * _columns) - 1; }
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
                    case LoopType.None:
                    case LoopType.Normal:
                    case LoopType.NormalThenReverse:
                        return EndingFrame - StartingFrame;
                    case LoopType.Reverse:
                    case LoopType.NoneReverse:
                        return StartingFrame - EndingFrame;
                }

                return -1;
            }
        }

        private float _timer;

        private bool _flagReverse = false;

        #endregion

        #region Playback

        /// <summary>
        /// Updates the animation.
        /// </summary>
        /// <returns>Whether a refresh of the frame rect is needed.</returns>
        public bool Update()
        {
            // Check if playing.
            if (!Playing) return false;

            // Check if forcing an update.
            if (_timer == -1)
            {
                _timer = 0;
                return true;
            }
            // If stopping update.
            if (_timer == -2) return false;

            // Increment timer.
            _timer += Core.FrameTime;

            // Check if enough time has passed.
            if (!(_timer > Speed)) return false;

            _timer -= Speed;
            NextFrame();
            return true;
        }

        /// <summary>
        /// Switches to the next frame.
        /// </summary>
        public void NextFrame()
        {
            switch (LoopType)
            {
                case LoopType.None:
                    // If the global frame is the last frame.
                    if (CurrentFrame == EndingFrame)
                    {
                        OnFinish?.Invoke(this);
                        // Stop the timer.
                        _timer = -2;
                    }
                    else
                    {
                        // Increment the frame.
                        CurrentFrame++;
                    }
                    break;
                case LoopType.Normal:
                    // If the global frame is the last frame.
                    if (CurrentFrame == EndingFrame)
                    {
                        //Loop to the starting frame.
                        CurrentFrame = StartingFrame;

                        // Call the loop event.
                        OnFinish?.Invoke(this);
                    }
                    else
                    {
                        // Increment the frame.
                        CurrentFrame++;
                    }
                    break;
                case LoopType.NormalThenReverse:
                    // If the global frame is the last frame and going in reverse or the first and not going in reverse.
                    if ((CurrentFrame == EndingFrame && _flagReverse == false) || (CurrentFrame == StartingFrame && _flagReverse == true))
                    {
                        // Change the reverse flag.
                        _flagReverse = !_flagReverse;

                        // Call the loop event.
                        OnFinish?.Invoke(this);

                        // Depending on the direction set the frame to be the appropriate one.
                        CurrentFrame = _flagReverse ? EndingFrame - 1 : StartingFrame + 1;
                    }
                    else
                    {
                        // Modify the current frame depending on the direction we are going in.
                        if (_flagReverse)
                            CurrentFrame--;
                        else
                            CurrentFrame++;
                    }
                    break;
                case LoopType.Reverse:
                    // If the global frame is the first frame.
                    if (CurrentFrame == StartingFrame)
                    {
                        // Loop to the ending frame.
                        CurrentFrame = EndingFrame;

                        // Call the loop event.
                        OnFinish?.Invoke(this);
                    }
                    else
                    {
                        // Otherwise decrement the frame, as we are going in reverse.
                        CurrentFrame--;
                    }
                    break;
                case LoopType.NoneReverse:
                    // If the global frame is the first frame.
                    if (CurrentFrame == StartingFrame)
                    {
                        // Call the finish event.
                        OnFinish?.Invoke(this);

                        // Stop the timer.
                        _timer = -2;
                    }
                    else
                    {
                        // Decrement the frame, as we are going in reverse.
                        CurrentFrame--;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Calculates rows and columns from the frame size, spritesheet size, and spacing.
        /// </summary>
        private void CalculateFrames()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_frameSize == null || _spacing == null) return;

            // Check if the frame size is invalid.
            if (_frameSize.X == 0 || _frameSize.Y == 0) return;

            // Calculate columns and rows.
            _columns = SpriteSheetSize.X / _frameSize.X;
            _rows = SpriteSheetSize.Y / _frameSize.Y;

            // Reset the current frame.
            ResetFrame();
        }

        /// <summary>
        /// Resets the current frame.
        /// </summary>
        private void ResetFrame()
        {
            // Set the current frame based on the loop type.
            switch (LoopType)
            {
                case LoopType.None:
                case LoopType.Normal:
                case LoopType.NormalThenReverse:
                    CurrentFrame = StartingFrame;
                    break;
                case LoopType.Reverse:
                case LoopType.NoneReverse:
                    CurrentFrame = EndingFrame;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Force an update. This is checked within the Update().
            _timer = -1;
        }

        #endregion
    }
}