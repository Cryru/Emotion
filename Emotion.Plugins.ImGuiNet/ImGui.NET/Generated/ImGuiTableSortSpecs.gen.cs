#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImGuiTableSortSpecs
    {
        public ImGuiTableColumnSortSpecs* Specs;
        public int SpecsCount;
        public byte SpecsDirty;
    }

    public unsafe struct ImGuiTableSortSpecsPtr
    {
        public ImGuiTableSortSpecs* NativePtr { get; }

        public ImGuiTableSortSpecsPtr(ImGuiTableSortSpecs* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiTableSortSpecsPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiTableSortSpecs*) nativePtr;
        }

        public static implicit operator ImGuiTableSortSpecsPtr(ImGuiTableSortSpecs* nativePtr)
        {
            return new ImGuiTableSortSpecsPtr(nativePtr);
        }

        public static implicit operator ImGuiTableSortSpecs*(ImGuiTableSortSpecsPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiTableSortSpecsPtr(IntPtr nativePtr)
        {
            return new ImGuiTableSortSpecsPtr(nativePtr);
        }

        public ImGuiTableColumnSortSpecsPtr Specs
        {
            get => new ImGuiTableColumnSortSpecsPtr(NativePtr->Specs);
        }

        public ref int SpecsCount
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->SpecsCount);
        }

        public ref bool SpecsDirty
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->SpecsDirty);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiTableSortSpecs_destroy(NativePtr);
        }
    }
}