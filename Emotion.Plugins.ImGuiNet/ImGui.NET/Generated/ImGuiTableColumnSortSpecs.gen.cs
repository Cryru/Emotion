#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiTableColumnSortSpecs
    {
        public uint ColumnUserID;
        public short ColumnIndex;
        public short SortOrder;
        public ImGuiSortDirection SortDirection;
    }

    public unsafe struct ImGuiTableColumnSortSpecsPtr
    {
        public ImGuiTableColumnSortSpecs* NativePtr { get; }

        public ImGuiTableColumnSortSpecsPtr(ImGuiTableColumnSortSpecs* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiTableColumnSortSpecsPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiTableColumnSortSpecs*) nativePtr;
        }

        public static implicit operator ImGuiTableColumnSortSpecsPtr(ImGuiTableColumnSortSpecs* nativePtr)
        {
            return new ImGuiTableColumnSortSpecsPtr(nativePtr);
        }

        public static implicit operator ImGuiTableColumnSortSpecs*(ImGuiTableColumnSortSpecsPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiTableColumnSortSpecsPtr(IntPtr nativePtr)
        {
            return new ImGuiTableColumnSortSpecsPtr(nativePtr);
        }

        public ref uint ColumnUserID
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->ColumnUserID);
        }

        public ref short ColumnIndex
        {
            get => ref Unsafe.AsRef<short>(&NativePtr->ColumnIndex);
        }

        public ref short SortOrder
        {
            get => ref Unsafe.AsRef<short>(&NativePtr->SortOrder);
        }

        public ref ImGuiSortDirection SortDirection
        {
            get => ref Unsafe.AsRef<ImGuiSortDirection>(&NativePtr->SortDirection);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiTableColumnSortSpecs_destroy(NativePtr);
        }
    }
}