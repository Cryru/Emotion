#region Using

using System.Numerics;
using Adfectus.IO;

#endregion

namespace Adfectus.Graphics
{
    /// <inheritdoc />
    /// <summary>
    /// A asset of a texture object loaded from an image.
    /// </summary>
    public class Texture : Asset
    {
        #region Properties

        /// <summary>
        /// The size of the texture.
        /// </summary>
        public Vector2 Size { get; protected set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Default texture constructor. Used by the AssetLoader.
        /// </summary>
        // ReSharper disable once PublicConstructorInAbstractClass
        public Texture()
        {
            Size = new Vector2();
        }

        #endregion

        protected override void CreateInternal(byte[] data)
        {
            // no-op
        }

        protected override void DisposeInternal()
        {
            // no-op
        }

        /// <summary>
        /// Return the texture flipped horizontally.
        /// </summary>
        /// <returns>An instance of this texture, but flipped horizontally.</returns>
        public virtual Texture FlipHorizontal()
        {
            return this;
        }

        /// <summary>
        /// Return the texture flipped vertically.
        /// </summary>
        /// <returns>An instance of this texture, but flipped vertically.</returns>
        public virtual Texture FlipVertical()
        {
            return this;
        }
    }
}