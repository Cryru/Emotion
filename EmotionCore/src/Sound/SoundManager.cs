// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion.Debug;
using Emotion.IO;
using Emotion.System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    public class SoundManager
    {
        private AudioContext _audioContext;
        private Dictionary<string, SoundLayer> _layers;

        public SoundManager()
        {
            _layers = new Dictionary<string, SoundLayer>();
            _audioContext = new AudioContext();

            // Setup listener.
            AL.Listener(ALListener3f.Position, 0, 0, 0);
            AL.Listener(ALListener3f.Velocity, 0, 0, 0);
        }

        #region Layer

        /// <summary>
        /// Create a sound layer. If already exists returns the already existing layer.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        /// <param name="startNow">Whether the layer should should out paused.</param>
        /// <param name="volume">What volume to play the layer at. Can be changed later.</param>
        /// <returns>The created sound layer which the manager will manage.</returns>
        public SoundLayer CreateLayer(string layerName, bool startNow = true, float volume = 1f)
        {
            lock (_layers)
            {
                if (_layers.ContainsKey(layerName)) return GetLayer(layerName);

                _layers.Add(layerName, new SoundLayer
                {
                    Volume = volume,
                    Paused = !startNow,
                    Name = layerName
                });

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Created layer [" + layerName + "]");

                return GetLayer(layerName);
            }
        }

        /// <summary>
        /// Returns the specified layer. If the layer doesn't exist, returns null.
        /// </summary>
        /// <param name="layerName">The name of the layer to return.</param>
        /// <returns>The specified layer.</returns>
        public SoundLayer GetLayer(string layerName)
        {
            lock (_layers)
            {
                return !_layers.ContainsKey(layerName) ? null : _layers[layerName];
            }
        }

        #endregion

        #region Source

        /// <summary>
        /// Play a sound file on a specified layer.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        /// <param name="file">The file to play.</param>
        /// <returns>A sound source representing the file on the layer.</returns>
        public Source PlayOnLayer(string layerName, SoundFile file)
        {
            lock (_layers)
            {
                Source newSource = new Source(file);
                _layers[layerName].Source = newSource;

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Playing [" + file.Name + "] on [" + _layers[layerName] + "]");

                return newSource;
            }
        }

        /// <summary>
        /// Play a list of sound files on a specified layer.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        /// <param name="files">The files to play.</param>
        /// <returns>A streaming sound source representing the file on the layer.</returns>
        public StreamingSource StreamOnLayer(string layerName, SoundFile[] files)
        {
            lock (_layers)
            {
                StreamingSource newSource = new StreamingSource(files);
                _layers[layerName].Source = newSource;

#if DEBUG
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Playing " + files.Length + " files on [" + _layers[layerName] + "]");
                foreach (SoundFile file in files)
                {
                    Debugger.Log(MessageType.Info, MessageSource.SoundManager, " |- " + file.Name);
                }
#endif

                return newSource;
            }
        }

        /// <summary>
        /// Returns the source running on the specified layer. If the layer has no source, or the layer doesn't exist, returns
        /// null.
        /// </summary>
        /// <param name="layerName">The name of the layer whose source to return.</param>
        /// <returns>The source running on the specified layer.</returns>
        public SourceBase GetLayerSource(string layerName)
        {
            lock (_layers)
            {
                SoundLayer layer = GetLayer(layerName);
                return layer?.Source;
            }
        }

        #endregion

        internal void Update()
        {
            lock (_layers)
            {
                foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                {
                    layer.Value.Update(Context.FrameTime, Context.Host.Focused, Context.Settings);
                }
            }
        }
    }
}