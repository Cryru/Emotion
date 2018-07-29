// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public class Renderable2D
    {
        #region Base Properties

        public float X
        {
            get => _x;
            set
            {
                _x = value;
                _updateMatrix = true;
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                _y = value;
                _updateMatrix = true;
            }
        }

        public float Z
        {
            get => _z;
            set
            {
               _z = value;
                _updateMatrix = true;
            }
        }

        public float Width 
        {
            get => _width;
            set
            {
                _width = value;
                _updateMatrix = true;
            }
        }
        
        public float Height 
        {
            get => _height;
            set
            {
                _height = value;
                _updateMatrix = true;
            }
        }

        public Rectangle TextureArea = new Rectangle();

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

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _updateMatrix = true;
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

        #region Privates

        private float _x;
        private float _y;
        private float _z;
        private float _width;
        private float _height;

        private Renderable2D _parent;
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

                float xCenter = Width / 2;
                float yCenter = Height / 2;

                // Add rotation.
                matrix *= Matrix4.CreateTranslation(xCenter, yCenter, 1).Inverted() * Matrix4.CreateRotationZ(Rotation) * Matrix4.CreateTranslation(xCenter, yCenter, 1);

                // Add position.
                matrix *= Matrix4.CreateTranslation(X, Y, Z);

                _modelMatrix = matrix;
                _updateMatrix = false;

                return _modelMatrix;
            }
        }

        private Matrix4 _modelMatrix;
        private bool _updateMatrix = true;

        #endregion

        #region Vector3 Constructors

        public Renderable2D(Vector3 position, Vector2 size, Color color, float rotation = 0f, Renderable2D parent = null)
            : this(position.X, position.Y, position.Z, size.X, size.Y, color, rotation, parent)
        {

        }

        public Renderable2D(Vector3 position, Vector2 size) : this(position, size, Color.White)
        {

        }

        #endregion

        #region Vector2 Constructors

        public Renderable2D(Vector2 position, Vector2 size, Color color, float rotation = 0f, Renderable2D parent = null)
            : this(position.X, position.Y, 0, size.X, size.Y, color, rotation, parent)
        {

        }

        public Renderable2D(Vector2 position, Vector2 size) : this(position, size, Color.White)
        {

        }

        #endregion

        #region Raw Constructors

        public Renderable2D() : this(0, 0, 0, 0, 0, Color.White)
        {

        }

        public Renderable2D(Rectangle bounds)
        {
            Bounds = bounds;
            Color = Color.White;
            Rotation = 0f;
            Parent = null;
        }

        public Renderable2D(float x, float y, float z, float width, float height) : this(x, y, z, width, height, Color.White)
        {

        }

        public Renderable2D(float x, float y, float z, float width, float height, Color color, float rotation = 0f, Renderable2D parent = null)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Color = color;
            Rotation = rotation;
            Parent = parent;
        }

        #endregion
    }
}