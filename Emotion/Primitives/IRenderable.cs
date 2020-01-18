#region Using

using Emotion.Graphics;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A renderable object.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// The function which performs the rendering.
        /// </summary>
        void Render(RenderComposer composer);
    }
}