// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public abstract class TransformRenderable : Transform
    {
        public virtual Matrix4 ModelMatrix
        {
            get
            {
                if (_transformUpdated)
                {
                    _modelMatrix = Matrix4.CreateTranslation(Position);
                    _transformUpdated = false;
                }

                return _modelMatrix;
            }
            protected set => _modelMatrix = value;
        }

        protected Matrix4 _modelMatrix = Matrix4.Identity;

        internal abstract void Render(Renderer renderer);

        #region Constructors

        protected TransformRenderable(Rectangle bounds) : this(bounds.Location, bounds.Size)
        {
        }

        protected TransformRenderable(Vector3 position, Vector2 size) : this(position.X, position.Y, position.Z, size.X, size.Y)
        {
        }

        protected TransformRenderable(Vector2 position, Vector2 size) : this(position.X, position.Y, 0, size.X, size.Y)
        {
        }

        protected TransformRenderable(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
        }

        #endregion
    }
}