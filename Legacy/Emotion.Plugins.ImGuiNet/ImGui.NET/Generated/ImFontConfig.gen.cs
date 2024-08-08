#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImFontConfig
    {
        public void* FontData;
        public int FontDataSize;
        public byte FontDataOwnedByAtlas;
        public int FontNo;
        public float SizePixels;
        public int OversampleH;
        public int OversampleV;
        public byte PixelSnapH;
        public Vector2 GlyphExtraSpacing;
        public Vector2 GlyphOffset;
        public ushort* GlyphRanges;
        public float GlyphMinAdvanceX;
        public float GlyphMaxAdvanceX;
        public byte MergeMode;
        public uint FontBuilderFlags;
        public float RasterizerMultiply;
        public ushort EllipsisChar;
        public fixed byte Name[40];
        public ImFont* DstFont;
    }

    public unsafe struct ImFontConfigPtr
    {
        public ImFontConfig* NativePtr { get; }

        public ImFontConfigPtr(ImFontConfig* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImFontConfigPtr(IntPtr nativePtr)
        {
            NativePtr = (ImFontConfig*) nativePtr;
        }

        public static implicit operator ImFontConfigPtr(ImFontConfig* nativePtr)
        {
            return new ImFontConfigPtr(nativePtr);
        }

        public static implicit operator ImFontConfig*(ImFontConfigPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImFontConfigPtr(IntPtr nativePtr)
        {
            return new ImFontConfigPtr(nativePtr);
        }

        public IntPtr FontData
        {
            get => (IntPtr) NativePtr->FontData;
            set => NativePtr->FontData = (void*) value;
        }

        public ref int FontDataSize
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->FontDataSize);
        }

        public ref bool FontDataOwnedByAtlas
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->FontDataOwnedByAtlas);
        }

        public ref int FontNo
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->FontNo);
        }

        public ref float SizePixels
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->SizePixels);
        }

        public ref int OversampleH
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->OversampleH);
        }

        public ref int OversampleV
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->OversampleV);
        }

        public ref bool PixelSnapH
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->PixelSnapH);
        }

        public ref Vector2 GlyphExtraSpacing
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->GlyphExtraSpacing);
        }

        public ref Vector2 GlyphOffset
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->GlyphOffset);
        }

        public IntPtr GlyphRanges
        {
            get => (IntPtr) NativePtr->GlyphRanges;
            set => NativePtr->GlyphRanges = (ushort*) value;
        }

        public ref float GlyphMinAdvanceX
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->GlyphMinAdvanceX);
        }

        public ref float GlyphMaxAdvanceX
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->GlyphMaxAdvanceX);
        }

        public ref bool MergeMode
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->MergeMode);
        }

        public ref uint FontBuilderFlags
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->FontBuilderFlags);
        }

        public ref float RasterizerMultiply
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->RasterizerMultiply);
        }

        public ref ushort EllipsisChar
        {
            get => ref Unsafe.AsRef<ushort>(&NativePtr->EllipsisChar);
        }

        public RangeAccessor<byte> Name
        {
            get => new RangeAccessor<byte>(NativePtr->Name, 40);
        }

        public ImFontPtr DstFont
        {
            get => new ImFontPtr(NativePtr->DstFont);
        }

        public void Destroy()
        {
            ImGuiNative.ImFontConfig_destroy(NativePtr);
        }
    }
}