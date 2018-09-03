// Emotion - https://github.com/Cryru/Emotion

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
                _x = value;
                _transformUpdated = true;
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                _y = value;
                _transformUpdated = true;
            }
        }

        public float Z
        {
            get => _z;
            set
            {
                _z = value;
                _transformUpdated = true;
            }
        }

        public float Width
        {
            get => _width;
            set
            {
                _width = value;
                _transformUpdated = true;
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                _transformUpdated = true;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _transformUpdated = true;
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

        public Rectangle Bounds
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

        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }

        #endregion

        #region Private Holders

        private float _x;
        private float _y;
        private float _z;
        private float _width;
        private float _height;
        private float _rotation;

        protected bool _transformUpdated { get; set; } = true;

        #endregion

        #region Constructors

        public Transform(Rectangle bounds, float rotation = 0f) : this(bounds.Location, bounds.Size, rotation)
        {
        }

        public Transform(Vector3 position, Vector2 size, float rotation = 0f) : this(position.X, position.Y, position.Z, size.X, size.Y, rotation)
        {
        }

        public Transform(Vector2 position, Vector2 size, float rotation = 0f) : this(position.X, position.Y, 0, size.X, size.Y, rotation)
        {
        }

        public Transform(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f, float rotation = 0f)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Rotation = rotation;
        }

        #endregion
    }
}