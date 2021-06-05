#region Using

using System;
using Emotion.Common.Serialization;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// An asset file.
    /// </summary>
    [DontSerialize]
    public abstract class Asset
    {
        /// <summary>
        /// The name of the asset. If loaded from the AssetLoader this is the path of the asset.
        /// </summary>
        public string Name { get; set; } = "Unknown";

        /// <summary>
        /// The byte size of the asset when loaded from the asset source. Subsequent operations may cause the asset to
        /// take more space/less space. Such as compression etc.
        /// </summary>
        public int Size { get; set; }

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
        /// <returns>The bytes to cache for faster loading in the future, or null if no cache.</returns>
        public void Create(ReadOnlyMemory<byte> data)
        {
            if (Created) return;

            Size = data.Length;
            CreateInternal(data);
            Created = true;
        }

        protected abstract void CreateInternal(ReadOnlyMemory<byte> data);

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