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
        /// <param name="layerName">The layer to play on.</param>
        /// <param name="file">The file to play.</param>
        /// <returns>The layer the sound is playing on.</returns>
        public abstract Task PlayOnLayer(string layerName, SoundFile file);

        /// <summary>
        /// Plays the specified file on the specified layer.
        /// If the layer doesn't exist it is created.
        /// If sometimes else is playing on that layer this file will be appended to it.
        /// </summary>
        /// <param name="layerName">The layer to play on.</param>
        /// <param name="file">The file to play.</param>
        /// <returns>The layer the sound is playing on.</returns>
        public abstract Task QueueOnLayer(string layerName, SoundFile file);
        public abstract Task StopLayer(string layerName);
        public abstract void Dispose();
    }
}