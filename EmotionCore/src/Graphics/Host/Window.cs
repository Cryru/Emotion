// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using Emotion.GLES;
using Emotion.Utils;
using OpenTK;
using OpenTK.Graphics;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.Graphics.Host
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
            if(CurrentPlatform.OS == PlatformID.MacOSX) return;
            _contextMode = GraphicsContextFlags.Debug;
#endif
        }

        internal Window(Settings settings) : base(960, 540, GraphicsMode.Default, "Emotion Window Host", GameWindowFlags.Default, DisplayDevice.Default, 3, 3, _contextMode)
        {
            ApplySettings(settings);
        }

        #region Host API

        public void SetHooks(Action<float> update, Action<float> draw)
        {
            _updateHook = update;
            _drawHook = draw;
        }

        public void ApplySettings(Settings settings)
        {
            Title = settings.WindowTitle;

            // Apply window mode.
            switch (settings.WindowMode)
            {
                case WindowMode.Borderless:
                    WindowBorder = WindowBorder.Hidden;
                    WindowState = WindowState.Normal;
                    Width = DisplayDevice.Default.Width;
                    Height = DisplayDevice.Default.Height;
                    X = 0;
                    Y = 0;
                    break;
                case WindowMode.Fullscreen:
                    WindowBorder = WindowBorder.Fixed;
                    WindowState = WindowState.Fullscreen;
                    break;
                default:
                    Width = settings.WindowWidth;
                    Height = settings.WindowHeight;
                    X = DisplayDevice.Default.Width / 2 - settings.WindowWidth / 2;
                    Y = DisplayDevice.Default.Height / 2 - settings.WindowHeight / 2;
                    WindowBorder = WindowBorder.Fixed;
                    WindowState = WindowState.Normal;
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
    }
}