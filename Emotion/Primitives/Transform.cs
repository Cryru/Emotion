#region Using

using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Game.QuadTree;
using Emotion.Utility;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// An object with a position and size.
    /// Named transform for more traditional than rational-model matrix reasons.
    /// </summary>
    public class Transform : Positional, IQuadTreeObject
    {
        #region Properties

        /// <summary>
        /// The width of the transform.
        /// </summary>
        [DontSerialize]
        public float Width
        {
            get => _width;
            set
            {
                if (_width == value) return;

                _width = value;
                Resized();
            }
        }

        /// <summary>
        /// The height of the transform.
        /// (Y in 2D and Z in 3D)
        /// </summary>
        [DontSerialize]
        public float Height
        {
            get => _height;
            set
            {
                if (_height == value) return;

                _height = value;
                Resized();
            }
        }

        /// <summary>
        /// The depth of the transform.
        /// (Z in 2D and Y in 3D)
        /// </summary>
        public float Depth
        {
            get => _depth;
            set
            {
                if (_depth == value) return;

                _depth = value;
                Resized();
            }
        }

        #endregion

        #region Higher Properties

        /// <summary>
        /// The size of the transform.
        /// This is the property that gets serialized.
        /// </summary>
        public virtual Vector2 Size
        {
            get => new Vector2(_width, _height);
            set
            {
                if (_width == value.X && _height == value.Y) return;

                _width = value.X;
                _height = value.Y;

                Resized();
            }
        }

        /// <summary>
        /// The scale of the transform in 3D.
        /// Width, Depth, Height
        /// </summary>
        [DontSerialize]
        public Vector3 Size3D
        {
            get => new Vector3(_width, _depth, _height);
            set
            {
                if (_width == value.X && _depth == value.Y && _height == value.Z)
                    return;

                _width = value.X;
                _depth = value.Y;
                _height = value.Z;
                Resized();
            }
        }

        /// <summary>
        /// The center of the transform, as if it was a rectangle.
        /// </summary>
        [DontSerialize]
        public virtual Vector2 Center
        {
            get => new Vector2(_x + _width / 2, _y + _height / 2);
            set
            {
                _x = value.X - _width / 2;
                _y = value.Y - _height / 2;

                Moved();
            }
        }

        /// <summary>
        /// The center of the transform, as if it was a rectangle, relative to its position.
        /// </summary>
        [DontSerialize]
        public virtual Vector2 CenterRelative
        {
            get => new Vector2(_width / 2, _height / 2);
        }

        /// <summary>
        /// The rectangle bounding the transform.
        /// </summary>
        [DontSerialize]
        public virtual Rectangle Bounds
        {
            get => new Rectangle(_x, _y, _width, _height);
            set
            {
                _x = value.X;
                _y = value.Y;
                _width = value.Width;
                _height = value.Height;

                Moved();
                Resized();
            }
        }

        #endregion

        #region Rotation

        /// <summary>
        /// The rotation of the transform in radians.
        /// </summary>
        [DontShowInEditor]
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                _rotation = value;
                Rotated();
            }
        }

        /// <summary>
        /// The rotation of the transform in decimal degrees.
        /// </summary>
        [DontSerialize]
        public Vector3 RotationDeg
        {
            get => new Vector3(Maths.RadiansToDegrees(_rotation.X), Maths.RadiansToDegrees(_rotation.Y), Maths.RadiansToDegrees(_rotation.Z));
            set
            {
                _rotation = new Vector3(Maths.DegreesToRadians(value.X), Maths.DegreesToRadians(value.Y), Maths.DegreesToRadians(value.Z));
                Rotated();
            }
        }

        protected Vector3 _rotation;

        protected virtual void Rotated()
        {
            OnRotate?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is invoked when the transform's size is changed.
        /// </summary>
        public event EventHandler OnResize;

        public event EventHandler OnRotate;

        #endregion

        #region Private Holders

        protected float _width;
        protected float _height;
        protected float _depth;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Resized()
        {
            Assert(!float.IsNaN(Width));
            Assert(!float.IsNaN(Height));
            OnResize?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Converts the transform to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[Position: {Position} | Size: {Size}]";
        }

        // IQuadTreeObject
        public virtual Rectangle GetBoundsForQuadTree()
        {
            return Bounds;
        }
    }
}