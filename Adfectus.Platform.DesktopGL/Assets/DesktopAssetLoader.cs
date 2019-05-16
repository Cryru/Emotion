using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Graphics.Text;
using Adfectus.IO;
using Adfectus.Sound;

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
            _customLoaders.Add(typeof(SoundFile), LoadSoundFile);
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

        private Asset LoadSoundFile()
        {
            return new ALSoundFile();
        }

        #endregion
    }
}
