// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A renderable object.
    /// </summary>
    public interface ITransformRenderable : IRenderable
    {
        /// <summary>
        /// The model matrix of the renderable.
        /// </summary>
        Matrix4x4 ModelMatrix { get; }

        /// <summary>
        /// The function which performs the rendering.
        /// </summary>
        void Render();
    }
}