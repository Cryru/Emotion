// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using Emotion.External;
using Emotion.Modules;
using Emotion.Objects;
using Emotion.Systems;
using SDL2;

#endregion

namespace Emotion.Engine
{
    public class Context
    {
        #region Declarations

        #region Properties

        /// <summary>
        /// Whether the context is running.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// The time it took to render the last frame.
        /// </summary>
        public float FrameTime { get; private set; }

        #endregion

        #region Objects

        /// <summary>
        /// The context's initial settings.
        /// </summary>
        internal Settings InitialSettings;

        /// <summary>
        /// The context's window.
        /// </summary>
        public Window Window { get; private set; }

        /// <summary>
        /// The context's renderer.
        /// </summary>
        public Renderer Renderer { get; private set; }

        #endregion

        #region Modules

        /// <summary>
        /// Module which handles loading assets and storing assets.
        /// </summary>
        public AssetLoader AssetLoader { get; private set; }

        /// <summary>
        /// Module which handles user input.
        /// </summary>
        public Input Input { get; private set; }

        #endregion

        #region Variables

        /// <summary>
        /// The performance frequency of the current frame.
        /// </summary>
        private ulong _now;

        /// <summary>
        /// The performance frequency of the last frame.
        /// </summary>
        private ulong _last;

        #endregion

        #endregion

        #region Hooks

        public Action Draw;

        #endregion

        /// <summary>
        /// Initiates external libraries and pre-initialization. Is run once for all instances.
        /// </summary>
        static Context()
        {
            // Set the DLL path on Windows.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Windows.SetDllDirectory(Environment.CurrentDirectory + "\\Libraries\\External\\" + (Environment.Is64BitProcess ? "x64" : "x86"));

                // Bypass an issue with SDL and debugging on Windows.
                if (Debugger.IsAttached)
                    SDL.SDL_SetHint("SDL_WINDOWS_DISABLE_THREAD_NAMING", "1");
            }
        }

        /// <summary>
        /// Create a new Emotion context.
        /// </summary>
        public Context(Action<Settings> config = null)
        {
            // Apply settings.
            InitialSettings = new Settings();
            config?.Invoke(InitialSettings);

            // Initialize SDL.
            ExternalErrorHandler.CheckError(SDL.SDL_Init(SDL.SDL_INIT_VIDEO));

            // Enable double buffering.
            ExternalErrorHandler.CheckError(SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1));

            // Create a window.
            Window = new Window(this);

            // Create a renderer.
            Renderer = new Renderer(this);

            // Load modules.
            AssetLoader = new AssetLoader(this);
            Input = new Input();

#if DEBUG

            Debugging.Log("Context created!");

#endif
        }

        private void Loop()
        {
            Running = true;

            while (Running)
            {
                // Calculate delta time.
                _last = _now;
                _now = SDL.SDL_GetPerformanceCounter();
                FrameTime = (_now - _last) * 1000 / SDL.SDL_GetPerformanceFrequency();

                // Update SDL.
                HandleEvents();
                // Update Emotion.
                Input.UpdateInputs();
                // Update drawing and user logic.
                DrawLogic();
            }

            // Context has stopped running - cleanup.

            // todo: object destroy

            // Dereference objects.
            Window = null;
            Renderer = null;
            InitialSettings = null;

            // Cleanup external.
            SDL.SDL_Quit();
        }

        #region Loop Parts

        private void HandleEvents()
        {
            // Loop while there are events to process.
            while (SDL.SDL_PollEvent(out SDL.SDL_Event currentEvent) == 1)
            {
                // Check the type.
                switch (currentEvent.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        Quit();
                        break;
                }
            }
        }

        private void DrawLogic()
        {
            // Clear the screen.
            Renderer.Clear();

            Draw?.Invoke();

#if DEBUG
            // Debug drawing.
            Debugging.DebugLoop(this);
#endif
           
            // Swap buffers.
            Renderer.Present();
        }

        #endregion

        /// <summary>
        /// Start running the context.
        /// </summary>
        public void Start(Action draw)
        {
            Draw = draw;

            Loop();
        }

        /// <summary>
        /// Stops running the context.
        /// </summary>
        public void Quit()
        {
            // Stop running.
            Running = false;
        }
    }
}