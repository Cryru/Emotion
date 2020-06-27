namespace Emotion.IO
{
    /// <summary>
    /// A storage for assets. Save files, and user generated stuff.
    /// </summary>
    public interface IAssetStore
    {
        string Folder { get; }

        /// <summary>
        /// Save an asset to the disk.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="name">The engine name of the asset.</param>
        /// <param name="backup">Whether to backup the old asset if any.</param>
        void SaveAsset(byte[] data, string name, bool backup);
    }
}