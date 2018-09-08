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
    }
}