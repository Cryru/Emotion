#region Using

using System;
using System.Text;

#endregion

namespace ImGuiNET
{
    public struct ImGuiTextBuffer
    {
        public ImVector Buf;
    }

    public unsafe struct ImGuiTextBufferPtr
    {
        public ImGuiTextBuffer* NativePtr { get; }

        public ImGuiTextBufferPtr(ImGuiTextBuffer* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiTextBufferPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiTextBuffer*) nativePtr;
        }

        public static implicit operator ImGuiTextBufferPtr(ImGuiTextBuffer* nativePtr)
        {
            return new ImGuiTextBufferPtr(nativePtr);
        }

        public static implicit operator ImGuiTextBuffer*(ImGuiTextBufferPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiTextBufferPtr(IntPtr nativePtr)
        {
            return new ImGuiTextBufferPtr(nativePtr);
        }

        public ImVector<byte> Buf
        {
            get => new ImVector<byte>(NativePtr->Buf);
        }

        public void append(string str)
        {
            byte* native_str;
            int str_byteCount = 0;
            if (str != null)
            {
                str_byteCount = Encoding.UTF8.GetByteCount(str);
                if (str_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_str = Util.Allocate(str_byteCount + 1);
                }
                else
                {
                    byte* native_str_stackBytes = stackalloc byte[str_byteCount + 1];
                    native_str = native_str_stackBytes;
                }

                int native_str_offset = Util.GetUtf8(str, native_str, str_byteCount);
                native_str[native_str_offset] = 0;
            }
            else
            {
                native_str = null;
            }

            byte* native_str_end = null;
            ImGuiNative.ImGuiTextBuffer_append(NativePtr, native_str, native_str_end);
            if (str_byteCount > Util.StackAllocationSizeLimit) Util.Free(native_str);
        }

        public void appendf(string fmt)
        {
            byte* native_fmt;
            int fmt_byteCount = 0;
            if (fmt != null)
            {
                fmt_byteCount = Encoding.UTF8.GetByteCount(fmt);
                if (fmt_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_fmt = Util.Allocate(fmt_byteCount + 1);
                }
                else
                {
                    byte* native_fmt_stackBytes = stackalloc byte[fmt_byteCount + 1];
                    native_fmt = native_fmt_stackBytes;
                }

                int native_fmt_offset = Util.GetUtf8(fmt, native_fmt, fmt_byteCount);
                native_fmt[native_fmt_offset] = 0;
            }
            else
            {
                native_fmt = null;
            }

            ImGuiNative.ImGuiTextBuffer_appendf(NativePtr, native_fmt);
            if (fmt_byteCount > Util.StackAllocationSizeLimit) Util.Free(native_fmt);
        }

        public string begin()
        {
            byte* ret = ImGuiNative.ImGuiTextBuffer_begin(NativePtr);
            return Util.StringFromPtr(ret);
        }

        public string c_str()
        {
            byte* ret = ImGuiNative.ImGuiTextBuffer_c_str(NativePtr);
            return Util.StringFromPtr(ret);
        }

        public void clear()
        {
            ImGuiNative.ImGuiTextBuffer_clear(NativePtr);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiTextBuffer_destroy(NativePtr);
        }

        public bool empty()
        {
            byte ret = ImGuiNative.ImGuiTextBuffer_empty(NativePtr);
            return ret != 0;
        }

        public string end()
        {
            byte* ret = ImGuiNative.ImGuiTextBuffer_end(NativePtr);
            return Util.StringFromPtr(ret);
        }

        public void reserve(int capacity)
        {
            ImGuiNative.ImGuiTextBuffer_reserve(NativePtr, capacity);
        }

        public int size()
        {
            int ret = ImGuiNative.ImGuiTextBuffer_size(NativePtr);
            return ret;
        }
    }
}