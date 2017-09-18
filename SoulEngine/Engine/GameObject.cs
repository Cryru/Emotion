// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Threading.Tasks;
using Raya.Graphics;
using Raya.Graphics.Primitives;
using Raya.System;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine
{
    public class GameObject : Actor
    {
        #region Transformative Properties

        /// <summary>
        /// The object's size.
        /// </summary>
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                if (_size == value) return;

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
                if (_position == value) return;

                _position = value;
                _updateTransform = true;
                onPositionChanged?.Invoke();
            }
        }

        private Vector2 _position = new Vector2(0, 0);

        /// <summary>
        /// The object's center.
        /// </summary>
        public Vector2 Center
        {
            get { return new Vector2(X + Width / 2, Y + Height / 2); }
            set
            {
                // Transform the location.
                value.X -= Width / 2;
                value.Y -= Height / 2;

                // Set the position.
                Position = value;
            }
        }

        /// <summary>
        /// The object's origin.
        /// </summary>
        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                if (_origin == value) return;

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
                if (_rotation == value) return;

                _rotation = value;
                _updateTransform = true;
                onRotationChanged?.Invoke();
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

                // Otherwise update the transform flag, and update the transform.
                _updateTransform = false;

                //Vector2 Origin = Center;
                //Vector2 Position = this.Position;

                //Position.X -= Origin.X;
                //Position.Y -= Origin.Y;


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

        #region Simplified Properties

        /// <summary>
        /// The position of the object within the X axis.
        /// </summary>
        public int X
        {
            get { return Position.X; }
            set
            {
                if (Position.X == value) return;

                Position = new Vector2(value, Y);
            }
        }

        /// <summary>
        /// The position of the object within the Y axis.
        /// </summary>
        public int Y
        {
            get { return Position.Y; }
            set
            {
                if (Position.Y == value) return;

                Position = new Vector2(X, value);
            }
        }

        /// <summary>
        /// The width of the object.
        /// </summary>
        public int Width
        {
            get { return Size.X; }
            set
            {
                if (Size.X == value) return;

                Size = new Vector2(value, Height);
            }
        }

        /// <summary>
        /// The height of the object.
        /// </summary>
        public int Height
        {
            get { return Size.Y; }
            set
            {
                if (Size.Y == value) return;

                Size = new Vector2(Width, value);
            }
        }

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

        public override void UpdateActor()
        {
            // If the object is outside of the screen don't update it.
            Rectangle Screen = Core.NativeContext.Window.Viewport;
            Screen.X -= 50;
            Screen.Y -= 50;
            Screen.Width += 100;
            Screen.Height += 100;
            if (Screen.IntersectsWith(Position))
            {
                base.UpdateActor();
            }
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

        #endregion
    }
}