// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Graphics
{
    /// <summary>
    /// A renderable object.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// The position of the renderable on the Z axis. Used for sorting.
        /// </summary>
        float Z { get; }

        /// <summary>
        /// The function which performs the rendering.
        /// </summary>
        void Render();
    }
}