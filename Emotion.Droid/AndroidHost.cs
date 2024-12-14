#region Using

using System.Numerics;
using System.Runtime.InteropServices;
using Android.Graphics;
using Android.Opengl;
using Android.Views;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Input;

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
        private Activity _activity;

        public AndroidGraphicsContext AndroidContext;

        public AndroidHost(Activity activity, Action<Configurator> mainMethod)
        {
            _activity = activity;

            var surface = new OpenGLSurface(activity, OnTouchEvent);
            activity.SetContentView(surface);
            var renderer = new AndroidGLRenderer(this);
            surface.SetRenderer(renderer);

            activity.SetContentView(surface);

            // Will flush only when there is something to flush.
            surface.RenderMode = Rendermode.Continuously;
            surface.PreserveEGLContextOnPause = true;

            AndroidContext = new AndroidGraphicsContext(this, surface, renderer);
            Context = AndroidContext;

            Audio = new AndroidAudio(this);

            _mainMethod = mainMethod;
        }

        protected override void SetupInternal(Configurator config)
        {
            Engine.AssetLoader.AddSource(new AndroidAssetSource(_activity));
            Engine.AssetLoader.AddStore(new FileAssetStore("Player"));
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
            return NativeLibrary.Load(path);
        }

        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return !NativeLibrary.TryGetExport(library, symbolName, out IntPtr ptr) ? IntPtr.Zero : ptr;
        }

        private Vector2 _prevTouch;

        private void OnTouchEvent(MotionEvent e)
        {
            Vector2 pos = new Vector2(e.GetX(), e.GetY());
            if (e.Action == MotionEventActions.Move ||
                e.Action == MotionEventActions.Down)
                UpdateMousePosition(pos);

            if (e.Action == MotionEventActions.Down)
            {
                UpdateKeyStatus(Key.MouseKeyLeft, true);
               
            }

            if (e.Action == MotionEventActions.Up)
            {
                UpdateKeyStatus(Key.MouseKeyLeft, false);
                UpdateMousePosition(new Vector2(-1));
            }

            _prevTouch = pos;
            if (e.Action == MotionEventActions.Move)
            {
                // todo: pinch to mouse wheel
            }
        }

        public override bool IsTouchScreen()
        {
            return true;
        }
    }
}