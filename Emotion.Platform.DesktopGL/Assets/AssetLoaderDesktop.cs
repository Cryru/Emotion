using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Graphics.Text;
using Adfectus.IO;

namespace Emotion.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public sealed class AssetLoaderDesktop : AssetLoader
    {
        /// <inheritdoc />
        public AssetLoaderDesktop(EngineBuilder builder)
        {
            // Create a source for the base assets folder.
            AddSource(new FileAssetSource(builder.AssetFolder));

            // Add custom loaders.
            _customLoaders.Add(typeof(Texture), LoadTexture);
            _customLoaders.Add(typeof(Font), LoadFont);
        }

        #region Loaders

        private Asset LoadTexture()
        {
            return new TextureGL();
        }

        private Asset LoadFont()
        {
            return new FontGL();
        }

        #endregion
    }
}
