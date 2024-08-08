#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImFontAtlasCustomRect
    {
        public ushort Width;
        public ushort Height;
        public ushort X;
        public ushort Y;
        public uint GlyphID;
        public float GlyphAdvanceX;
        public Vector2 GlyphOffset;
        public ImFont* Font;
    }

    public unsafe struct ImFontAtlasCustomRectPtr
    {
        public ImFontAtlasCustomRect* NativePtr { get; }

        public ImFontAtlasCustomRectPtr(ImFontAtlasCustomRect* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImFontAtlasCustomRectPtr(IntPtr nativePtr)
        {
            NativePtr = (ImFontAtlasCustomRect*) nativePtr;
        }

        public static implicit operator ImFontAtlasCustomRectPtr(ImFontAtlasCustomRect* nativePtr)
        {
            return new ImFontAtlasCustomRectPtr(nativePtr);
        }

        public static implicit operator ImFontAtlasCustomRect*(ImFontAtlasCustomRectPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImFontAtlasCustomRectPtr(IntPtr nativePtr)
        {
            return new ImFontAtlasCustomRectPtr(nativePtr);
        }

        public ref ushort Width
        {
            get => ref Unsafe.AsRef<ushort>(&NativePtr->Width);
        }

        public ref ushort Height
        {
            get => ref Unsafe.AsRef<ushort>(&NativePtr->Height);
        }

        public ref ushort X
        {
            get => ref Unsafe.AsRef<ushort>(&NativePtr->X);
        }

        public ref ushort Y
        {
            get => ref Unsafe.AsRef<ushort>(&NativePtr->Y);
        }

        public ref uint GlyphID
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->GlyphID);
        }

        public ref float GlyphAdvanceX
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->GlyphAdvanceX);
        }

        public ref Vector2 GlyphOffset
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->GlyphOffset);
        }

        public ImFontPtr Font
        {
            get => new ImFontPtr(NativePtr->Font);
        }

        public void Destroy()
        {
            ImGuiNative.ImFontAtlasCustomRect_destroy(NativePtr);
        }

        public bool IsPacked()
        {
            byte ret = ImGuiNative.ImFontAtlasCustomRect_IsPacked(NativePtr);
            return ret != 0;
        }
    }
}