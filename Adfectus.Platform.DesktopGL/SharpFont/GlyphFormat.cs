﻿namespace SharpFont
{
    /// <summary>
    /// An enumeration type used to describe the format of a given glyph image. Note that this version of FreeType only
    /// supports two image formats, even though future font drivers will be able to register their own format.
    /// </summary>
    public enum GlyphFormat : uint
    {
        /// <summary>
        /// The value 0 is reserved.
        /// </summary>
        None = 0,

        /// <summary>
        /// The glyph image is a composite of several other images. This format is only used with
        /// <see cref="LoadFlags.NoRecurse" />, and is used to report compound glyphs (like accented characters).
        /// </summary>
        Composite = ('c' << 24) | ('o' << 16) | ('m' << 8) | 'p',

        /// <summary>
        /// The glyph image is a bitmap, and can be described as an <see cref="FTBitmap" />. You generally need to
        /// access the ‘bitmap’ field of the <see cref="GlyphSlot" /> structure to read it.
        /// </summary>
        Bitmap = ('b' << 24) | ('i' << 16) | ('t' << 8) | 's',

        /// <summary>
        /// The glyph image is a vectorial outline made of line segments and Bézier arcs; it can be described as an
        /// <see cref="Outline" />; you generally want to access the ‘outline’ field of the <see cref="GlyphSlot" />
        /// structure to read it.
        /// </summary>
        Outline = ('o' << 24) | ('u' << 16) | ('t' << 8) | 'l',

        /// <summary>
        /// The glyph image is a vectorial path with no inside and outside contours. Some Type 1 fonts, like those in
        /// the Hershey family, contain glyphs in this format. These are described as <see cref="Outline" />, but
        /// FreeType isn't currently capable of rendering them correctly.
        /// </summary>
        Plotter = ('p' << 24) | ('l' << 16) | ('o' << 8) | 't'
    }
}