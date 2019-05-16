using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Graphics.Text;
using Adfectus.IO;

namespace Adfectus.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public sealed class DesktopAssetLoader : AssetLoader
    {
        /// <inheritdoc />
        public DesktopAssetLoader(EngineBuilder builder)
        {
            // Create a source for the base assets folder.
            AddSource(new FileAssetSource(builder.AssetFolder));

            // Add custom loaders.
            _customLoaders.Add(typeof(Texture), LoadTexture);
            _customLoaders.Add(typeof(Font), LoadFont);
        }

        public DesktopAssetLoader(AssetSource[] sources) : base(sources)
        {

        }

        #region Loaders

        private Asset LoadTexture()
        {
            return new GLTexture();
        }

        private Asset LoadFont()
        {
            return new FreeTypeFont();
        }

        #endregion
    }
}
