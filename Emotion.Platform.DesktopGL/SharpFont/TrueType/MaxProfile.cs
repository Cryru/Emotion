#region Using

using System;
using SharpFont.TrueType.Internal;

#endregion

namespace SharpFont.TrueType
{
    /// <summary>
    /// The maximum profile is a table containing many max values which can be used to pre-allocate arrays. This
    /// ensures that no memory allocation occurs during a glyph load.
    /// </summary>
    /// <remarks>
    /// This structure is only used during font loading.
    /// </remarks>
    public class MaxProfile
    {
        #region Fields

        private IntPtr reference;
        private MaxProfileRec rec;

        #endregion

        #region Constructors

        internal MaxProfile(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the version number.
        /// </summary>
        public int Version
        {
            get => (int) rec.version;
        }

        /// <summary>
        /// Gets the number of glyphs in this TrueType font.
        /// </summary>

        public ushort GlyphCount
        {
            get => rec.numGlyphs;
        }

        /// <summary>
        /// Gets the maximum number of points in a non-composite TrueType glyph. See also the structure element
        /// ‘maxCompositePoints’.
        /// </summary>

        public ushort MaxPoints
        {
            get => rec.maxPoints;
        }

        /// <summary>
        /// Gets the maximum number of contours in a non-composite TrueType glyph. See also the structure element
        /// ‘maxCompositeContours’.
        /// </summary>

        public ushort MaxContours
        {
            get => rec.maxContours;
        }

        /// <summary>
        /// Gets the maximum number of points in a composite TrueType glyph. See also the structure element
        /// ‘maxPoints’.
        /// </summary>

        public ushort MaxCompositePoints
        {
            get => rec.maxCompositePoints;
        }

        /// <summary>
        /// Gets the maximum number of contours in a composite TrueType glyph. See also the structure element
        /// ‘maxContours’.
        /// </summary>

        public ushort MaxCompositeContours
        {
            get => rec.maxCompositeContours;
        }

        /// <summary>
        /// Gets the maximum number of zones used for glyph hinting.
        /// </summary>

        public ushort MaxZones
        {
            get => rec.maxZones;
        }

        /// <summary>
        /// Gets the maximum number of points in the twilight zone used for glyph hinting.
        /// </summary>

        public ushort MaxTwilightPoints
        {
            get => rec.maxTwilightPoints;
        }

        /// <summary>
        /// Gets the maximum number of elements in the storage area used for glyph hinting.
        /// </summary>

        public ushort MaxStorage
        {
            get => rec.maxStorage;
        }

        /// <summary>
        /// Gets the maximum number of function definitions in the TrueType bytecode for this font.
        /// </summary>

        public ushort MaxFunctionDefs
        {
            get => rec.maxFunctionDefs;
        }

        /// <summary>
        /// Gets the maximum number of instruction definitions in the TrueType bytecode for this font.
        /// </summary>

        public ushort MaxInstructionDefs
        {
            get => rec.maxInstructionDefs;
        }

        /// <summary>
        /// Gets the maximum number of stack elements used during bytecode interpretation.
        /// </summary>

        public ushort MaxStackElements
        {
            get => rec.maxStackElements;
        }

        /// <summary>
        /// Gets the maximum number of TrueType opcodes used for glyph hinting.
        /// </summary>

        public ushort MaxSizeOfInstructions
        {
            get => rec.maxSizeOfInstructions;
        }

        /// <summary>
        /// Gets the maximum number of simple (i.e., non- composite) glyphs in a composite glyph.
        /// </summary>

        public ushort MaxComponentElements
        {
            get => rec.maxComponentElements;
        }

        /// <summary>
        /// Gets the maximum nesting depth of composite glyphs.
        /// </summary>

        public ushort MaxComponentDepth
        {
            get => rec.maxComponentDepth;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<MaxProfileRec>(reference);
            }
        }

        #endregion
    }
}