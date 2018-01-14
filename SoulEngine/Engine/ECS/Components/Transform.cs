// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Graphics;
using OpenTK;

#endregion

namespace Soul.Engine.ECS.Components
{
    public class Transform : ComponentBase
    {
        #region Properties

        /// <summary>
        /// The position of the transform along the x axis.
        /// </summary>
        public float X
        {
            get { return _x; }
            set
            {
                HasUpdated = true;
                _x = value;
            }
        }

        private float _x = 0;

        /// <summary>
        /// The position of the transform along the y axis.
        /// </summary>
        public float Y
        {
            get { return _y; }
            set
            {
                HasUpdated = true;
                _y = value;
            }
        }

        private float _y = 0;

        /// <summary>
        /// The width of the transform.
        /// </summary>
        public float Width
        {
            get { return _width; }
            set
            {
                HasUpdated = true;
                _width = value;
            }
        }

        private float _width = 100;

        /// <summary>
        /// The height of the transform.
        /// </summary>
        public float Height
        {
            get { return _height; }
            set
            {
                HasUpdated = true;
                _height = value;
            }
        }

        private float _height = 100;

        /// <summary>
        /// The transform's rotation in radians.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                HasUpdated = true;
                _rotation = value;
            }
        }

        private float _rotation = 0;

        #endregion

        #region Accessors

        /// <summary>
        /// The position of the transform.
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// The size of the transform.
        /// </summary>
        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        /// <summary>
        /// The center of the transform.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
            }
            set
            {
                X = (int) Math.Ceiling(value.X - Width / 2);
                Y = (int) Math.Ceiling(value.Y - Height / 2);
            }
        }

        /// <summary>
        /// The bounds of the transform.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(Position, Size); }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

        #endregion
    }
}