// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using Emotion.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.Host
{
    internal sealed class Window : GameWindow, IHost
    {
        #region Properties

        /// <summary>
        /// The size of the window.
        /// </summary>
        public new Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = (int) value.X;
                Height = (int) value.Y;
            }
        }

        /// <summary>
        /// The size of the projection matrix.
        /// </summary>
        public Vector2 RenderSize { get; private set; }

        /// <summary>
        /// The context's mode.
        /// </summary>
        private static GraphicsContextFlags _contextMode = GraphicsContextFlags.ForwardCompatible;

        private Action<float> _updateHook;
        private Action<float> _drawHook;

        #endregion

        static Window()
        {
#if DEBUG
            // Debug context breaks on Macs.
            if (CurrentPlatform.OS == PlatformID.MacOSX) return;
            _contextMode = GraphicsContextFlags.Debug;
#endif
        }

        internal Window(Settings settings) : base(960, 540, GraphicsMode.Default, "Emotion Window Host", GameWindowFlags.Default, DisplayDevice.Default, 3, 3, _contextMode)
        {
            ApplySettings(settings, true);
            OnResize(null);
        }

        #region Host API

        public void SetHooks(Action<float> update, Action<float> draw)
        {
            _updateHook = update;
            _drawHook = draw;
        }

        public void ApplySettings(Settings settings)
        {
            ApplySettings(settings, false);
        }

        public void ApplySettings(Settings settings, bool firstTime)
        {
            Title = settings.WindowTitle;
            RenderSize = new Vector2(settings.RenderWidth, settings.RenderHeight);

            // Apply window mode.
            switch (settings.WindowMode)
            {
                case WindowMode.Borderless:
                    WindowBorder = WindowBorder.Hidden;
                    WindowState = WindowState.Normal;
                    Width = DisplayDevice.Default.Width;
                    Height = DisplayDevice.Default.Height;
                    if (CurrentPlatform.OS == PlatformID.Unix && firstTime) return;
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
                    Width = settings.WindowWidth;
                    Height = settings.WindowHeight;
                    if (CurrentPlatform.OS == PlatformID.Unix && firstTime) return;
                    X = DisplayDevice.Default.Width / 2 - settings.WindowWidth / 2;
                    Y = DisplayDevice.Default.Height / 2 - settings.WindowHeight / 2;
                    break;
            }
        }

        #endregion

        #region Updating and Rendering

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _updateHook?.Invoke((float) e.Time * 1000);
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _drawHook?.Invoke((float) e.Time * 1000);
            base.OnRenderFrame(e);
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            // Calculate borderbox / pillarbox.
            float targetAspectRatio = RenderSize.X / RenderSize.Y;

            float width = ClientSize.Width;
            float height = (int) (width / targetAspectRatio + 0.5f);

            // If the height is bigger then the black bars will appear on the top and bottom, otherwise they will be on the left and right.
            if (height > ClientSize.Height)
            {
                height = ClientSize.Height;
                width = (int) (height * targetAspectRatio + 0.5f);
            }

            int vpX = (int) (ClientSize.Width / 2 - width / 2);
            int vpY = (int) (ClientSize.Height / 2 - height / 2);

            // Set viewport.
            GL.Viewport(vpX, vpY, (int) width, (int) height);
            GL.Scissor(vpX, vpY, (int) width, (int) height);
        }
    }
}