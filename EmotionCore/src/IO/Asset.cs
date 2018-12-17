// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.IO
{
    /// <summary>
    /// An asset file.
    /// </summary>
    public abstract class Asset
    {
        /// <summary>
        /// The name of the asset. If loaded from the AssetLoader this is the path of the asset.
        /// </summary>
        public string Name { get; set; } = "Unknown";

        internal abstract void Create(byte[] data);
        internal abstract void Destroy();

        /// <summary>
        /// The hashcode of the asset. Derived from the name.
        /// </summary>
        /// <returns>The hashcode of the asset.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}