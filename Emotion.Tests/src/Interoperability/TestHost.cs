// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Emotion.Engine.Configuration;
using Emotion.Engine.Hosting;
using Emotion.Engine.Hosting.Desktop;
using Emotion.Graphics;
using Emotion.Libraries;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Vector2 = System.Numerics.Vector2;

#endregion

namespace Emotion.Tests.Interoperability
{
    /// <summary>
    /// An OpenTK window used for testing.
    /// </summary>
    public sealed class TestHost : GameWindow, IHost
    {
        #region Properties

        /// <summary>
        /// Whether the window is focused. Accessible so tests can set it.
        /// </summary>
        public new bool Focused { get; set; } = true;

        /// <summary>
        /// The size of the window.
        /// </summary>
        public new Vector2 Size
        {
            get => new Vector2(ClientSize.Width, ClientSize.Height);
            set
            {
                Width = (int) value.X;
                Height = (int) value.Y;
            }
        }

        #endregion

        #region Hooks and Trackers

        private Action<float> _updateHook;
        private Action _drawHook;
        private Action _resizeHook;
        private Action _closeHook;
        private TestInputManager _inputManager;

        private bool _isFirstApplySettings = true;

        #endregion

        /// <inheritdoc />
        internal TestHost() : base(960, 540, GraphicsMode.Default, "Emotion Desktop Host",
            GameWindowFlags.Default, DisplayDevice.Default, Engine.Context.Flags.RenderFlags.OpenGLMajorVersion, Engine.Context.Flags.RenderFlags.OpenGLMinorVersion, GraphicsContextFlags.Offscreen,
            null, true)
        {
            OnUpdateThreadStarted += (a, b) => Thread.CurrentThread.Name = "Update Thread";
            _inputManager = new TestInputManager(this);
            Engine.Context.InputManager = _inputManager;
        }

        #region Host API

        /// <inheritdoc />
        public void SetHooks(Action<float> onUpdate, Action onDraw, Action onResize, Action onClose)
        {
            _updateHook = onUpdate;
            _drawHook = onDraw;
            _resizeHook = onResize;
            _closeHook = onClose;
        }

        /// <inheritdoc />
        public void ApplySettings(HostSettings settings)
        {
            Title = settings.Title;

            // Apply window mode.
            switch (settings.WindowMode)
            {
                case WindowMode.Borderless:
                    WindowBorder = WindowBorder.Hidden;
                    WindowState = WindowState.Normal;
                    Width = DisplayDevice.Default.Width;
                    Height = DisplayDevice.Default.Height;
                    if (CurrentPlatform.OS == PlatformName.Linux && _isFirstApplySettings) return;
                    X = 0;
                    Y = 0;
                    break;
                case WindowMode.Fullscreen:
                    WindowBorder = WindowBorder.Fixed;
                    WindowState = WindowState.Fullscreen;
                    break;
                default:
                    WindowBorder = WindowBorder.Fixed;
                    WindowState = WindowState.Normal;
                    Width = settings.Width;
                    Height = settings.Height;
                    if (CurrentPlatform.OS == PlatformName.Linux && _isFirstApplySettings) return;
                    X = DisplayDevice.Default.Width / 2 - settings.Width / 2;
                    Y = DisplayDevice.Default.Height / 2 - settings.Height / 2;
                    break;
            }

            _isFirstApplySettings = false;
        }

        /// <inheritdoc />
        public new void Run()
        {
            Visible = false;
            OnLoad(EventArgs.Empty);
            OnResize(EventArgs.Empty);
        }

        #endregion

        #region Event Wrapping

        /// <inheritdoc />
        protected override void OnResize(EventArgs e)
        {
            _resizeHook();
        }

        /// <inheritdoc />
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _closeHook();
        }

        #endregion

        #region Testing

        /// <summary>
        /// Run a single cycle of the host loop.
        /// </summary>
        /// <param name="frameTime">How much time should've passed.</param>
        public void RunCycle(float frameTime = 0)
        {
            ProcessEvents();
            _inputManager.Update();
            _updateHook?.Invoke(frameTime);
            _drawHook?.Invoke();
        }

        /// <summary>
        /// Take a screenshot of the host framebuffer.
        /// </summary>
        /// <returns>A screenshot of the host framebuffer.</returns>
        public Bitmap TakeScreenshot()
        {
            // Ensure that its called on the GLThread.
            GLThread.ForceGLThread();

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            Bitmap bmp = new Bitmap(w, h);
            BitmapData data =
                bmp.LockBits(ClientRectangle, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, w, h, OpenTK.Graphics.OpenGL4.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }

        #endregion
    }
}