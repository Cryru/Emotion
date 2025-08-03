#nullable enable

#region Using

using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;
using Object = Java.Lang.Object;

#endregion

namespace Emotion.Core.Platform.Implementation.Android;

public class AndroidGLRenderer : Object, GLSurfaceView.IRenderer
{
    private AndroidHost _host;

    public AndroidGLRenderer(AndroidHost host)
    {
        _host = host;
    }

    public void OnDrawFrame(IGL10 gl)
    {
        _host.DrawFrame();
    }

    public void OnSurfaceChanged(IGL10 gl, int width, int height)
    {
        _host.SurfaceChanged();
    }

    public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
    {
        _host.SurfaceCreated();
    }
}