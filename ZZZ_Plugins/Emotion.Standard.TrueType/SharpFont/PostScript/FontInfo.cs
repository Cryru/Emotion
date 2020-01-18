#region Using

using SharpFont.PostScript.Internal;

#endregion

namespace SharpFont.PostScript
{
    /// <summary>
    /// A structure used to model a Type 1 or Type 2 FontInfo dictionary. Note that for Multiple Master fonts, each
    /// instance has its own FontInfo dictionary.
    /// </summary>
    public class FontInfo
    {
        #region Fields

        private FontInfoRec rec;

        #endregion

        #region Constructors

        internal FontInfo(FontInfoRec rec)
        {
            this.rec = rec;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The version of the font.
        /// </summary>
        public string Version
        {
            get => rec.version;
        }

        /// <summary>
        /// The copyright notice for the font.
        /// </summary>
        public string Notice
        {
            get => rec.notice;
        }

        /// <summary>
        /// Gets the font's full name.
        /// </summary>
        public string FullName
        {
            get => rec.full_name;
        }

        /// <summary>
        /// Gets the font's family name.
        /// </summary>
        public string FamilyName
        {
            get => rec.family_name;
        }

        /// <summary>
        /// Gets the weight description of the font
        /// </summary>
        public string Weight
        {
            get => rec.weight;
        }

        /// <summary>
        /// Gets italic angle of the font.
        /// </summary>
        public int ItalicAngle
        {
            get => (int) rec.italic_angle;
        }

        /// <summary>
        /// Gets whether the font is fixed pitch.
        /// </summary>
        public bool IsFixedPitch
        {
            get => rec.is_fixed_pitch == 1;
        }

        /// <summary>
        /// Gets the position of the  underline.
        /// </summary>
        public short UnderlinePosition
        {
            get => rec.underline_position;
        }

        /// <summary>
        /// Gets the thickness of the underline stroke.
        /// </summary>

        public ushort UnderlineThickness
        {
            get => rec.underline_thickness;
        }

        #endregion
    }
}