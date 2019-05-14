#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal unsafe struct PCLTRec
    {
        internal FT_Long Version;
        internal FT_ULong FontNumber;
        internal ushort Pitch;
        internal ushort xHeight;
        internal ushort Style;
        internal ushort TypeFamily;
        internal ushort CapHeight;
        internal ushort SymbolSet;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        internal string TypeFace;

        private fixed byte characterComplement[8];

        internal byte[] CharacterComplement
        {
            get
            {
                byte[] array = new byte[8];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = characterComplement[i];
                }

                return array;
            }
        }

        private fixed byte fileName[6];

        internal byte[] FileName
        {
            get
            {
                byte[] array = new byte[6];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = fileName[i];
                }

                return array;
            }
        }

        internal byte StrokeWeight;
        internal byte WidthType;
        internal byte SerifStyle;
        internal byte Reserved;
    }
}