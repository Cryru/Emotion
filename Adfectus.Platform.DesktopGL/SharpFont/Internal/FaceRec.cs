#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a Face.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="Face" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FaceRec
    {
        internal FT_Long num_faces;
        internal FT_Long face_index;

        internal FT_Long face_flags;
        internal FT_Long style_flags;

        internal FT_Long num_glyphs;

        internal FT_Long family_name;
        internal FT_Long style_name;

        internal int num_fixed_sizes;
        internal FT_Long available_sizes;

        internal int num_charmaps;
        internal FT_Long charmaps;

        internal GenericRec generic;

        internal BBox bbox;

        internal ushort units_per_EM;
        internal short ascender;
        internal short descender;
        internal short height;

        internal short max_advance_width;
        internal short max_advance_height;

        internal short underline_position;
        internal short underline_thickness;

        internal FT_Long glyph;
        internal FT_Long size;
        internal FT_Long charmap;

        private FT_Long driver;
        private FT_Long memory;
        private FT_Long stream;

        private FT_Long sizes_list;
        private GenericRec autohint;
        private FT_Long extensions;

        private FT_Long @internal;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<FaceRec>();
        }
    }
}