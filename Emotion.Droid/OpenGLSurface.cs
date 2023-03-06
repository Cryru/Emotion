#region Using

using System.Numerics;
using Android.Content;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Emotion.Common;

#endregion

namespace Emotion.Droid;

public class OpenGLSurface : GLSurfaceView
{
	private Action<MotionEvent> _onTouch;

	public OpenGLSurface(Context? context, Action<MotionEvent> onTouch) : base(context)
	{
		_onTouch = onTouch;

		SetEGLContextClientVersion(3);
		//Holder?.SetFormat(Format.Rgba8888);
		SetEGLConfigChooser(8, 8, 8, 8, 24, 8);
	}

	public override bool OnTouchEvent(MotionEvent? e)
	{
		_onTouch(e);
		return true;
	}
}