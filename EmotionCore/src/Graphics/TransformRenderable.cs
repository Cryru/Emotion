// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public abstract class TransformRenderable : Transform
    {
        public Matrix4 ModelMatrix { get; protected set; } = Matrix4.Identity;

        internal abstract void Render(Renderer renderer);
    }
}