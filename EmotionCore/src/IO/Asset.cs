// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.IO
{
    public abstract class Asset
    {
        public string Name { get; set; } = "Unmanaged Asset";

        internal abstract void Create(byte[] data);
        internal abstract void Destroy();
    }
}