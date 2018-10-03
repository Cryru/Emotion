// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Primitives;
using Emotion.System;
using Emotion.Utils;

#endregion

namespace Emotion.Game.Layering
{
    public class LayerManager
    {
        /// <summary>
        /// Layers currently running.
        /// </summary>
        private Dictionary<string, Layer> _layers;

        /// <summary>
        /// Tasks to run. Loading and unloading layers.
        /// </summary>
        private ConcurrentQueue<Task> _managementTasks;

        internal LayerManager()
        {
            _layers = new Dictionary<string, Layer>();
            _managementTasks = new ConcurrentQueue<Task>();
        }

        #region Loops

        /// <summary>
        /// Update all loaded layers.
        /// </summary>
        internal void Update()
        {
            // Check if any management tasks are pending.
            if (!_managementTasks.IsEmpty)
            {
                // Get the current task.
                _managementTasks.TryPeek(out Task currentTask);
                if (currentTask != null)
                {
                    // If completed remove it from the queue.
                    if (currentTask.IsCompleted) _managementTasks.TryDequeue(out Task _);

                    // If not started, start it.
                    if (currentTask.Status == TaskStatus.Created) currentTask.Start();
                }
            }

            // Update layers. The list conversion copies the list, allowing the dictionary to be edited.
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
            foreach (Layer layer in _layers.Values.ToList())
            {
                Context.Renderer.MatrixStack.Push(Matrix4.CreateTranslation(0, 0, layer.Priority));
                layer.Draw(Context.Renderer);
                Context.Renderer.MatrixStack.Pop();
                Helpers.CheckError("layer draw");
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
            Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Added layer [" + name + "]");

            // Setup layer properties.
            layer.Priority = priority;
            layer.Name = name;

            Task loadLayer = new Task(() => LoadLayer(layer));
            _managementTasks.Enqueue(loadLayer);

            return loadLayer;
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
            Task unloadLayer = new Task(() => { UnloadLayer(layer); });
            _managementTasks.Enqueue(unloadLayer);

            return unloadLayer;
        }

        #endregion

        #region Threads

        private void LoadLayer(Layer layer)
        {
            Thread.CurrentThread.Name = "Layer Loading Task";

            Debugger.Log(MessageType.Trace, MessageSource.LayerManager, "Loading layer [" + layer.Name + "]");

            layer.Load();
            _layers.Add(layer.Name, layer);
            _layers = _layers.OrderBy(x => x.Value.Priority).ToList().ToDictionary(x => x.Key, x => x.Value);

            Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Loaded layer [" + layer.Name + "]");
        }

        private void UnloadLayer(Layer layer)
        {
            Thread.CurrentThread.Name = "Layer Unloading Task";

            Debugger.Log(MessageType.Trace, MessageSource.LayerManager, "Unloading layer [" + layer.Name + "]");

            layer.Unload();

            Debugger.Log(MessageType.Info, MessageSource.LayerManager, "Unloaded layer [" + layer.Name + "]");
        }

        #endregion
    }
}