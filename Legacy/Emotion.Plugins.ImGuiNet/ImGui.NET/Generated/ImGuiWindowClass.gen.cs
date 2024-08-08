#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiWindowClass
    {
        public uint ClassId;
        public uint ParentViewportId;
        public ImGuiViewportFlags ViewportFlagsOverrideSet;
        public ImGuiViewportFlags ViewportFlagsOverrideClear;
        public ImGuiTabItemFlags TabItemFlagsOverrideSet;
        public ImGuiDockNodeFlags DockNodeFlagsOverrideSet;
        public byte DockingAlwaysTabBar;
        public byte DockingAllowUnclassed;
    }

    public unsafe struct ImGuiWindowClassPtr
    {
        public ImGuiWindowClass* NativePtr { get; }

        public ImGuiWindowClassPtr(ImGuiWindowClass* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiWindowClassPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiWindowClass*) nativePtr;
        }

        public static implicit operator ImGuiWindowClassPtr(ImGuiWindowClass* nativePtr)
        {
            return new ImGuiWindowClassPtr(nativePtr);
        }

        public static implicit operator ImGuiWindowClass*(ImGuiWindowClassPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiWindowClassPtr(IntPtr nativePtr)
        {
            return new ImGuiWindowClassPtr(nativePtr);
        }

        public ref uint ClassId
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->ClassId);
        }

        public ref uint ParentViewportId
        {
            get => ref Unsafe.AsRef<uint>(&NativePtr->ParentViewportId);
        }

        public ref ImGuiViewportFlags ViewportFlagsOverrideSet
        {
            get => ref Unsafe.AsRef<ImGuiViewportFlags>(&NativePtr->ViewportFlagsOverrideSet);
        }

        public ref ImGuiViewportFlags ViewportFlagsOverrideClear
        {
            get => ref Unsafe.AsRef<ImGuiViewportFlags>(&NativePtr->ViewportFlagsOverrideClear);
        }

        public ref ImGuiTabItemFlags TabItemFlagsOverrideSet
        {
            get => ref Unsafe.AsRef<ImGuiTabItemFlags>(&NativePtr->TabItemFlagsOverrideSet);
        }

        public ref ImGuiDockNodeFlags DockNodeFlagsOverrideSet
        {
            get => ref Unsafe.AsRef<ImGuiDockNodeFlags>(&NativePtr->DockNodeFlagsOverrideSet);
        }

        public ref bool DockingAlwaysTabBar
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->DockingAlwaysTabBar);
        }

        public ref bool DockingAllowUnclassed
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->DockingAllowUnclassed);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiWindowClass_destroy(NativePtr);
        }
    }
}