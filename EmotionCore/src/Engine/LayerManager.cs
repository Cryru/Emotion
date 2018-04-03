// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Emotion.Engine.Debugging;
using Emotion.Engine.Objects;
using Emotion.Platform.Base;

#endregion

namespace Emotion.Engine
{
    public class LayerManager
    {
        private ContextBase _context;
        private Dictionary<string, Layer> _loadedLayers;
        private Dictionary<string, Layer> _unloadingLayers;
        private Dictionary<string, Layer> _loadingLayers;

        public LayerManager(ContextBase context)
        {
            _context = context;

            _loadedLayers = new Dictionary<string, Layer>();
            _unloadingLayers = new Dictionary<string, Layer>();
            _loadingLayers = new Dictionary<string, Layer>();

            Thread loadingThread = new Thread(LoadingThread);
            loadingThread.Start();

            Thread unloadingThread = new Thread(UnloadingThread);
            unloadingThread.Start();
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
                _unloadingLayers.Add(layer.Name, layer);
            }

            // Process layers which are loaded.
            foreach (Layer layer in _loadingLayers.Values.ToList())
            {
                if (!layer.Loaded) continue;
                _loadingLayers.Remove(layer.Name);
                _loadedLayers.Add(layer.Name, layer);
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
        public void Add(Layer layer, string name, int priority)
        {
            layer.Priority = priority;
            layer.Name = name;
            _loadingLayers.Add(name, layer);
        }

        /// <summary>
        /// Returns a layer that is being loaded or has been loaded with the provided name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The layer with the provided name, or null if none found.</returns>
        public Layer Get(string name)
        {
            if (_loadedLayers.ContainsKey(name))
            {
                return _loadedLayers[name];
            }
            else if (_loadingLayers.ContainsKey(name))
            {
                return _loadingLayers[name];
            }

            return null;
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
            if (!_loadingLayers.ContainsValue(layer)) throw new Exception("Tried to unload a layer in a manager which didn't load it.");
            layer.ToUnload = true;
        }

        #endregion

        #region Threads

        private void LoadingThread()
        {
#if DEBUG
            _context.Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Layer loading thread has started.");
#endif

            while (_context.Running)
            {
                foreach (Layer layer in _loadingLayers.Values.ToList())
                {
                    if (layer.Loaded) continue;
                    layer.Load();
                    layer.Loaded = true;
                }
            }
        }

        private void UnloadingThread()
        {
#if DEBUG
            _context.Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Layer unloading thread has started.");
#endif

            while (_context.Running)
            {
                foreach (Layer layer in _unloadingLayers.Values.ToList())
                {
                    if (!layer.ToUnload) continue;
                    layer.Unload();
                    _unloadingLayers.Remove(layer.Name);
                }
            }
        }

        #endregion
    }
}