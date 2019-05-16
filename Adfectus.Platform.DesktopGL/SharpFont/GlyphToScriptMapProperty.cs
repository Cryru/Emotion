#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// The data exchange structure for the glyph-to-script-map property.
    /// </summary>
    public class GlyphToScriptMapProperty
    {
        private GlyphToScriptMapPropertyRec rec;
        private Face face;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphToScriptMapProperty" /> class.
        /// </summary>
        /// <param name="face">The face to apply the property to.</param>
        public GlyphToScriptMapProperty(Face face)
        {
            Face = face;
        }

        internal GlyphToScriptMapProperty(GlyphToScriptMapPropertyRec rec, Face face)
        {
            this.rec = rec;
            this.face = face;
        }

        /// <summary>
        /// Gets or sets the associated face.
        /// </summary>
        public Face Face
        {
            get => face;

            set
            {
                face = value;
                rec.face = face.Reference;
            }
        }

        /// <summary>
        /// Gets or sets the associated map.
        /// </summary>
        public IntPtr Map
        {
            get => rec.map;

            set => rec.map = value;
        }

        internal GlyphToScriptMapPropertyRec Rec
        {
            get => rec;
        }
    }
}