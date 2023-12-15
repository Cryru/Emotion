#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImFontGlyph
    {
        public uint Colored;
        public uint Visible;
        public uint Codepoint;
        public float AdvanceX;
        public float X0;
        public float Y0;
        public float X1;
        public float Y1;
        public float U0;
        public float V0;
        public float U1;
        public float V1;
    }

    public unsafe struct ImFontGlyphPtr
    {
        public ImFontGlyph* NativePtr { get; }

        public ImFontGlyphPtr(ImFontGlyph* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImFontGlyphPtr(IntPtr nativePtr)
        {
            NativePtr = (ImFontGlyph*) nativePtr;
        }

        public static implicit operator ImFontGlyphPtr(ImFontGlyph* nativePtr)
        {
            return new ImFontGlyphPtr(nativePtr);
        }

        public static implicit operator ImFontGlyph*(ImFontGlyphPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImFontGlyphPtr(IntPtr nativePtr)
        {
            return new ImFontGlyphPtr(nativePtr);
        }

        public ref uint Colored
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->Colored);
        }

        public ref uint Visible
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->Visible);
        }

        public ref uint Codepoint
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->Codepoint);
        }

        public ref float AdvanceX
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->AdvanceX);
        }

        public ref float X0
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->X0);
        }

        public ref float Y0
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->Y0);
        }

        public ref float X1
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->X1);
        }

        public ref float Y1
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->Y1);
        }

        public ref float U0
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->U0);
        }

        public ref float V0
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->V0);
        }

        public ref float U1
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->U1);
        }

        public ref float V1
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->V1);
        }
    }
}