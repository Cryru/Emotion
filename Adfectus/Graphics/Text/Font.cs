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

        #region Asset API

        /// <summary>
        /// Default constructor. Used by the Asset Loader.
        /// </summary>
        // ReSharper disable once PublicConstructorInAbstractClass
        public Font()
        {

        }

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
        /// <returns>A font atlas.</returns>
        public Atlas GetFontAtlas(uint fontSize, int glyphs = 128)
        {
            // Cache the atlas if it isn't cached.
            if (!_atlasCache.ContainsKey(fontSize)) _atlasCache[fontSize] = CreateAtlas(_fontBytes, fontSize, glyphs);

            return _atlasCache[fontSize];
        }

        protected virtual Atlas CreateAtlas(byte[] fontBytes, uint fontSize, int glyphs)
        {
            // no-op
            return null;
        }
    }
}