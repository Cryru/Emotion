// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Platform.Base.Interfaces
{
    public interface IDestroyable
    {
        /// <summary>
        /// Cleanup resources used by the object.
        /// </summary>
        void Destroy();
    }
}