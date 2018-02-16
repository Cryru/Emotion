// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Soul.Engine.Diagnostics;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Scenography
{
    public static class SceneManager
    {
        #region Public

        /// <summary>
        /// Whether a scene is being loaded.
        /// </summary>
        public static bool SceneLoading
        {
            get => ScenesToLoad.Count > 0;
        }

        #endregion

        #region Internals

        /// <summary>
        /// The scene currently running.
        /// </summary>
        internal static Scene CurrentScene;

        /// <summary>
        /// A list of loaded scenes for hot swapping.
        /// </summary>
        internal static Dictionary<string, Scene> LoadedScenes;

        /// <summary>
        /// The queue of scenes to load.
        /// </summary>
        internal static List<SceneLoadArgs> ScenesToLoad;

        /// <summary>
        /// The loading scene.
        /// </summary>
        internal static Scene LoadingScene;

        #endregion

        #region Module API

        /// <summary>
        /// Setup the module.
        /// </summary>
        internal static void Setup()
        {
            // Initialize lists.
            ScenesToLoad = new List<SceneLoadArgs>();
            LoadedScenes = new Dictionary<string, Scene>();

            // If a loading scene is set - initialize it and load it.
            if (Settings.LoadingScene == null) return;

            LoadingScene = (Scene) Activator.CreateInstance(Settings.LoadingScene);
            LoadScene("__loading__", LoadingScene);
        }

        /// <summary>
        /// Updates the loaded scene, and any scene loading code.
        /// </summary>
        internal static void Update()
        {
            // Check if any scene to load.
            if (ScenesToLoad.Count > 0)
                foreach (SceneLoadArgs scene in ScenesToLoad)
                {
                    // Check if the scene is loading.
                    if (scene.Loading) continue;

                    // Set the state to queued.
                    scene.Loading = true;

#if DEBUG
                    // Send scene queue message to the debugger.
                    Debugging.DebugMessage(DiagnosticMessageType.SceneManager, "Loading [" + scene.Name + "]");
#endif

                    // Start the thread to load it.
                    Thread loadThread = new Thread(SceneLoadThread);
                    loadThread.Start(scene);

                    // Wait for thread to activate.
                    while (!loadThread.IsAlive) { }
                }

            // Update the scene if it's not null and loaded, else update the loading screen.
            if (CurrentScene != null && !SceneLoading) CurrentScene.Run();
            else if (LoadedScenes.ContainsKey("__loading__"))
                LoadedScenes["__loading__"].Run();
        }

        #endregion

        internal static void Draw()
        {
            // Draw the scene if it's not null, and loaded else draw the loading screen.
            if (CurrentScene != null && !SceneLoading) CurrentScene.Draw();
            else if (LoadedScenes.ContainsKey("__loading__"))
                LoadedScenes["__loading__"].Draw();
        }

        #region Functions

        /// <summary>
        /// Loads the provided scene.
        /// </summary>
        /// <param name="sceneName">The name to keep the scene under.</param>
        /// <param name="scene">The scene object itself.</param>
        /// <param name="swapTo">Whether to swap to that scene.</param>
        public static void LoadScene(string sceneName, Scene scene, bool swapTo = false)
        {
            // Check if that scene has already been loaded.
            if (LoadedScenes.ContainsKey(sceneName))
            {
                ErrorHandling.Raise(DiagnosticMessageType.SceneManager, "A scene with that name has already been loaded.");
                return;
            }

            // Add a scene to load in the next loop.
            ScenesToLoad.Add(new SceneLoadArgs
            {
                Scene = scene,
                Name = sceneName,
                SwapTo = swapTo
            });

#if DEBUG
            // Send scene queued message to the debugger.
            Debugging.DebugMessage(DiagnosticMessageType.SceneManager, "Queued [" + sceneName + "]");
#endif
        }


        /// <summary>
        /// Swaps the current scene to the loaded scene with the provided name.
        /// </summary>
        /// <param name="sceneName">The name of the loaded scene to swap to.</param>
        public static void SwapScene(string sceneName)
        {
            // Check if the requested scene is a loaded scene.
            if (!LoadedScenes.ContainsKey(sceneName))
            {
                ErrorHandling.Raise(DiagnosticMessageType.SceneManager, "Cannot swap to a scene that isn't loaded.");
                return;
            }

            // Select the scene.
            Scene selectedScene = LoadedScenes[sceneName];

            // Check if scene is current.
            if (CurrentScene != null && CurrentScene.Equals(selectedScene))
            {
                ErrorHandling.Raise(DiagnosticMessageType.SceneManager, "Cannot swap to the currently loaded scene.");
                return;
            }

            // Swap.
            CurrentScene = selectedScene;

            Scripting.Expose("scene", CurrentScene);

#if DEBUG
            // Send scene queue message to the debugger.
            Debugging.DebugMessage(DiagnosticMessageType.SceneManager, "Swapped scene to [" + sceneName + "]");
#endif
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="scene">The loaded scene to unload.</param>
        public static void UnloadScene(Scene scene)
        {
            // Check if scene is current.
            if (CurrentScene.Equals(scene))
            {
                ErrorHandling.Raise(DiagnosticMessageType.SceneManager,
                    "Cannot unload the currently loaded scene. Swap away from it first.");
                return;
            }

            // Remove the scene from the list of scenes.
            LoadedScenes.Remove(LoadedScenes.First(x => x.Value.Equals(scene)).Key);
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="sceneName">The name of the loaded scene to unload.</param>
        public static void UnloadScene(string sceneName)
        {
            UnloadScene(LoadedScenes[sceneName]);
        }

        /// <summary>
        /// Loads a scene on another thread.
        /// </summary>
        /// <param name="args">The scene load args.</param>
        private static void SceneLoadThread(object args)
        {
            SceneLoadArgs temp = (SceneLoadArgs)args;

            // Initiate loading.
            temp.Scene.InternalSetup();

#if DEBUG
            // Send scene queue message to the debugger.
            Debugging.DebugMessage(DiagnosticMessageType.SceneManager, "Loaded [" + temp.Name + "]");
#endif

            // Move the scene to the loaded scenes.
            LoadedScenes.Add(temp.Name, temp.Scene);

            // Check if we want to immediately swap to the new scene.
            if (temp.SwapTo)
                SwapScene(temp.Name);

            // Remove the scene from the list of to load, as its already loaded.
            ScenesToLoad.Remove(temp);
        }

        #endregion
    }
}