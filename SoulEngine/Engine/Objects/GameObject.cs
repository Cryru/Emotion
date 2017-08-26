// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Raya.Graphics;
using Raya.Graphics.Primitives;
using Raya.System;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine.Objects
{
    public class GameObject : Actor
    {
        #region Properties

        /// <summary>
        /// The object's size.
        /// </summary>
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                _updateTransform = true;
                onSizeChanged?.Invoke();
            }
        }

        private Vector2 _size = new Vector2(10, 10);

        /// <summary>
        /// The object's position.
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _updateTransform = true;
                onPositionChanged?.Invoke();
            }
        }

        private Vector2 _position = new Vector2(0, 0);

        /// <summary>
        /// The object's origin.
        /// </summary>
        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                _updateTransform = true;
            }
        }

        private Vector2 _origin = new Vector2(0, 0);


        /// <summary>
        /// The object's rotation in radians.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _updateTransform = true;
            }
        }

        private float _rotation;

        /// <summary>
        /// The object's rotation in degrees.
        /// </summary>
        public int RotationDegree
        {
            get { return (int) Helpers.ToDegrees(Rotation); }
            set { Rotation = Helpers.ToRadians(value); }
        }

        /// <summary>
        /// The object's position, rotation, and origin represented as a transform.
        /// </summary>
        public Transform Transform
        {
            get
            {
                // If we don't need to update the transform, return it straight away.
                if (!_updateTransform) return _transform;

                // Otherwise update the transform.
                _updateTransform = false;

                float cosine = (float) Math.Cos(_rotation);
                float sine = (float) Math.Sin(_rotation);
                float sxc = 1 * cosine;
                float syc = 1 * cosine;
                float sxs = 1 * sine;
                float sys = 1 * sine;
                float tx = -Origin.X * sxc - Origin.Y * sys + Position.X;
                float ty = Origin.X * sxs - Origin.Y * syc + Position.Y;

                _transform = new Transform(sxc, sys, tx,
                    -sxs, syc, ty,
                    0.0F, 0.0F, 1.0F);

                return _transform;
            }
        }

        private Transform _transform;

        #endregion

        #region Initial

        /// <summary>
        /// Whether the transform needs updating.
        /// </summary>
        private bool _updateTransform = true;

        #endregion

        public override void Initialize()
        {
        }

        public override void Update()
        {
        }

        #region Events

        /// <summary>
        /// The object's size has changed.
        /// </summary>
        public event Action onSizeChanged;

        /// <summary>
        /// The object's location has changed.
        /// </summary>
        public event Action onPositionChanged;

        /// <summary>
        /// The object's rotation has changed.
        /// </summary>
        public event Action onRotationChanged;

        /// <summary>
        /// The object's color has changed.
        /// </summary>
        public event Action onColorChanged;

        #endregion
    }
}