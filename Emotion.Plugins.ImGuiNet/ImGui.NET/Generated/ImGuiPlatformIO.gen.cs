#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiPlatformIO
    {
        public IntPtr Platform_CreateWindow;
        public IntPtr Platform_DestroyWindow;
        public IntPtr Platform_ShowWindow;
        public IntPtr Platform_SetWindowPos;
        public IntPtr Platform_GetWindowPos;
        public IntPtr Platform_SetWindowSize;
        public IntPtr Platform_GetWindowSize;
        public IntPtr Platform_SetWindowFocus;
        public IntPtr Platform_GetWindowFocus;
        public IntPtr Platform_GetWindowMinimized;
        public IntPtr Platform_SetWindowTitle;
        public IntPtr Platform_SetWindowAlpha;
        public IntPtr Platform_UpdateWindow;
        public IntPtr Platform_RenderWindow;
        public IntPtr Platform_SwapBuffers;
        public IntPtr Platform_GetWindowDpiScale;
        public IntPtr Platform_OnChangedViewport;
        public IntPtr Platform_CreateVkSurface;
        public IntPtr Renderer_CreateWindow;
        public IntPtr Renderer_DestroyWindow;
        public IntPtr Renderer_SetWindowSize;
        public IntPtr Renderer_RenderWindow;
        public IntPtr Renderer_SwapBuffers;
        public ImVector Monitors;
        public ImVector Viewports;
    }

    public unsafe struct ImGuiPlatformIOPtr
    {
        public ImGuiPlatformIO* NativePtr { get; }

        public ImGuiPlatformIOPtr(ImGuiPlatformIO* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiPlatformIOPtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiPlatformIO*) nativePtr;
        }

        public static implicit operator ImGuiPlatformIOPtr(ImGuiPlatformIO* nativePtr)
        {
            return new ImGuiPlatformIOPtr(nativePtr);
        }

        public static implicit operator ImGuiPlatformIO*(ImGuiPlatformIOPtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiPlatformIOPtr(IntPtr nativePtr)
        {
            return new ImGuiPlatformIOPtr(nativePtr);
        }

        public ref IntPtr Platform_CreateWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_CreateWindow);
        }

        public ref IntPtr Platform_DestroyWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_DestroyWindow);
        }

        public ref IntPtr Platform_ShowWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_ShowWindow);
        }

        public ref IntPtr Platform_SetWindowPos
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_SetWindowPos);
        }

        public ref IntPtr Platform_GetWindowPos
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_GetWindowPos);
        }

        public ref IntPtr Platform_SetWindowSize
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_SetWindowSize);
        }

        public ref IntPtr Platform_GetWindowSize
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_GetWindowSize);
        }

        public ref IntPtr Platform_SetWindowFocus
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_SetWindowFocus);
        }

        public ref IntPtr Platform_GetWindowFocus
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_GetWindowFocus);
        }

        public ref IntPtr Platform_GetWindowMinimized
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_GetWindowMinimized);
        }

        public ref IntPtr Platform_SetWindowTitle
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_SetWindowTitle);
        }

        public ref IntPtr Platform_SetWindowAlpha
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_SetWindowAlpha);
        }

        public ref IntPtr Platform_UpdateWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_UpdateWindow);
        }

        public ref IntPtr Platform_RenderWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_RenderWindow);
        }

        public ref IntPtr Platform_SwapBuffers
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_SwapBuffers);
        }

        public ref IntPtr Platform_GetWindowDpiScale
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_GetWindowDpiScale);
        }

        public ref IntPtr Platform_OnChangedViewport
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_OnChangedViewport);
        }

        public ref IntPtr Platform_CreateVkSurface
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Platform_CreateVkSurface);
        }

        public ref IntPtr Renderer_CreateWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Renderer_CreateWindow);
        }

        public ref IntPtr Renderer_DestroyWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Renderer_DestroyWindow);
        }

        public ref IntPtr Renderer_SetWindowSize
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Renderer_SetWindowSize);
        }

        public ref IntPtr Renderer_RenderWindow
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Renderer_RenderWindow);
        }

        public ref IntPtr Renderer_SwapBuffers
        {
            get => ref Unsafe.AsRef<IntPtr>(&NativePtr->Renderer_SwapBuffers);
        }

        public ImPtrVector<ImGuiPlatformMonitorPtr> Monitors
        {
            get => new ImPtrVector<ImGuiPlatformMonitorPtr>(NativePtr->Monitors, Unsafe.SizeOf<ImGuiPlatformMonitor>());
        }

        public ImVector<ImGuiViewportPtr> Viewports
        {
            get => new ImVector<ImGuiViewportPtr>(NativePtr->Viewports);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiPlatformIO_destroy(NativePtr);
        }
    }
}