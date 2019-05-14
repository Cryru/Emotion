#region Using

using SharpFont.PostScript.Internal;

#endregion

namespace SharpFont.PostScript
{
    /// <summary>
    /// A structure used to model a Type 1 or Type 2 private dictionary. Note that for Multiple Master fonts, each
    /// instance has its own Private dictionary.
    /// </summary>
    public class Private
    {
        #region Fields

        private PrivateRec rec;

        #endregion

        #region Constructors

        internal Private(PrivateRec rec)
        {
            this.rec = rec;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID unique to the Type 1 font.
        /// </summary>
        public int UniqueId
        {
            get => rec.unique_id;
        }

        /// <summary>
        /// Gets the number of random bytes at the beginning of charstrings (for encryption).
        /// </summary>
        public int LenIV
        {
            get => rec.lenIV;
        }

        /// <summary>
        /// Gets the number of values (pairs) in the Blues array.
        /// </summary>
        public byte BlueValuesCount
        {
            get => rec.num_blue_values;
        }

        /// <summary>
        /// Gets the number of values (pairs) in the OtherBlues array.
        /// </summary>
        public byte OtherBluesCount
        {
            get => rec.num_other_blues;
        }

        /// <summary>
        /// Gets the number of values (pairs) in the FamilyBlues array.
        /// </summary>
        public byte FamilyBluesCount
        {
            get => rec.num_family_blues;
        }

        /// <summary>
        /// Gets the number of values (pairs) in the FamilyOtherBlues array.
        /// </summary>
        public byte FamilyOtherBluesCount
        {
            get => rec.num_family_other_blues;
        }

        /// <summary>
        /// Gets the pairs of blue values.
        /// </summary>
        public short[] BlueValues
        {
            get => rec.blue_values;
        }

        /// <summary>
        /// Gets the pairs of blue values.
        /// </summary>
        public short[] OtherBlues
        {
            get => rec.other_blues;
        }

        /// <summary>
        /// Gets the pairs of blue values.
        /// </summary>
        public short[] FamilyBlues
        {
            get => rec.family_blues;
        }

        /// <summary>
        /// Gets the pairs of blue values.
        /// </summary>
        public short[] FamilyOtherBlues
        {
            get => rec.family_other_blues;
        }

        /// <summary>
        /// Gets the point size at which overshoot suppression ceases.
        /// </summary>
        public int BlueScale
        {
            get => (int) rec.blue_scale;
        }

        /// <summary>
        /// Gets whether characters smaller than the size given by BlueScale
        /// should have overshoots suppressed.
        /// </summary>
        public int BlueShift
        {
            get => rec.blue_shift;
        }

        /// <summary>
        /// Gets the number of character space units to extend the effect of an
        /// alignment zone on a horizontal stem. Setting this to 0 is recommended
        /// because it is unreliable.
        /// </summary>
        public int BlueFuzz
        {
            get => rec.blue_fuzz;
        }

        /// <summary>
        /// Indicates the standard stroke width of vertical stems.
        /// </summary>

        public ushort StandardWidth
        {
            get => rec.standard_width;
        }

        /// <summary>
        /// Indicates the standard stroke width of horizontal stems.
        /// </summary>

        public ushort StandardHeight
        {
            get => rec.standard_height;
        }

        /// <summary>
        /// Indicates the number of values in the SnapWidths array.
        /// </summary>
        public byte SnapWidthsCount
        {
            get => rec.num_snap_widths;
        }

        /// <summary>
        /// Indicates the number of values in the SnapHeights array.
        /// </summary>
        public byte SnapHeightsCount
        {
            get => rec.num_snap_heights;
        }

        /// <summary>
        /// Gets whether bold characters should appear thicker than non-bold characters
        /// at very small point sizes, where otherwise bold characters might appear the
        /// same as non-bold characters.
        /// </summary>
        public bool ForceBold
        {
            get => rec.force_bold == 1;
        }

        /// <summary>
        /// Superseded by the LanguageGroup entry.
        /// </summary>
        public bool RoundStemUp
        {
            get => rec.round_stem_up == 1;
        }

        /// <summary>
        /// StemSnapH is an array of up to 12 values of the most common stroke widths for horizontal stems
        /// (measured vertically).
        /// </summary>
        public short[] SnapWidths
        {
            get => rec.snap_widths;
        }

        /// <summary>
        /// StemSnapV is an array of up to 12 values of the most common stroke widths for vertical stems
        /// (measured horizontally).
        /// </summary>
        public short[] SnapHeights
        {
            get => rec.snap_heights;
        }

        /// <summary>
        /// The Expansion Factor provides a limit for changing character bounding boxes during
        /// processing that adjusts the size of fonts of Language Group 1.
        /// </summary>
        public int ExpansionFactor
        {
            get => (int) rec.expansion_factor;
        }

        /// <summary>
        /// Indicates the aesthetic characteristics of the font. Currently, only LanguageGroup 0
        /// (e.g. Latin, Greek, Cyrillic, etc.) and LanguageGroup 1 (e.g. Chinese ideographs, Japanese
        /// Kanji, etc) are recognized.
        /// </summary>
        public int LanguageGroup
        {
            get => (int) rec.language_group;
        }

        /// <summary>
        /// The Password value is required for the Type 1 BuildChar to operate.
        /// It must be set to 5839.
        /// </summary>
        public int Password
        {
            get => (int) rec.password;
        }

        /// <summary>
        /// The MinFeature value is required for the Type 1 BuildChar to operate, but is obsolete.
        /// It must be set to {16,16}.
        /// </summary>
        public short[] MinFeature
        {
            get => rec.min_feature;
        }

        #endregion
    }
}