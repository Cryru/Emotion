#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiStoragePair
    {
        public uint Key;
        public UnionValue Value;
    }

    public unsafe struct ImGuiStoragePairPtr
    {
        public ImGuiStoragePair* NativePtr { get; }

        public ImGuiStoragePairPtr(ImGuiStoragePair* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiStoragePairPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiStoragePair*) nativePtr;
        }

        public static implicit operator ImGuiStoragePairPtr(ImGuiStoragePair* nativePtr)
        {
            return new ImGuiStoragePairPtr(nativePtr);
        }

        public static implicit operator ImGuiStoragePair*(ImGuiStoragePairPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiStoragePairPtr(IntPtr nativePtr)
        {
            return new ImGuiStoragePairPtr(nativePtr);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct UnionValue
    {
        [FieldOffset(0)] public int ValueI32;
        [FieldOffset(0)] public float ValueF32;
        [FieldOffset(0)] public IntPtr ValuePtr;
    }
}