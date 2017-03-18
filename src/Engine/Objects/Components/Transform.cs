using Microsoft.Xna.Framework;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Location, Size, and Rotation.
    /// </summary>
    public class Transform : Component
    {
        #region "Variables"
        //The position of the object within the scene.
        #region "Positional"
        /// <summary>
        /// 
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 PositionFull
        {
            get
            {
                return new Vector3(X, Y, attachedObject.Priority);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                if(attachedObject != null) attachedObject.Priority = value.Z;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Bounds.Center.ToVector2();
                //return new Vector2(X + Width / 2, Y + Height / 2); <-- Custom Implementation
            }
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }
        #endregion
        //The size of the box wrapping the object.
        #region "Size"
        /// <summary>
        /// 
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(Width, Height);
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        #endregion
        //The rotation of the object.
        #region "Rotation"
        /// <summary>
        /// The object's rotation in radians.
        /// </summary>
        public float Rotation { get; set; }
        /// <summary>
        /// The object's rotation in degrees.
        /// </summary>
        public int RotationDegree
        {
            get
            {
                return (int) MathHelper.ToDegrees(Rotation);
            }
            set
            {
                Rotation = MathHelper.ToRadians(value);
            }
        }
        #endregion
        //Main variables.
        #region "Primary"
        /// <summary>
        /// The box wrapping the object.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }
        #endregion
        //Private variables.
        #region "Private"
        private bool _moveRunning
        {
            get
            {
                if (_moveTicker != null && _moveTicker.State == Enums.TickerState.Running) return true;
                return false;
            }
        }
        private Vector2 _moveStartPosition;
        private Vector2 _moveEndPosition;
        private Ticker _moveTicker;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public Transform()
        {
            PositionFull = new Vector3(0, 0, 0);
            Size = new Vector2(100, 100);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Size"></param>
        public Transform(Vector3 Position, Vector2 Size)
        {
            PositionFull = Position;
            this.Size = Size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Size"></param>
        public Transform(Vector2 Position, Vector2 Size)
        {
            this.Position = Position;
            this.Size = Size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        public Transform(Vector3 Position)
        {
            PositionFull = Position;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        public Transform(Vector2 Position)
        {
            this.Position = Position;
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// Is run every tick.
        /// </summary>
        public override void Update()
        {

        }
        /// <summary>
        /// Moves the object to the desired location over the desired duration.
        /// </summary>
        /// <param name="Duration">The time the movement should take.</param>
        /// <param name="TargetLocation">The location to move to.</param>
        /// <param name="Force">Whether to force movement, which means it will overwrite any current movement.</param>
        public void MoveTo(int Duration, Vector2 TargetLocation, bool Force = false)
        {
            //Check if the effect is already running, and if we are not forcing.
            if (Force) if (_moveRunning) moveOver(); else if (_moveRunning) return;

            _moveStartPosition = Position;
            _moveEndPosition = TargetLocation;

            _moveTicker = new Ticker(1, Duration, true);
            ESystem.Add(new Listen(EType.TICKER_TICK, moveApply, _moveTicker));
            ESystem.Add(new Listen(EType.TICKER_DONE, moveOver, _moveTicker));
        }
        /// <summary>
        /// Moves the object towards the desired direction.
        /// </summary>
        /// <param name="direction">A normalized vector as a direction.</param>
        /// <param name="Force">Whether to force movement, which means it will overwrite any current movement.</param>
        public void MoveIn(Vector2 direction, bool Force = false)
        {
            //Check if the effect is already running, and if we are not forcing.
            if (Force) if (_moveRunning) moveOver(); else if (_moveRunning) return;

            direction.Normalize();
            Position += Vector2.Transform(direction, Matrix.CreateRotationZ(Rotation));
        }
        #region "Positioning"
        /// <summary>
        /// Center the object within the window.
        /// </summary>
        public void ObjectCenter()
        {
            ObjectCenterX();
            ObjectCenterY();
        }
        /// <summary>
        /// Center the object within the window on the X axis.
        /// </summary>
        public void ObjectCenterX()
        {
            X = Settings.Width / 2 - Width / 2;
        }
        /// <summary>
        /// Center the object within the window on the Y axis.
        /// </summary>
        public void ObjectCenterY()
        {
            Y = Settings.Height / 2 - Height / 2;
        }
        /// <summary>
        /// Makes the object fit the whole screen.
        /// </summary>
        public void ObjectFullscreen()
        {
            Width = Settings.Width;
            Height = Settings.Height;
            X = 0;
            Y = 0;
        }
        #endregion
        #endregion
        //Private functions.
        #region "Internal Functions"
        private void moveApply()
        {
            X = MathHelper.Lerp(_moveStartPosition.X, _moveEndPosition.X, (_moveTicker.TimeSinceStart / _moveTicker.TotalTime));
            Y = MathHelper.Lerp(_moveStartPosition.Y, _moveEndPosition.Y, (_moveTicker.TimeSinceStart / _moveTicker.TotalTime));
        }
        private void moveOver()
        {
            //Remove the ticker.
            _moveTicker.Dispose();       
        }
        #endregion

        //Other
        #region "Component Interface"
        public override void Draw() { }
        public override void DrawFree() { }
        #endregion
    }
}
