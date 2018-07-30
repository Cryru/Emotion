// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.Legacy
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
        /// Bind the texture to be used for the next GL calls.
        /// </summary>
        void Bind();

        /// <summary>
        /// Cleanup resources used by the texture.
        /// </summary>
        void Delete();
    }
}