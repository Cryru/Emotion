using SoulEngine.Objects;
using SoulEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoulEngine.Modules
{
    class SceneManager : IModuleUpdatable, IModuleComposable, IModuleDrawable
    {
        #region Declarations
        /// <summary>
        /// The scene currently running.
        /// </summary>
        public Scene currentScene;
        /// <summary>
        /// A list of loaded scenes for hotswapping.
        /// </summary>
        private Dictionary<string, Scene> loadedScenes;
        /// <summary>
        /// The queue of scenes to load.
        /// </summary>
        private List<SceneLoadArgs> scenesToLoad;
        /// <summary>
        /// Whether we are loading.
        /// </summary>
        public bool Loading
        {
            get
            {
                return scenesToLoad.Count > 0;
            }
        }
        #endregion

        public bool Initialize()
        {
            // Initialize lists.
            scenesToLoad = new List<SceneLoadArgs>();
            loadedScenes = new Dictionary<string, Scene>();

            // Load the loading scene, without swapping to it.
            LoadScene("__loading__", new Loading(), false);

            // Load the primary scene.
            LoadScene("Prime", new ScenePrim(), true);

            return true;
        }

        public void Update()
        {
            // Check if debugging, in which case we want to not update the scene.
            if(!Context.Core.Paused)
            {
                // Update the current scene if its loaded, if not the loading screen if its loaded.
                if (currentScene != null && !Loading) currentScene.UpdateHook();
                else if (loadedScenes.ContainsKey("__loading__")) loadedScenes["__loading__"].UpdateHook();
            }
        }

        public void Compose()
        {
            // Check if any scene to load.
            if (scenesToLoad.Count > 0)
            {
                for (int i = 0; i < scenesToLoad.Count; i++)
                {
                    // Check if loaded.
                    if (scenesToLoad[i] != null && scenesToLoad[i].Loaded)
                    {
                        // Move the scene to the loaded scenes.
                        loadedScenes.Add(scenesToLoad[i].Name, scenesToLoad[i].Scene);

                        // Check if we want to immediately swap to the new scene.
                        if (scenesToLoad[i].swapTo)
                        {
                            SwapScene(scenesToLoad[i].Scene);
                        }

                        // Remove the scene from the list of to load, as its already loaded.
                        scenesToLoad[i] = null;
                    }
                    else
                    {
                        // Check if the scene has already been queued, to prevent thread spam.
                        if(!scenesToLoad[i].Queued)
                        {
                            // Set queued flag.
                            scenesToLoad[i].Queued = true;

                            // If not loaded create a new load thread and start loading.
                            Thread loadThread = new Thread(new ParameterizedThreadStart(SceneLoadThread));
                            loadThread.Start(scenesToLoad[i]);
                            // Wait for thread to activate.
                            while (!loadThread.IsAlive) ;
                        }
                    }
                }
            }

            // Trim nulls.
            scenesToLoad.RemoveAll(x => x == null);

            // Run the scene's compose.
            if (currentScene != null && !Loading) currentScene.Compose();
            else if (loadedScenes.ContainsKey("__loading__")) loadedScenes["__loading__"].Compose();
        }

        public void Draw()
        {
            if (currentScene != null && !Loading) currentScene.DrawHook();
            else if (loadedScenes.ContainsKey("__loading__")) loadedScenes["__loading__"].DrawHook();
        }

        public void LoadScene(string SceneName, Scene Scene, bool swapTo = false)
        {
            if(loadedScenes.ContainsKey(SceneName))
            {
                if (Context.Core.isModuleLoaded<ErrorManager>())
                    Context.Core.Module<ErrorManager>().RaiseError("Scene with that name has already been loaded.", 180);
                return;
            }

            // Add a scene to load in the next loop.
            scenesToLoad.Add(new SceneLoadArgs()
            {
                Scene = Scene,
                Name = SceneName,
                swapTo = swapTo
            });
        }

        private void SceneLoadThread(object args)
        {
            SceneLoadArgs temp = (SceneLoadArgs)args;

            // Initiate inner setup.
            temp.Scene.SetupScene();

            // Set the loaded flag to true.
            temp.Loaded = true;

            // Log the scene being loaded.
            if (Context.Core.isModuleLoaded<Logger>()) Context.Core.Module<Logger>().Add("Scene loaded: " + temp.Scene.ToString().Replace("SoulEngine.", ""));
        }

        /// <summary>
        /// Sets the provided loaded scene as current.
        /// </summary>
        /// <param name="Scene">The loaded scene to set as current.</param>
        public void SwapScene(Scene Scene)
        {
            // Check if scene is current.
            if (currentScene != null && currentScene.Equals(Scene))
            {
                if (Context.Core.isModuleLoaded<ErrorManager>()) Context.Core.Module<ErrorManager>().RaiseError("Cannot swap to the currently loaded scene.", 182);
                return;
            }

            // Swap.
            currentScene = Scene;
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="Scene">The loaded scene to unload.</param>
        public void UnloadScene(Scene Scene)
        {
            // Check if scene is current.
            if(currentScene.Equals(Scene))
            {
                if (Context.Core.isModuleLoaded<ErrorManager>()) Context.Core.Module<ErrorManager>().RaiseError("Cannot unload the currently loaded scene. Swap it first.", 181);
                return;
            }

            // Dispose of the current scene if any.
            if (Scene != null) Scene.Dispose();

            // Remove the scene from the list of scenes.
            loadedScenes.Remove(loadedScenes.Where(x => x.Equals(Scene)).First().Key);
        }

        /// <summary>
        /// Sets the provided loaded scene as current.
        /// </summary>
        /// <param name="SceneName">The name of the loaded scene to set as current.</param>
        public void SwapScene(string SceneName)
        {
            SwapScene(loadedScenes[SceneName]);
        }

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        /// <param name="Scene">The name of the loaded scene to unload.</param>
        public void UnloadScene(string SceneName)
        {
            UnloadScene(loadedScenes[SceneName]);
        }
    }

    public class SceneLoadArgs
    {
        public Scene Scene;
        public bool swapTo;
        public string Name;
        public bool Loaded = false;
        public bool Queued = false;
    }
}
