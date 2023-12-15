#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct StbUndoRecord
    {
        public int where;
        public int insert_length;
        public int delete_length;
        public int char_storage;
    }

    public unsafe struct StbUndoRecordPtr
    {
        public StbUndoRecord* NativePtr { get; }

        public StbUndoRecordPtr(StbUndoRecord* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public StbUndoRecordPtr(IntPtr nativePtr)
        {
            NativePtr = (StbUndoRecord*) nativePtr;
        }

        public static implicit operator StbUndoRecordPtr(StbUndoRecord* nativePtr)
        {
            return new StbUndoRecordPtr(nativePtr);
        }

        public static implicit operator StbUndoRecord*(StbUndoRecordPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator StbUndoRecordPtr(IntPtr nativePtr)
        {
            return new StbUndoRecordPtr(nativePtr);
        }

        public ref int where
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->where);
        }

        public ref int insert_length
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->insert_length);
        }

        public ref int delete_length
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->delete_length);
        }

        public ref int char_storage
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->char_storage);
        }
    }
}