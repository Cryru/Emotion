#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiKeyData
    {
        public byte Down;
        public float DownDuration;
        public float DownDurationPrev;
        public float AnalogValue;
    }

    public unsafe struct ImGuiKeyDataPtr
    {
        public ImGuiKeyData* NativePtr { get; }

        public ImGuiKeyDataPtr(ImGuiKeyData* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiKeyDataPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiKeyData*) nativePtr;
        }

        public static implicit operator ImGuiKeyDataPtr(ImGuiKeyData* nativePtr)
        {
            return new ImGuiKeyDataPtr(nativePtr);
        }

        public static implicit operator ImGuiKeyData*(ImGuiKeyDataPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiKeyDataPtr(IntPtr nativePtr)
        {
            return new ImGuiKeyDataPtr(nativePtr);
        }

        public ref bool Down
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->Down);
        }

        public ref float DownDuration
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->DownDuration);
        }

        public ref float DownDurationPrev
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->DownDurationPrev);
        }

        public ref float AnalogValue
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->AnalogValue);
        }
    }
}