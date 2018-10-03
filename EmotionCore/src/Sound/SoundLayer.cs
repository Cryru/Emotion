// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion.Debug;
using Emotion.System;

#endregion

namespace Emotion.Sound
{
    public sealed class SoundLayer
    {
        #region Properties

        /// <summary>
        /// The name of the layer.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The layer's volume.
        /// </summary>
        public float Volume;

        /// <summary>
        /// Whether the layer is paused.
        /// </summary>
        public bool Paused;

        /// <summary>
        /// Whether the source currently running is destroyed.
        /// </summary>
        public bool SourceDestroyed
        {
            get => _source == null || _source.Destroyed;
        }

        /// <summary>
        /// The source currently playing on the layer.
        /// </summary>
        public SourceBase Source
        {
            get => _source;
            set
            {
                DestroySource(_source);

                _source = value;
                _newSource = true;
                StateApplication();
            }
        }

        #endregion

        /// <summary>
        /// Whether the source was paused by a focus loss.
        /// </summary>
        internal bool FocusLossPaused { get; private set; }

        private SourceBase _source;
        private List<SoundEffect> _soundEffects = new List<SoundEffect>();
        private bool _newSource;

        #region Logic

        /// <summary>
        /// Update the sound layer. If a settings object is provided the source is updated too.
        /// </summary>
        /// <param name="frameTime">Time passed since the last update. Used for updating the sound effects.</param>
        /// <param name="focused">Whether the window is focused.</param>
        /// <param name="settings">A settings object to update the source with.</param>
        internal void Update(float frameTime, bool focused, Settings settings = null)
        {
            // Update effect.
            UpdateEffects(frameTime);

            // Check if any source.
            if (_source == null) return;

            // Check focus loss.
            if (FocusLossPaused && focused) FocusLossPaused = false;
            else if (!focused && !FocusLossPaused)
                FocusLossPaused = true;

            // Apply the layer state to the source state.
            StateApplication();

            // Set volume.
            _source.PersonalVolume = Volume;

            // Update source if needed.
            if (settings != null) _source.Update(settings);
        }

        /// <summary>
        /// Applies the layer state onto the source state.
        /// </summary>
        internal void StateApplication()
        {
            // Check if any source.
            if (_source == null) return;

            // Set volume.
            _source.PersonalVolume = Volume;

            // Apply paused state.
            if ((Paused || FocusLossPaused) && _source.Playing) _source.Pause();

            // Apply resume state.
            if (!Paused && _source.Paused && !FocusLossPaused) _source.Play();

            // Check if first play.
            if (!_newSource || Paused || FocusLossPaused) return;
            _source.Play();
            _newSource = false;
        }

        /// <summary>
        /// Updates attached effects and cleans them up when ready.
        /// </summary>
        /// <param name="frameTime"></param>
        internal void UpdateEffects(float frameTime)
        {
            // Update sound effects in reverse so removing is supported.
            for (int i = _soundEffects.Count - 1; i >= 0; i--)
            {
                // Check if the related source has been destroyed, or the effect has finished.
                if (_soundEffects[i].RelatedLayer.SourceDestroyed || _soundEffects[i].Finished)
                {
                    _soundEffects.Remove(_soundEffects[i]);
                    continue;
                }

                // Check if not playing.
                if (Paused || FocusLossPaused) continue;

                // Update the effect.
                _soundEffects[i].Update(frameTime);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Apply a sound effect on the layer.
        /// </summary>
        /// <param name="soundEffect">The sound effect to apply.</param>
        public void ApplySoundEffect(SoundEffect soundEffect)
        {
            Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Applying sound effect " + soundEffect);

            _soundEffects.Add(soundEffect);
        }

        /// <summary>
        /// Destroys the source.
        /// </summary>
        public void DestroySource(SourceBase source = null)
        {
            if (source == null) source = _source;
            if (source == null) return;

            Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Destroying sound source of layer [" + Name + "] " + source);

            // End the source.
            source.Stop();

            // Destroy it.
            source.Destroy();
        }

        #endregion

        #region Debugging

        public override string ToString()
        {
            string result = "[SoundLayer " + Name + "]";
            result += $"(source: {Source})";
            return result;
        }

        #endregion
    }
}