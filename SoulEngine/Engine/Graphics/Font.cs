using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpFont;
using Soul.Engine.Modules;

namespace Soul.Engine.Graphics
{
    /// <summary>
    /// A font object, loaded by FreeType.
    /// </summary>
    public class Font
    {

        #region Public Data

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name;

        #endregion

        #region Data

        /// <summary>
        /// The font face.
        /// </summary>
        private Face _face;

        /// <summary>
        /// ??
        /// </summary>
        private Stroker _stroker;

        #endregion

        #region Cache

        /// <summary>
        /// 
        /// </summary>
        private int _characterSizeLast = 10;


        #endregion

        /// <summary>
        /// Create a font from a byte array representing a .ttf font.
        /// </summary>
        /// <param name="data">A byte array to create the font from</param>
        public Font(byte[] data)
        {
            _face = new Face(AssetLoader.FreeTypeLib, data, 0);
            Stroker stroker = new Stroker(AssetLoader.FreeTypeLib);
            _face.SelectCharmap(Encoding.Unicode);

            Name = _face.FamilyName;
            return;

            _face.LoadChar((int) 'A', LoadFlags.CropBitmap, LoadTarget.Normal);
            var a = _face.Glyph.Bitmap.BufferData;
        }

        public void GetGlyph(char charCode, uint characterSize)
        {
            LoadGlyph(charCode, characterSize);
        }

        #region Local

        private void LoadGlyph(char charCode, uint characterSize)
        {
            // Check if we have to set pixel size. This is a slow operation so we need to avoid calling it too often.
            if (_characterSizeLast != characterSize) _face.SetPixelSizes(0, characterSize);

            _face.LoadChar((uint) charCode, LoadFlags.ForceAutohint, LoadTarget.Normal);

            GlyphSlot freeTypeGlyph = _face.Glyph;
            freeTypeGlyph.RenderGlyph(RenderMode.Normal);

            // Convert the character to bitmap.
            FTBitmap freeTypeBitmap = _face.Glyph.Bitmap;

            if (freeTypeBitmap.Width > 0 && freeTypeBitmap.Rows > 0)
            {
                bool a = true;
            }

        }

        #endregion

        public void Dispose()
        {

        }

    }
}
