#region Using

using Emotion.Common;
using Emotion.Platform;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Droid;

public unsafe class AndroidGraphicsContext : GraphicsContext
{
    public OpenGLSurface Surface; // GL surface to render on.
    public AndroidGLRenderer Renderer; // Android API GL context.

    protected PlatformBase _platform;
    private IntPtr _eglLib;

    public AndroidGraphicsContext(PlatformBase platform, OpenGLSurface surface, AndroidGLRenderer renderer)
    {
        _platform = platform;
        Surface = surface;
        Renderer = renderer;

        _eglLib = platform.LoadLibrary("libEGL.so");
        if (_eglLib == IntPtr.Zero)
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

    public override IntPtr GetProcAddress(string func)
    {
        IntPtr eglFunc = _platform.GetLibrarySymbolPtr(_eglLib, func);
        if (eglFunc != IntPtr.Zero) return eglFunc;
        return Egl.GetProcAddress(func);
    }
}