#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImColor
    {
        public Vector4 Value;
    }

    public unsafe struct ImColorPtr
    {
        public ImColor* NativePtr { get; }

        public ImColorPtr(ImColor* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImColorPtr(IntPtr nativePtr)
        {
            NativePtr = (ImColor*) nativePtr;
        }

        public static implicit operator ImColorPtr(ImColor* nativePtr)
        {
            return new ImColorPtr(nativePtr);
        }

        public static implicit operator ImColor*(ImColorPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImColorPtr(IntPtr nativePtr)
        {
            return new ImColorPtr(nativePtr);
        }

        public ref Vector4 Value
        {
            get => ref Unsafe.AsRef<Vector4>(&NativePtr->Value);
        }

        public void Destroy()
        {
            ImGuiNative.ImColor_destroy(NativePtr);
        }

        public ImColor HSV(float h, float s, float v)
        {
            ImColor __retval;
            float a = 1.0f;
            ImGuiNative.ImColor_HSV(&__retval, h, s, v, a);
            return __retval;
        }

        public ImColor HSV(float h, float s, float v, float a)
        {
            ImColor __retval;
            ImGuiNative.ImColor_HSV(&__retval, h, s, v, a);
            return __retval;
        }

        public void SetHSV(float h, float s, float v)
        {
            float a = 1.0f;
            ImGuiNative.ImColor_SetHSV(NativePtr, h, s, v, a);
        }

        public void SetHSV(float h, float s, float v, float a)
        {
            ImGuiNative.ImColor_SetHSV(NativePtr, h, s, v, a);
        }
    }
}