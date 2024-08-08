#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct StbTexteditRow
    {
        public float x0;
        public float x1;
        public float baseline_y_delta;
        public float ymin;
        public float ymax;
        public int num_chars;
    }

    public unsafe struct StbTexteditRowPtr
    {
        public StbTexteditRow* NativePtr { get; }

        public StbTexteditRowPtr(StbTexteditRow* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public StbTexteditRowPtr(IntPtr nativePtr)
        {
            NativePtr = (StbTexteditRow*) nativePtr;
        }

        public static implicit operator StbTexteditRowPtr(StbTexteditRow* nativePtr)
        {
            return new StbTexteditRowPtr(nativePtr);
        }

        public static implicit operator StbTexteditRow*(StbTexteditRowPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator StbTexteditRowPtr(IntPtr nativePtr)
        {
            return new StbTexteditRowPtr(nativePtr);
        }

        public ref float x0
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->x0);
        }

        public ref float x1
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->x1);
        }

        public ref float baseline_y_delta
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->baseline_y_delta);
        }

        public ref float ymin
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ymin);
        }

        public ref float ymax
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ymax);
        }

        public ref int num_chars
        {
            get => ref Unsafe.AsRef<int>(&NativePtr->num_chars);
        }
    }
}