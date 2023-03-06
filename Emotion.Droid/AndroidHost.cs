#region Using

using System.Numerics;
using Android.Graphics;
using Android.Opengl;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Platform.Implementation.CommonDesktop;

#endregion

namespace Emotion.Droid
{
	public class AndroidHost : PlatformBase
	{
		#region Android API

		public void SurfaceChanged()
		{
			// todo: gotta recreate everything yikes
			Resized(Size);
		}

		public void SurfaceCreated()
		{
			var config = new Configurator
			{
				PlatformOverride = this,
				LoopFactory = (onTick, onFrame) =>
				{
					_onTick = onTick;
					_onFrame = onFrame;
				},
				Logger = new AndroidLogger()
			};
			_mainMethod?.Invoke(config);
			_mainMethod = null;
			FocusChanged(true);
		}

		public void DrawFrame()
		{
			// On Android the loop is managed by the built in renderer.
			_onTick?.Invoke();
			_onFrame?.Invoke();
		}

		#endregion

		private Action<Configurator>? _mainMethod;
		private Action? _onTick;
		private Action? _onFrame;
		public AndroidGraphicsContext AndroidContext;

		public AndroidHost(Activity activity, Action<Configurator> mainMethod)
		{
			var surface = new OpenGLSurface(activity);
			activity.SetContentView(surface);
			var renderer = new AndroidGLRenderer(this);
			surface.SetRenderer(renderer);

			activity.SetContentView(surface);

			// Will flush only when there is something to flush.
			surface.RenderMode = Rendermode.Continuously;
			surface.PreserveEGLContextOnPause = true;

			AndroidContext = new AndroidGraphicsContext(surface, renderer);
			Context = AndroidContext;

			_mainMethod = mainMethod;
		}

		protected override void SetupInternal(Configurator config)
		{
			Engine.AssetLoader.AddSource(new FileAssetSource(""));
		}

		public override void DisplayMessageBox(string message)
		{
		}

		protected override bool UpdatePlatform()
		{
			return true;
		}

		public override WindowState WindowState { get; set; } = WindowState.Normal;

		protected override void UpdateDisplayMode()
		{
			// unsupported - always fullscreen.
			DisplayMode = DisplayMode.Fullscreen;
		}

		protected override Vector2 GetPosition()
		{
			return Vector2.Zero;
		}

		protected override void SetPosition(Vector2 position)
		{
			// unsupported
		}

		protected override Vector2 GetSize()
		{
			var drawableRect = new Rect();
			AndroidContext.Surface.GetDrawingRect(drawableRect);
			return new Vector2(drawableRect.Width(), drawableRect.Height());
		}

		protected override void SetSize(Vector2 size)
		{
			// unsupported
		}

		public override IntPtr LoadLibrary(string path)
		{
			return IntPtr.Zero;
		}

		public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
		{
			return IntPtr.Zero;
		}
	}
}