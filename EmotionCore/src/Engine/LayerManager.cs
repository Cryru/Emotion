// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Engine.Objects;
using Emotion.Platform.Base;
#if DEBUG
using Emotion.Engine.Debugging;

#endif

#endregion

namespace Emotion.Engine
{
    public class LayerManager
    {
        private ContextBase _context;
        private Dictionary<string, Layer> _loadedLayers;

        public LayerManager(ContextBase context)
        {
            _context = context;

            _loadedLayers = new Dictionary<string, Layer>();
        }

        #region Loops

        /// <summary>
        /// Update all loaded layers.
        /// </summary>
        public void Update()
        {
            // Process layers to unload.
            foreach (Layer layer in _loadedLayers.Values.ToList())
            {
                if (!layer.ToUnload) continue;
                _loadedLayers.Remove(layer.Name);
                Task unloadLayer = new Task(() => UnloadLayer(layer));
                unloadLayer.Start();
            }

            // Update loaded layers.
            foreach (KeyValuePair<string, Layer> layer in _loadedLayers)
            {
                layer.Value.Update();
            }
        }

        /// <summary>
        /// Draw all loaded layers.
        /// </summary>
        public void Draw()
        {
            foreach (KeyValuePair<string, Layer> layer in _loadedLayers)
            {
                layer.Value.Draw();
            }
        }

        #endregion

        #region Control

        /// <summary>
        /// Load a layer.
        /// </summary>
        /// <param name="layer">The layer to load.</param>
        /// <param name="name">The name to alias the layer under.</param>
        /// <param name="priority">The layer update and draw priority.</param>
        public Task Add(Layer layer, string name, int priority)
        {
#if DEBUG
            _context.Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Added layer [" + name + "]");
#endif

            layer.Priority = priority;
            layer.Name = name;

            Task loadLayer = new Task(() => LoadLayer(layer));
            loadLayer.Start();
            return loadLayer;
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
            layer.Load();
            _loadedLayers.Add(layer.Name, layer);

#if DEBUG
            _context.Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Loaded layer [" + layer.Name + "]");
#endif
        }

        private void UnloadLayer(Layer layer)
        {
            layer.Unload();
#if DEBUG
            _context.Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Unloaded layer [" + layer.Name + "]");
#endif
        }

        #endregion
    }
}