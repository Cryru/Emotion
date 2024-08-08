#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImDrawVert
    {
        public Vector2 pos;
        public Vector2 uv;
        public uint col;
    }

    public unsafe struct ImDrawVertPtr
    {
        public ImDrawVert* NativePtr { get; }

        public ImDrawVertPtr(ImDrawVert* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImDrawVertPtr(IntPtr nativePtr)
        {
            NativePtr = (ImDrawVert*) nativePtr;
        }

        public static implicit operator ImDrawVertPtr(ImDrawVert* nativePtr)
        {
            return new ImDrawVertPtr(nativePtr);
        }

        public static implicit operator ImDrawVert*(ImDrawVertPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImDrawVertPtr(IntPtr nativePtr)
        {
            return new ImDrawVertPtr(nativePtr);
        }

        public ref Vector2 pos
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->pos);
        }

        public ref Vector2 uv
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->uv);
        }

        public ref uint col
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->col);
        }
    }
}