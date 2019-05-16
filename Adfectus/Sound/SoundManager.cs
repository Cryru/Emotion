using System;
using System.Threading.Tasks;
using Adfectus.Common;

namespace Adfectus.Sound
{
    public abstract class SoundManager : IDisposable
    {
        /// <summary>
        /// The volume of sounds.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Whether sound is enabled.
        /// </summary>
        public bool Sound { get; set; }

        /// <summary>
        /// List of active sound layers.
        /// </summary>
        public abstract string[] Layers {  get; }

        /// <summary>
        /// Create a new sound manager.
        /// </summary>
        /// <param name="builder">The engine build configurator.</param>
        protected SoundManager(EngineBuilder builder)
        {
            Volume = builder.InitialVolume;
            Sound = builder.InitialSound;
        }

        /// <summary>
        /// Plays the specified file on the specified layer.
        /// If the layer doesn't exist it is created.
        /// If sometimes else is playing on that layer it will be forcefully stopped.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <param name="layerName">The layer to play on.</param>
        /// <returns>A task for when the action is complete.</returns>
        public abstract Task Play(SoundFile file, string layerName);

        /// <summary>
        /// Pauses the specified layer.
        /// </summary>
        /// <param name="layerName">The layer to pause.</param>
        /// <returns>A task for when the action is complete.</returns>
        public abstract Task Pause(string layerName);

        /// <summary>
        /// Resume a paused layer.
        /// </summary>
        /// <param name="layerName">The layer to resume.</param>
        /// <returns>A task for when the action is complete.</returns>
        public abstract Task Resume(string layerName);

        /// <summary>
        /// Plays the specified file on the specified layer.
        /// If the layer doesn't exist it is created.
        /// If sometimes else is playing on that layer this file will be appended to it.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <param name="layerName">The layer to play on.</param>
        /// <returns>A task for when the action is complete.</returns>
        public abstract Task QueuePlay(SoundFile file, string layerName);

        /// <summary>
        /// Stop playing sound on the specified layer.
        /// </summary>
        /// <param name="layerName">The layer name to stop playing.</param>
        /// <param name="now">Whether to stop the layer now - regardless whether any effects need to finish.</param>
        /// <returns>A task for when the action is complete.</returns>
        public abstract Task StopLayer(string layerName, bool now = false);

        /// <summary>
        /// Get an existing layer by name - or create it if it doesn't exist.
        /// </summary>
        /// <param name="layerName">The name of the layer to retrieve.</param>
        /// <returns>The layer with the specified name.</returns>
        public abstract SoundLayer GetLayer(string layerName);

        /// <summary>
        /// Remove an existing layer by name.
        /// </summary>
        /// <param name="layerName">The layer to remove.</param>
        public abstract void RemoveLayer(string layerName);

        /// <summary>
        /// Dispose of the sound manager, freeing resources. Is called by the Engine during cleanup.
        /// </summary>
        public abstract void Dispose();
    }
}