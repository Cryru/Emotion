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

        #endregion

        #region Private Holders

        private float _x;
        private float _y;
        private float _z;
        private float _width;
        private float _height;

        protected bool _transformUpdated { get; set; } = true;

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

        public void FromRectangle(Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return $"[X:{X} Y:{Y} Z:{Z} Width:{Width} Height:{Height}]";
        }
    }
}