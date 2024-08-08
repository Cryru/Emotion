#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiPlatformImeData
    {
        public byte WantVisible;
        public Vector2 InputPos;
        public float InputLineHeight;
    }

    public unsafe struct ImGuiPlatformImeDataPtr
    {
        public ImGuiPlatformImeData* NativePtr { get; }

        public ImGuiPlatformImeDataPtr(ImGuiPlatformImeData* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiPlatformImeDataPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiPlatformImeData*) nativePtr;
        }

        public static implicit operator ImGuiPlatformImeDataPtr(ImGuiPlatformImeData* nativePtr)
        {
            return new ImGuiPlatformImeDataPtr(nativePtr);
        }

        public static implicit operator ImGuiPlatformImeData*(ImGuiPlatformImeDataPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiPlatformImeDataPtr(IntPtr nativePtr)
        {
            return new ImGuiPlatformImeDataPtr(nativePtr);
        }

        public ref bool WantVisible
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->WantVisible);
        }

        public ref Vector2 InputPos
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->InputPos);
        }

        public ref float InputLineHeight
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->InputLineHeight);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiPlatformImeData_destroy(NativePtr);
        }
    }
}