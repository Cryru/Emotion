#region Using

using System;
using System.Text;

#endregion

namespace ImGuiNET
{
    public struct ImFontGlyphRangesBuilder
    {
        public ImVector UsedChars;
    }

    public unsafe struct ImFontGlyphRangesBuilderPtr
    {
        public ImFontGlyphRangesBuilder* NativePtr { get; }

        public ImFontGlyphRangesBuilderPtr(ImFontGlyphRangesBuilder* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImFontGlyphRangesBuilderPtr(IntPtr nativePtr)
        {
            NativePtr = (ImFontGlyphRangesBuilder*) nativePtr;
        }

        public static implicit operator ImFontGlyphRangesBuilderPtr(ImFontGlyphRangesBuilder* nativePtr)
        {
            return new ImFontGlyphRangesBuilderPtr(nativePtr);
        }

        public static implicit operator ImFontGlyphRangesBuilder*(ImFontGlyphRangesBuilderPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImFontGlyphRangesBuilderPtr(IntPtr nativePtr)
        {
            return new ImFontGlyphRangesBuilderPtr(nativePtr);
        }

        public ImVector<uint> UsedChars
        {
            get => new ImVector<uint>(NativePtr->UsedChars);
        }

        public void AddChar(ushort c)
        {
            ImGuiNative.ImFontGlyphRangesBuilder_AddChar(NativePtr, c);
        }

        public void AddRanges(IntPtr ranges)
        {
            ushort* native_ranges = (ushort*) ranges.ToPointer();
            ImGuiNative.ImFontGlyphRangesBuilder_AddRanges(NativePtr, native_ranges);
        }

        public void AddText(string text)
        {
            byte* native_text;
            int text_byteCount = 0;
            if (text != null)
            {
                text_byteCount = Encoding.UTF8.GetByteCount(text);
                if (text_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_text = Util.Allocate(text_byteCount + 1);
                }
                else
                {
                    byte* native_text_stackBytes = stackalloc byte[text_byteCount + 1];
                    native_text = native_text_stackBytes;
                }

                int native_text_offset = Util.GetUtf8(text, native_text, text_byteCount);
                native_text[native_text_offset] = 0;
            }
            else
            {
                native_text = null;
            }

            byte* native_text_end = null;
            ImGuiNative.ImFontGlyphRangesBuilder_AddText(NativePtr, native_text, native_text_end);
            if (text_byteCount > Util.StackAllocationSizeLimit) Util.Free(native_text);
        }

        public void BuildRanges(out ImVector out_ranges)
        {
            fixed (ImVector* native_out_ranges = &out_ranges)
            {
                ImGuiNative.ImFontGlyphRangesBuilder_BuildRanges(NativePtr, native_out_ranges);
            }
        }

        public void Clear()
        {
            ImGuiNative.ImFontGlyphRangesBuilder_Clear(NativePtr);
        }

        public void Destroy()
        {
            ImGuiNative.ImFontGlyphRangesBuilder_destroy(NativePtr);
        }

        public bool GetBit(uint n)
        {
            byte ret = ImGuiNative.ImFontGlyphRangesBuilder_GetBit(NativePtr, n);
            return ret != 0;
        }

        public void SetBit(uint n)
        {
            ImGuiNative.ImFontGlyphRangesBuilder_SetBit(NativePtr, n);
        }
    }
}