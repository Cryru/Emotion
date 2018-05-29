// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.IO;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    public class SoundManager : ContextObject
    {
        private AudioContext _audioContext;
        private Dictionary<string, Source> _layers;
        private List<SoundEffect> _soundEffects;

        public SoundManager(Context context) : base(context)
        {
            _layers = new Dictionary<string, Source>();
            _audioContext = new AudioContext();
            _soundEffects = new List<SoundEffect>();

            // Setup listener.
            AL.Listener(ALListener3f.Position, 0, 0, 0);
            AL.Listener(ALListener3f.Velocity, 0, 0, 0);
        }

        #region Public API

        /// <summary>
        /// Play the sound file on the sound layer.
        /// </summary>
        /// <param name="layerName">The name of the layer. If reused the previous sound will be stopped.</param>
        /// <param name="file">The file to play.</param>
        /// <param name="startNow">Whether to start immediately.</param>
        /// <param name="volume">What volume to play the source at.</param>
        /// <returns>A sound source running on the layer to be further configured by the user.</returns>
        public Source PlaySoundLayer(string layerName, SoundFile file, bool startNow = true, float volume = 1f)
        {
            lock (_layers)
            {
                // Check if the layer exists.
                if (_layers.ContainsKey(layerName))
                {
                    // Check if already destroyed.
                    if(_layers[layerName] != null && !_layers[layerName].Destroyed) DestroySoundLayer(layerName);
                }
                else
                {
                    _layers.Add(layerName, null);
                }

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Playing track [" + file.AssetName + "] on sound layer [" + layerName + "]");

                Source newSource = new Source(file, volume);
                _layers[layerName] = newSource;
                if (startNow) newSource.Play();

                return newSource;
            }
        }

        /// <summary>
        /// Pause the specified layer.
        /// </summary>
        /// <param name="layerName">The layer to pause.</param>
        public void PauseSoundLayer(string layerName)
        {
            _layers[layerName].Pause();
        }

        /// <summary>
        /// Returns the source running on the specified layer.
        /// </summary>
        /// <param name="layerName">The layer to return the sound source of.</param>
        /// <returns></returns>
        public Source GetSoundLayer(string layerName)
        {
            return _layers[layerName];
        }

        /// <summary>
        /// Resumes a sound layer if paused.
        /// </summary>
        /// <param name="layerName">The layer to resume.</param>
        public void ResumeSoundLayer(string layerName)
        {
            _layers[layerName].Resume();
        }

        /// <summary>
        /// Destroy a sound layer, stopping playback and destroying the source.
        /// </summary>
        /// <param name="layerName">The name of the layer to destroy.</param>
        public void DestroySoundLayer(string layerName)
        {
            Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Destroying sound layer [" + layerName + "] with source " +  _layers[layerName].Pointer + " playing [" + _layers[layerName].FileName + "]");

            // Stop the source.
            _layers[layerName].Stop();
            // Destroy it.
            _layers[layerName].Destroy();
            _layers[layerName] = null;
        }

        #region Effects

        /// <summary>
        /// Add a sound effect to manage.
        /// </summary>
        /// <param name="soundEffect">An instance of a sound effect. Is updated every tick safely.</param>
        public void AddEffect(SoundEffect soundEffect)
        {
            Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Applying sound effect [" + soundEffect + "] to source " + soundEffect.RelatedSource.Pointer + " playing [" + soundEffect.RelatedSource.FileName + "]");

            _soundEffects.Add(soundEffect);
        }

        #endregion

        #endregion

        internal void Update()
        {
            lock (_layers)
            {
                // Go through all layers and update them.
                for (int i = 0; i < _layers.Count; i++)
                {
                    KeyValuePair<string, Source> soundLayer = _layers.ElementAt(i);

                    // Check if destroyed.
                    if (soundLayer.Value == null || soundLayer.Value.Destroyed) continue;

                    // Check if window focus is gone.
                    if (!Context.Window.Focused)
                    {
                        soundLayer.Value.FocusLossPause();
                        continue;
                    }

                    if (soundLayer.Value.FocusLossPaused) soundLayer.Value.Resume();

                    soundLayer.Value.Update(Context.Settings);
                }
            }

            // Update sound effects in reverse so removing is supported.
            for (int i = _soundEffects.Count - 1; i >= 0; i--)
            {
                // Check if the related source has been destroyed, or the effect has finished.
                if (_soundEffects[i].RelatedSource.Destroyed || _soundEffects[i].Finished)
                {
                    _soundEffects.Remove(_soundEffects[i]);
                    continue;
                }

                // Check if not playing.
                if (!_soundEffects[i].RelatedSource.Playing) continue;

                // Update the effect.
                _soundEffects[i].Update(Context.FrameTime);
            }
        }
    }
}