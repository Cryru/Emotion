// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Platform.Base.Interfaces
{
    public abstract class Destroyable
    {
        /// <summary>
        /// Whether the texture is destroyed.
        /// </summary>
        public bool Destroyed { get; protected set; }

        /// <summary>
        /// Cleanup resources used by the object.
        /// </summary>
        public abstract void Destroy();
    }
}