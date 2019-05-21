#region Using

using System.Collections.Generic;
using Adfectus.IO;

#endregion

namespace Adfectus.Graphics.Text
{
    /// <inheritdoc />
    public class Font : Asset
    {
        private byte[] _fontBytes;
        private Dictionary<uint, Atlas> _atlasCache = new Dictionary<uint, Atlas>();
        private Dictionary<uint, Atlas> _atlasCachePixelSize = new Dictionary<uint, Atlas>();

        /// <summary>
        /// Default constructor. Used by the Asset Loader.
        /// </summary>
        // ReSharper disable once PublicConstructorInAbstractClass
        // ReSharper disable once EmptyConstructor
        public Font()
        {

        }

        #region Asset API

        protected override void CreateInternal(byte[] data)
        {
            _fontBytes = data;
        }

        protected override void DisposeInternal()
        {
            // Destroy loaded atlases.
            foreach (KeyValuePair<uint, Atlas> atlas in _atlasCache)
            {
                atlas.Value.Dispose();
            }

            _atlasCache.Clear();
        }

        #endregion

        /// <summary>
        /// Returns a font atlas of the specified size and which the specified glyphs loaded. Loaded atlases won't be reloaded.
        /// </summary>
        /// <param name="fontSize">The font size of the atlas to return.</param>
        /// <param name="glyphs">The number of glyphs to load. By default loads letters and grammar - the first 128.</param>
        /// <param name="charSize">
        /// Whether to create the characters in char size, on by default. If off the font size passed will
        /// be the pixel size instead.
        /// </param>
        /// <returns>A font atlas.</returns>
        public Atlas GetFontAtlas(uint fontSize, int glyphs = 128, bool charSize = true)
        {
            // Check if loading a char size or a pixel size font.
            if (charSize)
            {
                if (!_atlasCache.ContainsKey(fontSize)) _atlasCache[fontSize] = CreateAtlas(_fontBytes, fontSize, glyphs, true);
                return _atlasCache[fontSize];
            }

            if (!_atlasCachePixelSize.ContainsKey(fontSize)) _atlasCachePixelSize[fontSize] = CreateAtlas(_fontBytes, fontSize, glyphs, false);
            return _atlasCachePixelSize[fontSize];
        }

        protected virtual Atlas CreateAtlas(byte[] fontBytes, uint fontSize, int glyphs, bool charSize)
        {
            // no-op
            return null;
        }
    }
}