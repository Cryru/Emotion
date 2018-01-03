// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Soul.Engine.Enums;
using Soul.Engine.Scenography;

#endregion

namespace Soul.Engine.Modules
{
    public static class SceneManager
    {
        #region Public

        /// <summary>
        /// Whether a scene is being loaded.
        /// </summary>
        public static bool isSceneLoading
        {
            get { return ScenesToLoad.Count > 0; }
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
            if (Settings.LoadingScene != null)
            {
                LoadingScene = (Scene) Activator.CreateInstance(Settings.LoadingScene);
                LoadScene("__loading__", LoadingScene);
            }
        }

        /// <summary>
        /// Updates the loaded scene, and any scene loading code.
        /// </summary>
        internal static void Update()
        {
            // Check if any scene to load.
            if (ScenesToLoad.Count > 0)
                for (int i = 0; i < ScenesToLoad.Count; i++)
                {
                    // Check if loaded.
                    if (ScenesToLoad[i] != null && ScenesToLoad[i].Loaded)
                    {
                        // Move the scene to the loaded scenes.
                        LoadedScenes.Add(ScenesToLoad[i].Name, ScenesToLoad[i].Scene);

                        // Check if we want to immediately swap to the new scene.
                        if (ScenesToLoad[i].SwapTo)
                            SwapScene(ScenesToLoad[i].Name);

                        // Remove the scene from the list of to load, as its already loaded.
                        ScenesToLoad[i] = null;
                    }
                    else
                    {
                        // Check if the scene has already been queued, to prevent thread spam.
                        SceneLoadArgs sceneLoadArgs = ScenesToLoad[i];
                        if (sceneLoadArgs != null && !sceneLoadArgs.Queued)
                        {
                            // Set queued flag.
                            SceneLoadArgs loadArgs = ScenesToLoad[i];
                            if (loadArgs != null) loadArgs.Queued = true;

                            // If not loaded create a new load thread and start loading.
                            Thread loadThread = new Thread(SceneLoadThread);
                            loadThread.Start(ScenesToLoad[i]);
                            // Wait for thread to activate.
                            while (!loadThread.IsAlive)
                            {
                            }
                        }
                    }
                }

            // Trim nulls.
            ScenesToLoad.RemoveAll(x => x == null);

            // Update the scene if it's not null and loaded, else update the loading screen.
            if (CurrentScene != null && !isSceneLoading) CurrentScene.Run();
            else if (LoadedScenes.ContainsKey("__loading__")) LoadedScenes["__loading__"].Run();
        }

        #endregion

        internal static void Draw()
        {
            // Draw the scene if it's not null, and loaded else draw the loading screen.
            if (CurrentScene != null && !isSceneLoading) CurrentScene.Draw();
            else if (LoadedScenes.ContainsKey("__loading__")) LoadedScenes["__loading__"].Draw();
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
                ErrorHandling.Raise(ErrorOrigin.SceneManager, "A scene with that name has already been loaded.");
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
            // Send scene queue message to the debugger.
            Debugging.DebugMessage(DebugMessageType.Info, "Queued scene " + sceneName);
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
                ErrorHandling.Raise(ErrorOrigin.SceneManager, "Cannot swap to a scene that isn't loaded.");
                return;
            }

            // Select the scene.
            Scene selectedScene = LoadedScenes[sceneName];

            // Check if scene is current.
            if (CurrentScene != null && CurrentScene.Equals(selectedScene))
            {
                ErrorHandling.Raise(ErrorOrigin.SceneManager, "Cannot swap to the currently loaded scene.");
                return;
            }

            // Swap.
            CurrentScene = selectedScene;

#if DEBUG
            // Send scene queue message to the debugger.
            Debugging.DebugMessage(DebugMessageType.Info, "Swapped scene to " + sceneName);
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
                ErrorHandling.Raise(ErrorOrigin.SceneManager,
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
        /// The thread which loads the next scene.
        /// </summary>
        /// <param name="args">The scene loading arguments.</param>
        private static void SceneLoadThread(object args)
        {
            SceneLoadArgs temp = (SceneLoadArgs) args;

            // Initiate inner setup.
            temp.Scene.InternalSetup();

            // Set the loaded flag to true.
            temp.Loaded = true;

#if DEBUG
            // Send scene queue message to the debugger.
            Debugging.DebugMessage(DebugMessageType.Info, "Loaded scene " + temp.Name);
#endif
        }

        #endregion
    }
}