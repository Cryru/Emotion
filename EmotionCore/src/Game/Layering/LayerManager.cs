// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;

#endregion

namespace Emotion.Game.Layering
{
    public class LayerManager : ContextObject
    {
        /// <summary>
        /// Layers currently running.
        /// </summary>
        private Dictionary<string, Layer> _loadedLayers;

        /// <summary>
        /// Layers ready to run.
        /// </summary>
        private Dictionary<string, Layer> _readyLayers;

        internal LayerManager(Context context) : base(context)
        {
            _loadedLayers = new Dictionary<string, Layer>();
            _readyLayers = new Dictionary<string, Layer>();
        }

        #region Loops

        /// <summary>
        /// Update all loaded layers.
        /// </summary>
        internal void Update()
        {
            // Process layers to unload.
            foreach (Layer layer in _loadedLayers.Values.ToList())
            {
                if (!layer.ToUnload) continue;
                _loadedLayers.Remove(layer.Name);
                Task unloadLayer = new Task(() => UnloadLayer(layer));
                unloadLayer.Start();
            }

            // Process layers to add.
            foreach (Layer layer in _readyLayers.Values.ToList())
            {
                _readyLayers.Remove(layer.Name);
                _loadedLayers.Add(layer.Name, layer);

                // Order by priority.
                _loadedLayers = _loadedLayers.OrderBy(x => x.Value.Priority).ToList().ToDictionary(x => x.Key, x => x.Value);
            }

            // Update loaded layers.
            foreach (KeyValuePair<string, Layer> layer in _loadedLayers)
            {
                // If the window is not focused run the light update, otherwise run the full update.
                if (!Context.Window.Focused)
                {
                    layer.Value.LightUpdate(Context.FrameTime);
                }
                else
                {
                    layer.Value.Update(Context.FrameTime);
                }
            }
        }

        /// <summary>
        /// Draw all loaded layers.
        /// </summary>
        internal void Draw()
        {
            foreach (KeyValuePair<string, Layer> layer in _loadedLayers)
            {
                layer.Value.Draw(Context.Renderer);
            }
        }

        #endregion

        #region API

        /// <summary>
        /// Load a layer.
        /// </summary>
        /// <param name="layer">The layer to load.</param>
        /// <param name="name">The name to alias the layer under.</param>
        /// <param name="priority">The layer update and draw priority.</param>
        /// <returns>The thread task which will load the layer.</returns>
        public void Add(Layer layer, string name, int priority)
        {
            Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Added layer [" + name + "]");

            // Setup layer API.
            layer.Priority = priority;
            layer.Name = name;
            layer.Context = Context;

            Task loadLayer = new Task(() => LoadLayer(layer));
            loadLayer.Start();
        }

        /// <summary>
        /// Returns a layer that is being loaded or has been loaded with the provided name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The layer with the provided name, or null if none found.</returns>
        public Layer Get(string name)
        {
            if (_loadedLayers.ContainsKey(name)) return _loadedLayers[name];

            return _loadedLayers.ContainsKey(name) ? _loadedLayers[name] : null;
        }

        /// <summary>
        /// Unload a loaded layer.
        /// </summary>
        /// <param name="name">The layer's name to unload.</param>
        public void Remove(string name)
        {
            Remove(_loadedLayers[name]);
        }

        /// <summary>
        /// Unload a loaded layer.
        /// </summary>
        /// <param name="layer">The layer to unload.</param>
        public void Remove(Layer layer)
        {
            layer.ToUnload = true;
        }

        #endregion

        #region Threads

        private void LoadLayer(Layer layer)
        {
            Debugger.Log(MessageType.Trace, MessageSource.LayerManager, "Loading layer [" + layer.Name + "]");

            layer.Load();
            _readyLayers.Add(layer.Name, layer);

            Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Loaded layer [" + layer.Name + "]");
        }

        private void UnloadLayer(Layer layer)
        {
            Debugger.Log(MessageType.Trace, MessageSource.LayerManager, "Unloading layer [" + layer.Name + "]");

            layer.Unload();

            Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Unloaded layer [" + layer.Name + "]");
        }

        #endregion
    }
}