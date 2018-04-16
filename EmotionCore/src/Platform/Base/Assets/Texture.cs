// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Platform.Base.Interfaces;

#endregion

namespace Emotion.Platform.Base.Assets
{
    /// <summary>
    /// An image loaded into memory which can be drawn to the screen. Managed by the platform's asset loader.
    /// </summary>
    public abstract class Texture : IDestroyable
    {
        #region Properties

        /// <summary>
        /// The width of the texture in pixels.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The height of the texture in pixels.
        /// </summary>
        public int Height { get; protected set; }

        #endregion

        public abstract void Destroy();
    }
}