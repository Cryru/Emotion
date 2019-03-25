#region Using

using System;
using System.Numerics;

#endregion

namespace Adfectus.Primitives
{
    /// <summary>
    /// A class which holds the position and size of an entity.
    /// </summary>
    public class Transform
    {
        #region Properties

        /// <summary>
        /// The location of the transform on the X-axis.
        /// </summary>
        public virtual float X
        {
            get => _x;
            set
            {
                if (_x == value) return;

                _x = value;
                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The location of the transform on the Y-axis.
        /// </summary>
        public virtual float Y
        {
            get => _y;
            set
            {
                if (_y == value) return;

                _y = value;
                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The location of the transform on the Z-axis.
        /// </summary>
        public virtual float Z
        {
            get => _z;
            set
            {
                if (_z == value) return;

                _z = value;
                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The width of the transform.
        /// </summary>
        public virtual float Width
        {
            get => _width;
            set
            {
                if (_width == value) return;

                _width = value;
                OnResize?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The height of the transform.
        /// </summary>
        public virtual float Height
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

        /// <summary>
        /// The position of the transform within 3D space.
        /// </summary>
        public Vector3 Position
        {
            get => new Vector3(X, Y, Z);
            set
            {
                if (X == value.X && Y == value.Y && Z == value.Z) return;

                X = value.X;
                Y = value.Y;
                Z = value.Z;

                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The size of the transform.
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                if (Width == value.X && Height == value.Y) return;

                Width = value.X;
                Height = value.Y;

                OnResize?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The right side of a transform, as if it was a rectangle.
        /// </summary>
        public float RightSide
        {
            get => X + Width;
            set => X = value - Width;
        }

        /// <summary>
        /// The bottom side of the transform, as if it was a rectangle.
        /// </summary>
        public float BottomSide
        {
            get => Y + Height;
            set => Y = value - Height;
        }

        /// <summary>
        /// The center of the transform, as if it was a rectangle.
        /// </summary>
        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = (int) value.X - Width / 2;
                Y = (int) value.Y - Height / 2;

                OnMove?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The center of the transform, as if it was a rectangle, relative to its position.
        /// </summary>
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

        /// <summary>
        /// Create a new transform from a vec3 and a vec2.
        /// </summary>
        /// <param name="position">The position of the transform.</param>
        /// <param name="size">The size of the transform.</param>
        public Transform(Vector3 position, Vector2 size) : this(position.X, position.Y, position.Z, size.X, size.Y)
        {
        }

        /// <param name="x">The position of the transform on the X axis.</param>
        /// <param name="y">The position of the transform on the Y axis.</param>
        /// <param name="z">The position of the transform ont he Z axis.</param>
        /// <param name="width">The width of the transform.</param>
        /// <param name="height">The height of the transform.</param>
        public Transform(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f)
        {
            _x = x;
            _y = y;
            _z = z;
            _width = width;
            _height = height;
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

        /// <summary>
        /// Converts the transform to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[Position: {Position} | Size: {Size}]";
        }
    }
}