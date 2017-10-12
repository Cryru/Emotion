// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Soul.Engine.Enums;
using Soul.Engine.Internal;

#endregion

namespace Soul.Engine.Modules
{
    public static class SceneManager
    {
        #region Declarations

        /// <summary>
        /// The scene currently running.
        /// </summary>
        public static Scene CurrentScene;

        /// <summary>
        /// A list of loaded scenes for hot swapping.
        /// </summary>
        private static Dictionary<string, Scene> _loadedScenes;

        /// <summary>
        /// The queue of scenes to load.
        /// </summary>
        private static List<SceneLoadArgs> _scenesToLoad;

        /// <summary>
        /// Whether a scene is being loaded.
        /// </summary>
        public static bool SceneLoading
        {
            get { return _scenesToLoad.Count > 0; }
        }

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        static SceneManager()
        {
            // Check if already loaded.
            if (_scenesToLoad != null) return;

            // Initialize lists.
            _scenesToLoad = new List<SceneLoadArgs>();
            _loadedScenes = new Dictionary<string, Scene>();

            // Load the loading scene, without swapping to it.
            if(Core.LoadingScene != null) LoadScene("__loading__", Core.LoadingScene);
        }

        /// <summary>
        /// Updates the loaded scene, and any scene loading code.
        /// </summary>
        public static void Update()
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
            if (CurrentScene != null && !SceneLoading) CurrentScene.UpdateActor();
            else if (_loadedScenes.ContainsKey("__loading__")) _loadedScenes["__loading__"].UpdateActor();
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
            Debugger.DebugMessage(DebugMessageSource.SceneManager, "Queued scene " + sceneName);
        }


        /// <summary>
        /// Swaps the current scene to the loaded scene with the provided name.
        /// </summary>
        /// <param name="sceneName">The name of the loaded scene to swap to.</param>
        public static void SwapScene(string sceneName)
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
            Debugger.DebugMessage(DebugMessageSource.SceneManager, "Swapped scene to " + sceneName);

            // Expose the scene to the script engine.
            ScriptEngine.Expose("scene", CurrentScene);
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
                Error.Raise(181, "Cannot unload the currently loaded scene. Swap it first.");
                return;
            }

            // Remove the scene from the list of scenes.
            _loadedScenes.Remove(_loadedScenes.First(x => x.Value.Equals(scene)).Key);
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="sceneName">The name of the loaded scene to unload.</param>
        public static void UnloadScene(string sceneName)
        {
            UnloadScene(_loadedScenes[sceneName]);
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
            Debugger.DebugMessage(DebugMessageSource.SceneManager, "Loaded scene " + temp.Name);
        }

        #endregion
    }
}