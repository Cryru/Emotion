#region Using

using System;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImGuiTextRange
    {
        public byte* b;
        public byte* e;
    }

    public unsafe struct ImGuiTextRangePtr
    {
        public ImGuiTextRange* NativePtr { get; }

        public ImGuiTextRangePtr(ImGuiTextRange* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiTextRangePtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiTextRange*) nativePtr;
        }

        public static implicit operator ImGuiTextRangePtr(ImGuiTextRange* nativePtr)
        {
            return new ImGuiTextRangePtr(nativePtr);
        }

        public static implicit operator ImGuiTextRange*(ImGuiTextRangePtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiTextRangePtr(IntPtr nativePtr)
        {
            return new ImGuiTextRangePtr(nativePtr);
        }

        public IntPtr b
        {
            get => (IntPtr) NativePtr->b;
            set => NativePtr->b = (byte*) value;
        }

        public IntPtr e
        {
            get => (IntPtr) NativePtr->e;
            set => NativePtr->e = (byte*) value;
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiTextRange_destroy(NativePtr);
        }

        public bool empty()
        {
            byte ret = ImGuiNative.ImGuiTextRange_empty(NativePtr);
            return ret != 0;
        }

        public void split(byte separator, out ImVector @out)
        {
            fixed (ImVector* native_out = &@out)
            {
                ImGuiNative.ImGuiTextRange_split(NativePtr, separator, native_out);
            }
        }
    }
}