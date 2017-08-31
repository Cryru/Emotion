// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Raya.Graphics;
using Raya.System;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Scenes;

#endregion

namespace Soul.Engine
{
    public class Context : Actor
    {
        #region Information

        /// <summary>
        /// The time between frames. Used for accurate time keeping.
        /// </summary>
        public int FrameTime
        {
            get { return _frameTime; }
        }

        #endregion

        #region Internals

        /// <summary>
        /// The modules to load on boot. Do not edit unless you know what you are doing.
        /// </summary>
        private static Type[] ModulesToLoad = {typeof(ScriptModule), typeof(DebugModule)};

        /// <summary>
        /// Internal frame time variable.
        /// </summary>
        private int _frameTime;

        #endregion

        #region Raya API

        /// <summary>
        /// The context within the actor system.
        /// </summary>
        private Raya.System.Context nativeContext;

        #endregion

        public Context(Scene startScene)
        {
            // Define this context as the current.
            Globals.Context = this;

            // Create a Raya context.
            nativeContext = new Raya.System.Context();

            // Start boot timer.
            Clock bootTime = new Clock();

            // Setup logger.
            // Start Soul logging.
            Logger.Enabled = true;
            Logger.LogLimit = 2;
            Logger.Stamp = "==========\n" + "SoulEngine 2018 Log" + "\n==========";

            // Send debugging boot messages.
            DebugModule.DebugMessage(DebugMessageSource.Boot,
                "Starting SoulEngine " + Assembly.GetExecutingAssembly().GetName().Version);
            DebugModule.DebugMessage(DebugMessageSource.Boot, "Using: ");
            DebugModule.DebugMessage(DebugMessageSource.Boot, "Raya " + Meta.Version);
            DebugModule.DebugMessage(DebugMessageSource.Boot, "SoulLib " + Info.SoulVersion);

            // Create the window.
            nativeContext.CreateWindow();

            // Hook logger to the closing events.
            nativeContext.Closed += (sender, args) => Logger.ForceDump();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.ForceDump();

            // Load scene manager.
            InitializeSceneManager();

            // Load core modules.
            LoadModules();

            // Boot ready.
            DebugModule.DebugMessage(DebugMessageSource.Boot,
                "Boot took " + bootTime.ElapsedTime.AsMilliseconds() + " ms");
            bootTime.Dispose();

            // Load the starting scene, if any.
            if (startScene != null) LoadScene("startScene", startScene, true);

            // Define the timing clock.
            Clock timingClock = new Clock();

            // Start main loop.
            while (nativeContext.Running)
            {
                // Get the time since the last frame time timer restart, which is the time it took for the last frame to render.
                _frameTime = timingClock.ElapsedTime.AsMilliseconds();

                // Restart the frame time timer.
                timingClock.Restart();

                // Tick events and update Raya.
                nativeContext.Tick();

                // Start drawing.
                nativeContext.StartDraw();

                // Update module actors.
                UpdateActor();

                // Update the scene manager.
                UpdateScene();

                // Finish drawing frame.
                nativeContext.EndDraw();
            }
        }

        /// <summary>
        /// Loads all modules.
        /// </summary>
        private void LoadModules()
        {
            // Initiate engine modules.
            foreach (Type module in ModulesToLoad)
            {
                Actor newModule = (Actor) Activator.CreateInstance(module, new object[] { });
                AddChild(newModule);
            }
        }

        public override void Initialize()
        {
            // Context shouldn't be attached
            Error.Raise(1, "The Context's Initialize shouldn't be called!");
        }

        public override void Update()
        {
        }

        #region Drawing

        public void Draw(Drawable drawable)
        {
            nativeContext.Draw(drawable);
        }

        #endregion

        #region Scene Manager

        /// <summary>
        /// The scene currently running.
        /// </summary>
        public Scene CurrentScene;

        /// <summary>
        /// A list of loaded scenes for hot swapping.
        /// </summary>
        private Dictionary<string, Scene> _loadedScenes;

        /// <summary>
        /// The queue of scenes to load.
        /// </summary>
        private List<SceneLoadArgs> _scenesToLoad;

        /// <summary>
        /// Whether we are loading.
        /// </summary>
        public bool Loading
        {
            get { return _scenesToLoad.Count > 0; }
        }

        /// <summary>
        /// Setups the Scene Manager and the loading screen.
        /// </summary>
        public void InitializeSceneManager()
        {
            // Check if already loaded.
            if (_scenesToLoad != null) return;

            // Initialize lists.
            _scenesToLoad = new List<SceneLoadArgs>();
            _loadedScenes = new Dictionary<string, Scene>();

            // Load the loading scene, without swapping to it.
            LoadScene("__loading__", new Loading());
        }

        /// <summary>
        /// Loads the provided scene.
        /// </summary>
        /// <param name="sceneName">The name to keep the scene under.</param>
        /// <param name="scene">The scene object itself.</param>
        /// <param name="swapTo">Whether to swap to that scene.</param>
        public void LoadScene(string sceneName, Scene scene, bool swapTo = false)
        {
            // Check if that scene has already been loaded.
            if (_loadedScenes.ContainsKey(sceneName))
            {
                Error.Raise(180, "A scene with that name has already been loaded.");
                return;
            }

            // Add a scene to load in the next loop.
            _scenesToLoad.Add(new SceneLoadArgs
            {
                Scene = scene,
                Name = sceneName,
                SwapTo = swapTo
            });

            // Send scene queue message.
            DebugModule.DebugMessage(DebugMessageSource.SceneManager, "Queued scene " + sceneName);
        }

        /// <summary>
        /// Swaps the current scene to the loaded scene with the provided name.
        /// </summary>
        /// <param name="sceneName">The name of the loaded scene to swap to.</param>
        public void SwapScene(string sceneName)
        {
            // Check if the requested scene is a loaded scene.
            if (!_loadedScenes.ContainsKey(sceneName))
            {
                Error.Raise(183, "Cannot swap to a scene that isn't loaded.");
                return;
            }

            // Select the scene.
            Scene selectedScene = _loadedScenes[sceneName];

            // Check if scene is current.
            if (CurrentScene != null && CurrentScene.Equals(selectedScene))
            {
                Error.Raise(182, "Cannot swap to the currently loaded scene.");
                return;
            }

            // Swap.
            CurrentScene = selectedScene;

            // Log the scene swap.
            DebugModule.DebugMessage(DebugMessageSource.SceneManager, "Swapped scene to " + sceneName);
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="scene">The loaded scene to unload.</param>
        public void UnloadScene(Scene scene)
        {
            // Check if scene is current.
            if (CurrentScene.Equals(scene))
            {
                Error.Raise(181, "Cannot unload the currently loaded scene. Swap it first.");
                return;
            }

            // Dispose of the current scene if any.
            scene?.Dispose();

            // Remove the scene from the list of scenes.
            _loadedScenes.Remove(_loadedScenes.First(x => x.Value.Equals(scene)).Key);
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="sceneName">The name of the loaded scene to unload.</param>
        public void UnloadScene(string sceneName)
        {
            UnloadScene(_loadedScenes[sceneName]);
        }

        /// <summary>
        /// Updates the current scene.
        /// </summary>
        private void UpdateScene()
        {
            // Check if any scene to load.
            if (_scenesToLoad.Count > 0)
                for (int i = 0; i < _scenesToLoad.Count; i++)
                {
                    // Check if loaded.
                    if (_scenesToLoad[i] != null && _scenesToLoad[i].Loaded)
                    {
                        // Move the scene to the loaded scenes.
                        _loadedScenes.Add(_scenesToLoad[i].Name, _scenesToLoad[i].Scene);

                        // Check if we want to immediately swap to the new scene.
                        if (_scenesToLoad[i].SwapTo)
                            SwapScene(_scenesToLoad[i].Name);

                        // Remove the scene from the list of to load, as its already loaded.
                        _scenesToLoad[i] = null;
                    }
                    else
                    {
                        // Check if the scene has already been queued, to prevent thread spam.
                        SceneLoadArgs sceneLoadArgs = _scenesToLoad[i];
                        if (sceneLoadArgs != null && !sceneLoadArgs.Queued)
                        {
                            // Set queued flag.
                            SceneLoadArgs loadArgs = _scenesToLoad[i];
                            if (loadArgs != null) loadArgs.Queued = true;

                            // If not loaded create a new load thread and start loading.
                            Thread loadThread = new Thread(SceneLoadThread);
                            loadThread.Start(_scenesToLoad[i]);
                            // Wait for thread to activate.
                            while (!loadThread.IsAlive)
                            {
                            }
                        }
                    }
                }

            // Trim nulls.
            _scenesToLoad.RemoveAll(x => x == null);

            // Update the scene if it's loaded and not null, else update the loading screen.
            if (CurrentScene != null && !Loading) CurrentScene.UpdateActor();
            else if (_loadedScenes.ContainsKey("__loading__")) _loadedScenes["__loading__"].UpdateActor();
        }

        /// <summary>
        /// The thread which loads the next scene.
        /// </summary>
        /// <param name="args">The scene loading arguments.</param>
        private static void SceneLoadThread(object args)
        {
            SceneLoadArgs temp = (SceneLoadArgs) args;

            // Initiate inner setup.
            temp.Scene.Initialize();

            // Set the loaded flag to true.
            temp.Loaded = true;

            // Log the scene being loaded.
            DebugModule.DebugMessage(DebugMessageSource.SceneManager, "Loaded scene " + temp.Name);
        }

        #endregion
    }
}