// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.ComponentModel;
using System.Threading;
using Emotion.Engine.Configuration;
using Emotion.Libraries;
using OpenTK;
using OpenTK.Graphics;
using Vector2 = System.Numerics.Vector2;

#endregion

namespace Emotion.Engine.Hosting.Desktop
{
    /// <summary>
    /// An OpenTK window.
    /// </summary>
    internal sealed class OtkWindow : GameWindow, IHost
    {
        #region Properties

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

        /// <summary>
        /// The context's mode.
        /// </summary>
        private static GraphicsContextFlags _contextMode = GraphicsContextFlags.ForwardCompatible;

        #endregion

        #region Hooks and Trackers

        private Action<float> _updateHook;
        private Action<float> _drawHook;
        private Action _resizeHook;
        private Action _closeHook;
        private OtkInputManager _inputManager;

        private bool _isFirstApplySettings = true;

        #endregion

        /// <inheritdoc />
        static OtkWindow()
        {
#if DEBUG
            // Debug context breaks on Macs.
            if (CurrentPlatform.OS != PlatformName.Mac) _contextMode = GraphicsContextFlags.Debug;
#endif
        }

        /// <inheritdoc />
        internal OtkWindow() : base(960, 540, GraphicsMode.Default, "Emotion Desktop Host",
            GameWindowFlags.Default, DisplayDevice.Default, Engine.Context.Flags.RenderFlags.OpenGLMajorVersion, Engine.Context.Flags.RenderFlags.OpenGLMinorVersion, _contextMode, null, true)
        {
            OnUpdateThreadStarted += (a, b) => Thread.CurrentThread.Name = "Update Thread";
            _inputManager = new OtkInputManager(this);
            Engine.Context.InputManager = _inputManager;
        }

        #region Host API

        /// <inheritdoc />
        public void SetHooks(Action<float> onUpdate, Action<float> onDraw, Action onResize, Action onClose)
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
            // Run is blocking.
            Run(Engine.Context.Settings.RenderSettings.CapFPS);
        }

        #endregion

        #region Updating and Rendering

        /// <inheritdoc />
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _updateHook?.Invoke((float) e.Time * 1000);
            _inputManager.Update();
            base.OnUpdateFrame(e);
        }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _drawHook?.Invoke((float) e.Time * 1000);
            base.OnRenderFrame(e);
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
    }
}