#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImDrawCmd
    {
        public Vector4 ClipRect;
        public IntPtr TextureId;
        public uint VtxOffset;
        public uint IdxOffset;
        public uint ElemCount;
        public IntPtr UserCallback;
        public void* UserCallbackData;
    }

    public unsafe struct ImDrawCmdPtr
    {
        public ImDrawCmd* NativePtr { get; }

        public ImDrawCmdPtr(ImDrawCmd* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImDrawCmdPtr(IntPtr nativePtr)
        {
            NativePtr = (ImDrawCmd*) nativePtr;
        }

        public static implicit operator ImDrawCmdPtr(ImDrawCmd* nativePtr)
        {
            return new ImDrawCmdPtr(nativePtr);
        }

        public static implicit operator ImDrawCmd*(ImDrawCmdPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImDrawCmdPtr(IntPtr nativePtr)
        {
            return new ImDrawCmdPtr(nativePtr);
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

        public ref uint IdxOffset
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->IdxOffset);
        }

        public ref uint ElemCount
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->ElemCount);
        }

        public ref IntPtr UserCallback
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->UserCallback);
        }

        public IntPtr UserCallbackData
        {
            get => (IntPtr) NativePtr->UserCallbackData;
            set => NativePtr->UserCallbackData = (void*) value;
        }

        public void Destroy()
        {
            ImGuiNative.ImDrawCmd_destroy(NativePtr);
        }

        public IntPtr GetTexID()
        {
            IntPtr ret = ImGuiNative.ImDrawCmd_GetTexID(NativePtr);
            return ret;
        }
    }
}