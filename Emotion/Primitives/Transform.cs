#region Using

using System;
using System.Numerics;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// An object with a position and size.
    /// </summary>
    public class Transform : Positional
    {
        #region Properties

        /// <summary>
        /// The width of the transform.
        /// </summary>
        [DontSerialize]
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
        [DontSerialize]
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
        /// The size of the transform.
        /// </summary>
        public virtual Vector2 Size
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
        /// The center of the transform, as if it was a rectangle.
        /// </summary>
        [DontSerialize]
        public virtual Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;

                Moved();
            }
        }

        /// <summary>
        /// The center of the transform, as if it was a rectangle, relative to its position.
        /// </summary>
        [DontSerialize]
        public virtual Vector2 CenterRelative
        {
            get => new Vector2(Width / 2, Height / 2);
        }

        /// <summary>
        /// The rectangle bounding the transform.
        /// </summary>
        [DontSerialize]
        public virtual Rectangle Bounds
        {
            get => new Rectangle(X, Y, Width, Height);
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Is invoked when the transform's size is changed.
        /// </summary>
        public event EventHandler<EventArgs> OnResize;

        #endregion

        #region Private Holders

        protected float _width;
        protected float _height;

        #endregion

        #region Constructors

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        private Transform()
        {

        }

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
        public Transform(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f) : base(x, y, z)
        {
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