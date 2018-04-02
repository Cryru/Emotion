// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Diagnostics;
using Emotion.Engine;
using Emotion.Engine.Objects;
using Emotion.External;
using Emotion.Platform.Base;
using Emotion.Platform.SDL2.Assets;
using Emotion.Primitives;
using SDL2;
using Debugger = Emotion.Engine.Debugging.Debugger;
#if DEBUG
using Emotion.Engine.Debugging;

#endif

#endregion

namespace Emotion.Platform.SDL2
{
    public sealed class Context : ContextBase
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

        #region Platform Modules

        /// <summary>
        /// SDL2 Renderer.
        /// </summary>
        public new Renderer Renderer { get; private set; }

        /// <summary>
        /// SDL2 Input.
        /// </summary>
        public new Input Input { get; private set; }

        /// <summary>
        /// The context's window.
        /// </summary>
        public Window Window { get; private set; }

        /// <summary>
        /// Handles loading assets and storing assets.
        /// </summary>
        public Loader AssetLoader { get; private set; }

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
                if (System.Diagnostics.Debugger.IsAttached)
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
            ErrorHandler.CheckError(SDL.SDL_Init(SDL.SDL_INIT_VIDEO));

            // Enable double buffering.
            ErrorHandler.CheckError(SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1));

            // Create a window.
            Window = new Window(this);

            // Create a renderer.
            Renderer = new Renderer(this);
            base.Renderer = Renderer;

            // Load modules.
            AssetLoader = new Loader(this);
            Input = new Input();
            base.Input = Input;
            ScriptingEngine = new ScriptingEngine();

#if DEBUG
            Debugger = new Debugger(this);
            Debugger.Log(MessageType.Info, MessageSource.Engine, "Starting Emotion version " + Meta.Version);
            Debugger.Log(MessageType.Info, MessageSource.PlatformCore, "SDL Context created!");
#endif
        }

        /// <summary>
        /// Start running the context.
        /// </summary>
        public void Start(Action draw)
        {
            Draw = draw;

            Loop();
        }

        #region Loop Parts

        private void Loop()
        {
            Running = true;
            Stopwatch frameCapTimer = new Stopwatch();

            while (Running)
            {
                frameCapTimer.Restart();

                // Calculate delta time.
                _last = _now;
                _now = SDL.SDL_GetPerformanceCounter();
                // Minimum is 1ms, maximum is 1s.
                FrameTime = GameMath.Clamp((_now - _last) * 1000 / SDL.SDL_GetPerformanceFrequency(), 1, 1000);

                // Update input module. This is done before events are handled so it can clear states from the previous frame.
                Input.UpdateInputs();

                // Update SDL.
                HandleEvents();
                // Check if an event caused a closing.
                if (Running == false) break;

                // Update user logic.
                DrawLogic();

                // Check if capping fps, and not enough time has passed.
                if (InitialSettings.CapFPS > 0)
                {
                    // Get difference between expected and actual.
                    float difference = 1000 / InitialSettings.CapFPS - frameCapTimer.ElapsedMilliseconds;

                    if (difference > 0) SDL.SDL_Delay((uint) difference);
                }
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
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                        Input.MouseStatePressed[currentEvent.button.button - 1] = false;
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        Input.MouseStatePressed[currentEvent.button.button - 1] = true;
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                        Input.MousePosition = new Vector2(currentEvent.motion.x, currentEvent.motion.y);
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
            Debugger.DebugLoop();
#endif

            // Swap buffers.
            Renderer.Present();
        }

        #endregion

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

#endif