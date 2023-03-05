#region Using

using Android.Content;
using Android.Opengl;
using Android.Runtime;
using Android.Util;

#endregion

namespace Emotion.Android;

public class OpenGLSurface : GLSurfaceView
{
	protected OpenGLSurface(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
	{
	}

	public OpenGLSurface(Context? context) : base(context)
	{
		SetEGLContextClientVersion(3);
		//Holder?.SetFormat(Format.Rgba8888);
		SetEGLConfigChooser(8, 8, 8, 8, 24, 8);
	}

	public OpenGLSurface(Context? context, IAttributeSet? attrs) : base(context, attrs)
	{
	}
}