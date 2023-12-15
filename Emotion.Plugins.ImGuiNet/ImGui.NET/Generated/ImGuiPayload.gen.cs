#region Using

using System;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImGuiPayload
    {
        public void* Data;
        public int DataSize;
        public uint SourceId;
        public uint SourceParentId;
        public int DataFrameCount;
        public fixed byte DataType[33];
        public byte Preview;
        public byte Delivery;
    }

    public unsafe struct ImGuiPayloadPtr
    {
        public ImGuiPayload* NativePtr { get; }

        public ImGuiPayloadPtr(ImGuiPayload* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiPayloadPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiPayload*) nativePtr;
        }

        public static implicit operator ImGuiPayloadPtr(ImGuiPayload* nativePtr)
        {
            return new ImGuiPayloadPtr(nativePtr);
        }

        public static implicit operator ImGuiPayload*(ImGuiPayloadPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiPayloadPtr(IntPtr nativePtr)
        {
            return new ImGuiPayloadPtr(nativePtr);
        }

        public IntPtr Data
        {
            get => (IntPtr) NativePtr->Data;
            set => NativePtr->Data = (void*) value;
        }

        public ref int DataSize
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->DataSize);
        }

        public ref uint SourceId
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->SourceId);
        }

        public ref uint SourceParentId
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->SourceParentId);
        }

        public ref int DataFrameCount
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->DataFrameCount);
        }

        public RangeAccessor<byte> DataType
        {
            get => new RangeAccessor<byte>(NativePtr->DataType, 33);
        }

        public ref bool Preview
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->Preview);
        }

        public ref bool Delivery
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->Delivery);
        }

        public void Clear()
        {
            ImGuiNative.ImGuiPayload_Clear(NativePtr);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiPayload_destroy(NativePtr);
        }

        public bool IsDataType(string type)
        {
            byte* native_type;
            int type_byteCount = 0;
            if (type != null)
            {
                type_byteCount = Encoding.UTF8.GetByteCount(type);
                if (type_byteCount > Util.StackAllocationSizeLimit)
                {
                    native_type = Util.Allocate(type_byteCount + 1);
                }
                else
                {
                    byte* native_type_stackBytes = stackalloc byte[type_byteCount + 1];
                    native_type = native_type_stackBytes;
                }

                int native_type_offset = Util.GetUtf8(type, native_type, type_byteCount);
                native_type[native_type_offset] = 0;
            }
            else
            {
                native_type = null;
            }

            byte ret = ImGuiNative.ImGuiPayload_IsDataType(NativePtr, native_type);
            if (type_byteCount > Util.StackAllocationSizeLimit) Util.Free(native_type);
            return ret != 0;
        }

        public bool IsDelivery()
        {
            byte ret = ImGuiNative.ImGuiPayload_IsDelivery(NativePtr);
            return ret != 0;
        }

        public bool IsPreview()
        {
            byte ret = ImGuiNative.ImGuiPayload_IsPreview(NativePtr);
            return ret != 0;
        }
    }
}