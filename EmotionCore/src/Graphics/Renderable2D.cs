// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class Renderable2D
    {
        #region Main Properties

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _updateMatrix = true;
            }
        }

        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                _updateMatrix = true;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _updateMatrix = true;
            }
        }

        public Renderable2D Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                _updateMatrix = true;
            }
        }

        public Color Color { get; set; }

        #endregion

        #region Higher Properties

        public Rectangle Bounds
        {
            get => new Rectangle(Position.Xy, Size);
            set
            {
                Position = new Vector3(value.X, value.Y, Position.Z);
                Size = value.Size;
            }
        }

        public Vector2 Center
        {
            get => new Vector2(_position.X + _size.X / 2, _position.Y + _size.Y / 2);
            set
            {
                _position.X = value.X - _size.X / 2;
                _position.Y = value.Y - _size.Y / 2;
                _updateMatrix = true;
            }
        }

        #endregion

        #region Simplified Properties

        public float X
        {
            get => _position.X;
            set
            {
                _position.X = value;
                _updateMatrix = true;
            }
        }

        public float Y
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                _updateMatrix = true;
            }
        }

        public float Z
        {
            get => _position.Z;
            set
            {
                _position.Z = value;
                _updateMatrix = true;
            }
        }

        public float Width 
        {
            get => _size.X;
            set
            {
                _size.X = value;
                _updateMatrix = true;
            }
        }
        
        public float Height 
        {
            get => _size.Y;
            set
            {
                _size.Y = value;
                _updateMatrix = true;
            }
        }

        public float Left
        {
            get => X;
            set => X = value;
        }

        public float Right
        {
            get => X + Width;
            set => X = value - Width;
        }

        public float Top
        {
            get => Y;
            set => Y = value;
        }

        public float Bottom
        {
            get => Y + Height;
            set => Y = value - Height;
        }

        #endregion

        #region Privates

        private Renderable2D _parent;
        private Vector3 _position;
        private Vector2 _size;
        private float _rotation;

        #endregion

        #region Matrix

        public Matrix4 ModelMatrix
        {
            get
            {
                // Check if we need to update the model matrix.
                if (!_updateMatrix) return _modelMatrix;

                Matrix4 matrix = Matrix4.Identity;

                // If the renderable has a parent, add its matrix first.
                if (_parent != null) matrix *= _parent.ModelMatrix;

                float xCenter = _size.X / 2;
                float yCenter = _size.Y / 2;

                // Add rotation.
                matrix *= Matrix4.CreateTranslation(xCenter, yCenter, 1).Inverted() * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(xCenter, yCenter, 1);

                // Add position.
                matrix *= Matrix4.CreateTranslation(_position);

                _modelMatrix = matrix;
                _updateMatrix = false;

                return _modelMatrix;
            }
        }

        private Matrix4 _modelMatrix;
        private bool _updateMatrix = true;

        #endregion

        public Renderable2D(Rectangle bounds)
        {
            Bounds = bounds;
            Color = Color.White;
            Rotation = 0f;
            Parent = null;
        }

        public Renderable2D(Vector3 position, Vector2 size, Color color, float rotation = 0f, Renderable2D parent = null)
        {
            Position = position;
            Size = size;
            Color = color;
            Rotation = rotation;
            Parent = parent;
        }
    }
}