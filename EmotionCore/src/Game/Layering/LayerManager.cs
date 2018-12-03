// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Layering
{
    /// <summary>
    /// Manages game layers.
    /// </summary>
    public class LayerManager
    {
        /// <summary>
        /// Layers currently running.
        /// </summary>
        private Dictionary<string, Layer> _layers;

        internal LayerManager()
        {
            _layers = new Dictionary<string, Layer>();
        }

        #region Loops

        /// <summary>
        /// Update all loaded layers.
        /// </summary>
        internal void Update()
        {
            // Update layers. The list conversion copies the list, allowing the dictionary to be edited. Do not replace with locks.
            foreach (Layer layer in _layers.Values.ToList())
            {
                // If the window is not focused run the light update, otherwise run the full update.
                if (!Context.Host.Focused)
                    layer.LightUpdate(Context.FrameTime);
                else
                    layer.Update(Context.FrameTime);
            }
        }

        /// <summary>
        /// Draw all loaded layers.
        /// </summary>
        internal void Draw()
        {
            // Update layers. The list conversion copies the list, allowing the dictionary to be edited. Do not replace with locks.
            foreach (Layer layer in _layers.Values.ToList())
            {
                Context.Renderer.MatrixStack.Push(Matrix4.CreateTranslation(0, 0, layer.Priority));
                layer.Draw(Context.Renderer);
                Context.Renderer.MatrixStack.Pop();
                GLThread.CheckError("layer draw");
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
        public Task Add(Layer layer, string name, int priority)
        {
            Context.Log.Trace($"Preparing to load layer [{name}]", MessageSource.LayerManager);

            // Setup layer properties.
            layer.Priority = priority;
            layer.Name = name;

            return Task.Run(() => LoadLayer(layer));
        }

        /// <summary>
        /// Returns a layer that is being loaded or has been loaded with the provided name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The layer with the provided name, or null if none found.</returns>
        public Layer Get(string name)
        {
            if (_layers.ContainsKey(name)) return _layers[name];

            return _layers.ContainsKey(name) ? _layers[name] : null;
        }

        /// <summary>
        /// Unload a loaded layer.
        /// </summary>
        /// <param name="name">The layer's name to unload.</param>
        public Task Remove(string name)
        {
            return Remove(_layers[name]);
        }

        /// <summary>
        /// Unload a loaded layer.
        /// </summary>
        /// <param name="layer">The layer to unload.</param>
        public Task Remove(Layer layer)
        {
            _layers.Remove(layer.Name);

            return Task.Run(() => UnloadLayer(layer));
        }

        #endregion

        #region Threads

        private void LoadLayer(Layer layer)
        {
#if !DEBUG
            try
            {
#endif
            Thread.CurrentThread.Name = $"Layer Loading Task - {layer.Name}";
            Context.Log.Trace($"Loading layer [{layer.Name}].", MessageSource.LayerManager);

            layer.Load();

            _layers.Add(layer.Name, layer);
            _layers = _layers.OrderBy(x => x.Value.Priority).ToList().ToDictionary(x => x.Key, x => x.Value);

            Context.Log.Info($"Loaded layer [{layer.Name}].", MessageSource.LayerManager);
#if !DEBUG
            }
            catch (Exception ex)
            {
                Context.Log.Error($"Error while loading layer {layer.Name}.", ex, MessageSource.LayerManager);
            }
#endif
        }

        private void UnloadLayer(Layer layer)
        {
#if !DEBUG
            try
            {
#endif
            Thread.CurrentThread.Name = $"Layer Unloading Task - {layer.Name}";
            Context.Log.Trace($"Unloading layer [{layer.Name}].", MessageSource.LayerManager);

            layer.Unload();

            Context.Log.Info($"Unloaded layer [{layer.Name}].", MessageSource.LayerManager);
#if !DEBUG
            }
            catch (Exception ex)
            {
                Context.Log.Error($"Error while unloading layer [{layer.Name}].", ex, MessageSource.LayerManager);
            }
#endif
        }

        #endregion
    }
}