#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImGuiListClipper
    {
        public int DisplayStart;
        public int DisplayEnd;
        public int ItemsCount;
        public float ItemsHeight;
        public float StartPosY;
        public void* TempData;
    }

    public unsafe struct ImGuiListClipperPtr
    {
        public ImGuiListClipper* NativePtr { get; }

        public ImGuiListClipperPtr(ImGuiListClipper* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiListClipperPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiListClipper*) nativePtr;
        }

        public static implicit operator ImGuiListClipperPtr(ImGuiListClipper* nativePtr)
        {
            return new ImGuiListClipperPtr(nativePtr);
        }

        public static implicit operator ImGuiListClipper*(ImGuiListClipperPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiListClipperPtr(IntPtr nativePtr)
        {
            return new ImGuiListClipperPtr(nativePtr);
        }

        public ref int DisplayStart
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->DisplayStart);
        }

        public ref int DisplayEnd
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->DisplayEnd);
        }

        public ref int ItemsCount
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->ItemsCount);
        }

        public ref float ItemsHeight
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ItemsHeight);
        }

        public ref float StartPosY
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->StartPosY);
        }

        public IntPtr TempData
        {
            get => (IntPtr) NativePtr->TempData;
            set => NativePtr->TempData = (void*) value;
        }

        public void Begin(int items_count)
        {
            float items_height = -1.0f;
            ImGuiNative.ImGuiListClipper_Begin(NativePtr, items_count, items_height);
        }

        public void Begin(int items_count, float items_height)
        {
            ImGuiNative.ImGuiListClipper_Begin(NativePtr, items_count, items_height);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiListClipper_destroy(NativePtr);
        }

        public void End()
        {
            ImGuiNative.ImGuiListClipper_End(NativePtr);
        }

        public void ForceDisplayRangeByIndices(int item_min, int item_max)
        {
            ImGuiNative.ImGuiListClipper_ForceDisplayRangeByIndices(NativePtr, item_min, item_max);
        }

        public bool Step()
        {
            byte ret = ImGuiNative.ImGuiListClipper_Step(NativePtr);
            return ret != 0;
        }
    }
}