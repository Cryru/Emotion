#region Using

#endregion

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
        void SaveAsset(byte[] data, string name);
    }
}