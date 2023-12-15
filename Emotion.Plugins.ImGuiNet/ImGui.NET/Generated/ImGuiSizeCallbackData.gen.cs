#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public unsafe struct ImGuiSizeCallbackData
    {
        public void* UserData;
        public Vector2 Pos;
        public Vector2 CurrentSize;
        public Vector2 DesiredSize;
    }

    public unsafe struct ImGuiSizeCallbackDataPtr
    {
        public ImGuiSizeCallbackData* NativePtr { get; }

        public ImGuiSizeCallbackDataPtr(ImGuiSizeCallbackData* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiSizeCallbackDataPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiSizeCallbackData*) nativePtr;
        }

        public static implicit operator ImGuiSizeCallbackDataPtr(ImGuiSizeCallbackData* nativePtr)
        {
            return new ImGuiSizeCallbackDataPtr(nativePtr);
        }

        public static implicit operator ImGuiSizeCallbackData*(ImGuiSizeCallbackDataPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiSizeCallbackDataPtr(IntPtr nativePtr)
        {
            return new ImGuiSizeCallbackDataPtr(nativePtr);
        }

        public IntPtr UserData
        {
            get => (IntPtr) NativePtr->UserData;
            set => NativePtr->UserData = (void*) value;
        }

        public ref Vector2 Pos
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->Pos);
        }

        public ref Vector2 CurrentSize
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->CurrentSize);
        }

        public ref Vector2 DesiredSize
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->DesiredSize);
        }
    }
}