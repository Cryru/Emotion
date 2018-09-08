// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Primitives
{
    public class Point
    {
        #region Properties

        public float X
        {
            get => _x;
            set
            {
                _x = value;
                _pointUpdated = true;
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                _y = value;
                _pointUpdated = true;
            }
        }

        public float Z
        {
            get => _z;
            set
            {
                _z = value;
                _pointUpdated = true;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _pointUpdated = true;
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

        #endregion

        #region Private Holders

        private float _x;
        private float _y;
        private float _z;
        private float _rotation;

        protected bool _pointUpdated { get; set; } = true;

        #endregion

        #region Constructors

        public Point(Vector3 position, float rotation = 0f) : this(position.X, position.Y, position.Z, rotation)
        {
        }

        public Point(Vector2 position, float rotation = 0f) : this(position.X, position.Y, 0, rotation)
        {
        }

        public Point(float x = 0f, float y = 0f, float z = 0f, float rotation = 0f)
        {
            X = x;
            Y = y;
            Z = z;
            Rotation = rotation;
        }

        #endregion
    }
}