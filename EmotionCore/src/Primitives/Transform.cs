// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;

#endregion

namespace Emotion.Primitives
{
    public class Transform
    {
        #region Properties

        public float X
        {
            get => _x;
            set
            {
                if (_x == value) return;

                _x = value;
                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                if (_y == value) return;

                _y = value;
                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        public float Z
        {
            get => _z;
            set
            {
                if (_z == value) return;

                _z = value;
                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        public float Width
        {
            get => _width;
            set
            {
                if (_width == value) return;

                _width = value;
                OnResize?.Invoke(this, EventArgs.Empty);
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                if (_height == value) return;

                _height = value;
                OnResize?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Higher Properties

        public Vector3 Position
        {
            get => new Vector3(X, Y, Z);
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public float RightSide
        {
            get => X + Width;
            set => X = value - Width;
        }

        public float BottomSide
        {
            get => Y + Height;
            set => Y = value - Height;
        }

        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }

        public Vector2 CenterRelative
        {
            get => new Vector2(Width / 2, Height / 2);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is invoked when the transform's size is changed.
        /// </summary>
        public event EventHandler<EventArgs> OnResize;

        /// <summary>
        /// Is invoked when the transform's position is changed.
        /// </summary>
        public event EventHandler<EventArgs> OnMove;

        #endregion

        #region Private Holders

        private float _x;
        private float _y;
        private float _z;
        private float _width;
        private float _height;

        #endregion

        #region Constructors

        protected Transform(Vector3 position, Vector2 size) : this(position.X, position.Y, position.Z, size.X, size.Y)
        {
        }

        protected Transform(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
        }

        #endregion

        /// <summary>
        /// Set the Transform's size and position from a rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle's properties to copy.</param>
        public void FromRectangle(Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        /// <summary>
        /// Convert the transform to a rectangle.
        /// </summary>
        /// <returns>A rectangle which represents the transform.</returns>
        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return $"[X:{X} Y:{Y} Z:{Z} | Width:{Width} Height:{Height}]";
        }
    }
}