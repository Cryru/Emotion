#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ModuleClassRec
    {
        internal uint module_flags;
        internal FT_Long module_size;

        [MarshalAs(UnmanagedType.LPStr)] internal string module_name;
        internal FT_Long module_version;
        internal FT_Long module_requires;

        internal FT_Long module_interface;

        internal ModuleConstructor module_init;
        internal ModuleDestructor module_done;
        internal ModuleRequester get_interface;
    }
}