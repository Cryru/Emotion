// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public abstract class TransformRenderable : Transform, IRenderable
    {
        public virtual Matrix4 ModelMatrix { get; protected set; } = Matrix4.Identity;

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

            // Recalculate the model matrix on movement.
            OnMove += (a, b) => SyncModelMatrix();
            SyncModelMatrix();
        }

        #endregion

        private void SyncModelMatrix()
        {
            ModelMatrix = Matrix4.CreateTranslation(Position);
        }

        public abstract void Render(Renderer renderer);
    }
}