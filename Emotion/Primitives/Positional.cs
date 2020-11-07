#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// An object with a position.
    /// </summary>
    public class Positional
    {
        #region Properties

        /// <summary>
        /// The location of the object on the X-axis.
        /// </summary>
        [DontSerialize]
        public virtual float X
        {
            get => _x;
            set
            {
                if (_x == value) return;

                _x = value;
                Moved();
            }
        }

        /// <summary>
        /// The location of the object on the Y-axis.
        /// </summary>
        [DontSerialize]
        public virtual float Y
        {
            get => _y;
            set
            {
                if (_y == value) return;

                _y = value;
                Moved();
            }
        }

        /// <summary>
        /// The location of the object on the Z-axis.
        /// </summary>
        [DontSerialize]
        public virtual float Z
        {
            get => _z;
            set
            {
                if (_z == value) return;

                _z = value;
                Moved();
            }
        }

        #endregion

        #region Higher Properties

        /// <summary>
        /// The position within 2D space.
        /// </summary>
        [DontSerialize]
        public Vector2 Position2
        {
            get => new Vector2(_x, _y);
            set
            {
                if (_x == value.X && _y == value.Y) return;

                _x = value.X;
                _y = value.Y;

                Moved();
            }
        }

        /// <summary>
        /// The position within 3D space.
        /// </summary>
        public Vector3 Position
        {
            get => new Vector3(_x, _y, _z);
            set
            {
                if (_x == value.X && _y == value.Y && _z == value.Z) return;

                _x = value.X;
                _y = value.Y;
                _z = value.Z;

                Moved();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Is invoked when the position is changed.
        /// </summary>
        public event EventHandler OnMove;

        #endregion

        #region Private Holders

        protected float _x;
        protected float _y;
        protected float _z;

        #endregion

        #region Constructors

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        private Positional()
        {
        }

        /// <summary>
        /// Create a new position from a vec3.
        /// </summary>
        /// <param name="position">The position of the object.</param>
        public Positional(Vector3 position) : this(position.X, position.Y, position.Z)
        {
        }

        /// <param name="x">The position of the object on the X axis.</param>
        /// <param name="y">The position of the object on the Y axis.</param>
        /// <param name="z">The position of the object ont he Z axis.</param>
        public Positional(float x = 0f, float y = 0f, float z = 0f)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Moved()
        {
            OnMove?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Converts the object to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[Position: {Position}]";
        }
    }
}