#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImDrawData
    {
        public byte Valid;
        public int CmdListsCount;
        public int TotalIdxCount;
        public int TotalVtxCount;
        public ImDrawList** CmdLists;
        public Vector2 DisplayPos;
        public Vector2 DisplaySize;
        public Vector2 FramebufferScale;
        public ImGuiViewport* OwnerViewport;
    }

    public unsafe partial struct ImDrawDataPtr
    {
        public ImDrawData* NativePtr { get; }

        public ImDrawDataPtr(ImDrawData* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImDrawDataPtr(IntPtr nativePtr)
        {
            NativePtr = (ImDrawData*) nativePtr;
        }

        public static implicit operator ImDrawDataPtr(ImDrawData* nativePtr)
        {
            return new ImDrawDataPtr(nativePtr);
        }

        public static implicit operator ImDrawData*(ImDrawDataPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImDrawDataPtr(IntPtr nativePtr)
        {
            return new ImDrawDataPtr(nativePtr);
        }

        public ref bool Valid
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->Valid);
        }

        public ref int CmdListsCount
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->CmdListsCount);
        }

        public ref int TotalIdxCount
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->TotalIdxCount);
        }

        public ref int TotalVtxCount
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->TotalVtxCount);
        }

        public IntPtr CmdLists
        {
            get => (IntPtr) NativePtr->CmdLists;
            set => NativePtr->CmdLists = (ImDrawList**) value;
        }

        public ref Vector2 DisplayPos
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->DisplayPos);
        }

        public ref Vector2 DisplaySize
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->DisplaySize);
        }

        public ref Vector2 FramebufferScale
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->FramebufferScale);
        }

        public ImGuiViewportPtr OwnerViewport
        {
            get => new ImGuiViewportPtr(NativePtr->OwnerViewport);
        }

        public void Clear()
        {
            ImGuiNative.ImDrawData_Clear(NativePtr);
        }

        public void DeIndexAllBuffers()
        {
            ImGuiNative.ImDrawData_DeIndexAllBuffers(NativePtr);
        }

        public void Destroy()
        {
            ImGuiNative.ImDrawData_destroy(NativePtr);
        }

        public void ScaleClipRects(Vector2 fb_scale)
        {
            ImGuiNative.ImDrawData_ScaleClipRects(NativePtr, fb_scale);
        }
    }
}