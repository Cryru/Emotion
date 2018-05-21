// Emotion - https://github.com/Cryru/Emotion

#region Using

using OpenTK;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.GLES
{
    public interface ITexture
    {
        /// <summary>
        /// The texture's matrix for converting UVs to GL texture UVs.
        /// </summary>
        Matrix4 TextureMatrix { get; }

        /// <summary>
        /// The size of the loaded texture.
        /// </summary>
        Vector2 Size { get; }

        /// <summary>
        /// The width of the texture in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the texture in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Bind the texture to be used for the next GL calls.
        /// </summary>
        void Use();

        /// <summary>
        /// Cleanup resources used by the texture.
        /// </summary>
        void Cleanup();
    }
}