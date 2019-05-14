#region Using

using System.Runtime.InteropServices;

#endregion

namespace SharpFont.MultipleMasters.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MultiMasterRec
    {
        internal uint num_axis;
        internal uint num_designs;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal MMAxisRec[] axis;
    }
}