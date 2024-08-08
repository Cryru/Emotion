#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImDrawCmdHeader
    {
        public Vector4 ClipRect;
        public IntPtr TextureId;
        public uint VtxOffset;
    }

    public unsafe struct ImDrawCmdHeaderPtr
    {
        public ImDrawCmdHeader* NativePtr { get; }

        public ImDrawCmdHeaderPtr(ImDrawCmdHeader* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImDrawCmdHeaderPtr(IntPtr nativePtr)
        {
            NativePtr = (ImDrawCmdHeader*) nativePtr;
        }

        public static implicit operator ImDrawCmdHeaderPtr(ImDrawCmdHeader* nativePtr)
        {
            return new ImDrawCmdHeaderPtr(nativePtr);
        }

        public static implicit operator ImDrawCmdHeader*(ImDrawCmdHeaderPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImDrawCmdHeaderPtr(IntPtr nativePtr)
        {
            return new ImDrawCmdHeaderPtr(nativePtr);
        }

        public ref Vector4 ClipRect
        {
            get => ref Unsafe.AsRef<Vector4>(&NativePtr->ClipRect);
        }

        public ref IntPtr TextureId
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->TextureId);
        }

        public ref uint VtxOffset
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->VtxOffset);
        }
    }
}