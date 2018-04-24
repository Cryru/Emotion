// Emotion - https://github.com/Cryru/Emotion

#if GLES

#region Using

using System;
using Emotion.Engine;
using Emotion.Engine.Enums;
using Emotion.Engine.Objects;
using Emotion.Platform.Base;
using OpenTK;
#if DEBUG
using Debugger = Emotion.Engine.Debugging.Debugger;
using Emotion.Engine.Debugging;

#endif

#endregion

namespace Emotion.Platform.GLES
{
    public sealed class GLContext : Context
    {
        #region Declarations

        #region Platform Modules

        /// <summary>
        /// SDL2 Renderer.
        /// </summary>
        public new GLRenderer Renderer { get; private set; }

        ///// <summary>
        ///// SDL2 Input.
        ///// </summary>
        //public new SDLInput Input { get; private set; }

        /// <summary>
        /// The context's window.
        /// </summary>
        public GLWindow Window { get; private set; }

        ///// <summary>
        ///// Handles loading assets and storing assets.
        ///// </summary>
        //public Loader AssetLoader { get; private set; }

        #endregion

        #endregion

        /// <summary>
        /// Create a new Emotion context.
        /// </summary>
        public GLContext(Action<Settings> config = null)
        {
#if DEBUG
            Debugger = new Debugger(this);
            Debugger.Log(MessageType.Info, MessageSource.Engine, "Starting Emotion version " + Meta.Version);
            Debugger.Log(MessageType.Info, MessageSource.PlatformCore, "Creating OpenGL ES 3 context.");
#endif
            // Apply settings.
            Settings = new Settings();
            config?.Invoke(Settings);

            // Create a window.
            Window = new GLWindow(this);

            // Create a renderer.
            Renderer = new GLRenderer(this);
            base.Renderer = Renderer;

            // Load modules.
            //AssetLoader = new Loader(this);
            //Input = new SDLInput(this);
            //base.Input = Input;
            ScriptingEngine = new ScriptingEngine(this);
            LayerManager = new LayerManager(this);
        }

        public override void ApplySettings()
        {
            // Apply window mode.
            switch (Settings.WindowMode)
            {
                case WindowMode.Borderless:
                    Window.WindowBorder = WindowBorder.Hidden;
                    Window.WindowState = WindowState.Maximized;
                    break;
                case WindowMode.Fullscreen:
                    Window.WindowBorder = WindowBorder.Fixed;
                    Window.WindowState = WindowState.Fullscreen;
                    break;
                default:
                    Window.WindowBorder = WindowBorder.Fixed;
                    Window.WindowState = WindowState.Normal;
                    break;
            }
        }

        public override void Start()
        {
            Loop();
        }

        public override void Quit()
        {
            // Stop running.
            Running = false;
        }

        #region Loop Parts

        internal void LoopUpdate(float frameTime)
        {
            FrameTime = frameTime;

            // Run layer updates.
            LayerManager.Update();
        }

        internal void LoopDraw()
        {
            // Clear the screen.
            Renderer.Clear(Settings.ClearColor);

            // Run layer draws.
            LayerManager.Draw();

#if DEBUG
            // Debug drawing.
            Debugger.DebugLoop();
#endif

            // Swap buffers.
            Renderer.Present();
        }

        private void Loop()
        {
            Running = true;

            Window.Run(Settings.CapFPS, Settings.CapFPS);

            // Context has stopped running - cleanup.
            Window.Destroy();
            Renderer.Destroy();

            // Platform cleanup.
            Window.Dispose();

            // Dereference objects.
            Window = null;
            Renderer = null;
            Settings = null;

            // Close application.
            Environment.Exit(1);
        }

        #endregion
    }
}

#endif