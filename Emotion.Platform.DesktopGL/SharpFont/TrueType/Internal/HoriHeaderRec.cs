#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct HoriHeaderRec
    {
        internal FT_Long Version;
        internal short Ascender;
        internal short Descender;
        internal short Line_Gap;

        internal ushort advance_Width_Max;

        internal short min_Left_Side_Bearing;
        internal short min_Right_Side_Bearing;
        internal short xMax_Extent;
        internal short caret_Slope_Rise;
        internal short caret_Slope_Run;
        internal short caret_Offset;

        private fixed short reserved[4];

        internal short[] Reserved
        {
            get
            {
                short[] array = new short[4];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = reserved[i];
                }

                return array;
            }
        }

        internal short metric_Data_Format;
        internal ushort number_Of_HMetrics;

        internal FT_Long long_metrics;
        internal FT_Long short_metrics;
    }
}