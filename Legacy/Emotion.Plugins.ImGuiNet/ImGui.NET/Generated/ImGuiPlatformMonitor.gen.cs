#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiPlatformMonitor
    {
        public Vector2 MainPos;
        public Vector2 MainSize;
        public Vector2 WorkPos;
        public Vector2 WorkSize;
        public float DpiScale;
    }

    public unsafe struct ImGuiPlatformMonitorPtr
    {
        public ImGuiPlatformMonitor* NativePtr { get; }

        public ImGuiPlatformMonitorPtr(ImGuiPlatformMonitor* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiPlatformMonitorPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiPlatformMonitor*) nativePtr;
        }

        public static implicit operator ImGuiPlatformMonitorPtr(ImGuiPlatformMonitor* nativePtr)
        {
            return new ImGuiPlatformMonitorPtr(nativePtr);
        }

        public static implicit operator ImGuiPlatformMonitor*(ImGuiPlatformMonitorPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiPlatformMonitorPtr(IntPtr nativePtr)
        {
            return new ImGuiPlatformMonitorPtr(nativePtr);
        }

        public ref Vector2 MainPos
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->MainPos);
        }

        public ref Vector2 MainSize
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->MainSize);
        }

        public ref Vector2 WorkPos
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->WorkPos);
        }

        public ref Vector2 WorkSize
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->WorkSize);
        }

        public ref float DpiScale
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->DpiScale);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiPlatformMonitor_destroy(NativePtr);
        }
    }
}