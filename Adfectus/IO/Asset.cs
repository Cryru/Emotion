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
        /// Whether the asset is created.
        /// </summary>
        public bool Created { get; protected set; }

        /// <summary>
        /// Whether the asset is disposed.
        /// </summary>
        public bool Disposed { get; protected set; }

        /// <summary>
        /// Create an asset from bytes.
        /// </summary>
        /// <param name="data">The bytes to create an asset from.</param>
        public void Create(byte[] data)
        {
            if (Created) return;

            CreateInternal(data);
            Created = true;
        }

        protected abstract void CreateInternal(byte[] data);

        /// <summary>
        /// Dispose of the asset clearing any resources it used.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;

            DisposeInternal();
            Disposed = true;
        }

        protected abstract void DisposeInternal();

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