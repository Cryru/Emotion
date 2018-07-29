// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class Renderable2D
    {
        #region Properties

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

        public Rectangle Bounds
        {
            get => new Rectangle(Position.Xy, Size);
            set
            {
                Position = new Vector3(value.X, value.Y, Position.Z);
                Size = value.Size;
            }
        }

        public Color Color { get; set; }

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

                float xCenter = _position.X + _size.X / 2;
                float yCenter = _position.X + _size.Y / 2;

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

        public Renderable2D(Vector3 position, Vector2 size, Color color, float rotation = 0f, Renderable2D parent = null)
        {
            Position = position;
            Size = size;
            Color = color;
            Rotation = rotation;
            Parent = parent;
        }

        /// <summary>
        /// Destroy the renderable freeing memory.
        /// </summary>
        protected void Destroy()
        {
        }
    }
}