#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiOnceUponAFrame
    {
        public int RefFrame;
    }

    public unsafe struct ImGuiOnceUponAFramePtr
    {
        public ImGuiOnceUponAFrame* NativePtr { get; }

        public ImGuiOnceUponAFramePtr(ImGuiOnceUponAFrame* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiOnceUponAFramePtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiOnceUponAFrame*) nativePtr;
        }

        public static implicit operator ImGuiOnceUponAFramePtr(ImGuiOnceUponAFrame* nativePtr)
        {
            return new ImGuiOnceUponAFramePtr(nativePtr);
        }

        public static implicit operator ImGuiOnceUponAFrame*(ImGuiOnceUponAFramePtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiOnceUponAFramePtr(IntPtr nativePtr)
        {
            return new ImGuiOnceUponAFramePtr(nativePtr);
        }

        public ref int RefFrame
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->RefFrame);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiOnceUponAFrame_destroy(NativePtr);
        }
    }
}