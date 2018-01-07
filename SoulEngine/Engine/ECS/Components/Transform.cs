// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

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
                _hasUpdated = true;
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
                _hasUpdated = true;
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
                _hasUpdated = true;
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
                _hasUpdated = true;
                _height = value;
            }
        }

        private float _height = 100;

        /// <summary>
        /// The transform's rotation in degrees.
        /// </summary>
        public int Rotation
        {
            get { return _rotation; }
            set
            {
                if (value > 360)
                {
                    value -= 360;
                }

                _hasUpdated = true;
                _rotation = value;
            }
        }

        private int _rotation = 0;

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
                _hasUpdated = true;
                _x = value.X;
                _y = value.Y;
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
                _hasUpdated = true;
                _width = value.X;
                _height = value.Y;
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
                Rectangle temp = Bounds;

                // Set the position by transforming it.
                temp.X -= (int)value.X - Bounds.Width / 2;
                temp.Y -= (int)value.Y - Bounds.Height / 2;

                Bounds = temp;
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
                _x = value.X;
                _y = value.Y;
                _width = value.Width;
                _height = value.Height;
            }
        }

        #endregion
    }
}