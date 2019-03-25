namespace Adfectus.IO
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

        /// <summary>
        /// Create the asset from a byte array.
        /// </summary>
        /// <param name="data">The byte array to create an asset from.</param>
        internal abstract void CreateAsset(byte[] data);

        /// <summary>
        /// Dispose of the asset and its used resources.
        /// </summary>
        internal abstract void DestroyAsset();

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