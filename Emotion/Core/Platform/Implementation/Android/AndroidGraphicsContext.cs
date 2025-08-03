#nullable enable

#region Using

using Emotion.Core.Platform;
using Emotion.Core.Platform.Implementation.Android;

#endregion

namespace Emotion.Core.Platform.Implementation.Android;

public unsafe class AndroidGraphicsContext : GraphicsContext
{
    public OpenGLSurface Surface; // GL surface to render on.
    public AndroidGLRenderer Renderer; // Android API GL context.

    protected PlatformBase _platform;
    private nint _eglLib;

    public AndroidGraphicsContext(PlatformBase platform, OpenGLSurface surface, AndroidGLRenderer renderer)
    {
        _platform = platform;
        Surface = surface;
        Renderer = renderer;

        _eglLib = platform.LoadLibrary("libEGL.so");
        if (_eglLib == nint.Zero)
        {
            Engine.Log.Error("Couldn't load EGL.", MessageSource.Egl);
            return;
        }
    }

    protected override void SetSwapIntervalPlatform(int interval)
    {
        // todo
    }

    public override void MakeCurrent()
    {
        // always current
    }

    public override void SwapBuffers()
    {
        Surface.RequestRender();
    }

    public override nint GetProcAddress(string func)
    {
        nint eglFunc = _platform.GetLibrarySymbolPtr(_eglLib, func);
        if (eglFunc != nint.Zero) return eglFunc;
        return AndroidEglNative.GetProcAddress(func);
    }
}