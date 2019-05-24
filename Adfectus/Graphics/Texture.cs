#region Using

using System.Numerics;
using Adfectus.Common;
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
        /// The texture matrix used to convert UVs.
        /// </summary>
        public Matrix4x4 TextureMatrix { get; protected set; }

        /// <summary>
        /// The size of the texture.
        /// </summary>
        public Vector2 Size { get; protected set; }

        /// <summary>
        /// Whether to smooth the texture.
        /// </summary>
        public bool Smooth
        {
            get => _smooth;
            set
            {
                GLThread.ExecuteGLThread(() =>
                {
                    Engine.GraphicsManager.BindTexture(this);
                    Engine.GraphicsManager.SetTextureSmooth(value);
                });
                _smooth = value;
            }
        }

        private bool _smooth = false;

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

        /// <summary>
        /// Return this texture but with a matrix multiplied by the provided matrix.
        /// </summary>
        /// <param name="mat">The matrix to multiply the current matrix with.</param>
        /// <param name="replace">Whether to replace the matrix instead of multiplying it.</param>
        /// <returns>An instance of this texture, but with the texture multiplied.</returns>
        public virtual Texture ModifyMatrix(Matrix4x4 mat, bool replace = false)
        {
            return this;
        }
    }
}