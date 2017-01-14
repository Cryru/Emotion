using Microsoft.Xna.Framework;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Location, Size, and Rotation.
    /// </summary>
    public class Transform
    {
        //The position of the object within the scene.
        #region "Positional"
        /// <summary>
        /// 
        /// </summary>
        float X { get; set; }
        /// <summary>
        /// 
        /// </summary>
        float Y { get; set; }
        /// <summary>
        /// 
        /// </summary>
        float Z { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Vector3 PositionFull
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        Vector2 Position
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
        Vector2 Center
        {
            get
            {
                return new Vector2(X + Width / 2, Y + Height / 2);
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
        float Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        float Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Vector2 Size
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
        /// 
        /// </summary>
        float Rotation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int RotationDegree
        {
            get
            {
                return Soul.Convert.RadiansToDegrees(Rotation);
            }
            set
            {
                Rotation = Soul.Convert.DegreesToRadians(value);
            }
        }
        #endregion

        /// <summary>
        /// The box wrapping the object.
        /// </summary>
        Rectangle Bounds
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

        #region "Initialization"
        public Transform()
        {
            PositionFull = new Vector3(0, 0, 0);
            Size = new Vector2(100, 100);
        }
        public Transform(Vector3 Position, Vector2 Size)
        {
            PositionFull = Position;
            this.Size = Size;
        }
        public Transform(Vector2 Position, Vector2 Size)
        {
            this.Position = Position;
            this.Size = Size;
        }
        public Transform(Vector3 Position)
        {
            PositionFull = Position;
        }
        public Transform(Vector2 Position)
        {
            this.Position = Position;
        }
        #endregion

        #region "Private Helpers"
        private bool _moveRunning = false;
        private Vector3 _moveStartPosition;
        private Vector3 _moveEndPosition;
        private Ticker _moveTicker;
        #endregion

        #region "Functions"
        /// <summary>
        /// Is run every tick.
        /// </summary>
        public void Update()
        {

        }
        /// <summary>
        /// Moves the object to the desired location over the desired duration.
        /// </summary>
        /// <param name="Duration">The time the movement should take.</param>
        /// <param name="TargetLocation">The location to move to.</param>
        /// <param name="Force">Whether to force movement, which means it will overwrite any current movement.</param>
        public void MoveTo(int Duration, Vector3 TargetLocation, bool Force = false)
        {
            //Check if the effect is already running, and if we are not forcing.
            if (_moveRunning && !Force) return;

            _moveStartPosition = PositionFull;
            _moveEndPosition = TargetLocation;

            _moveTicker = new Ticker(1, Duration, true);
            _moveTicker.onTick.Add(moveApply);
            _moveTicker.onDone.Add(moveOver);
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

        #region "Internal Functions"
        private void moveApply()
        {
            X = MathHelper.Lerp(_moveStartPosition.X, _moveEndPosition.X, (_moveTicker.TimeSinceStart / _moveTicker.TotalTime));
            Y = MathHelper.Lerp(_moveStartPosition.Y, _moveEndPosition.Y, (_moveTicker.TimeSinceStart / _moveTicker.TotalTime));
        }
        private void moveOver()
        {
            _moveRunning = false;
            _moveTicker = null;
        }
        #endregion
    }
}
