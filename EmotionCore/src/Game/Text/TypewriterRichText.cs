﻿// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Debug;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Text
{
    /// <inheritdoc />
    public class TypewriterRichText : RichText
    {
        #region Properties

        /// <summary>
        /// Whether the effect has finished.
        /// </summary>
        public bool EffectFinished
        {
            get => _characterEffectIndex == -1 || _characterEffectIndex >= _textStripped.Length;
        }

        /// <summary>
        /// How far the effect has reached.
        /// </summary>
        public int EffectTo
        {
            get => _characterEffectIndex;
        }

        protected float _totalDuration;
        protected float _durationPerCharacter;
        protected float _timer;

        /// <summary>
        /// How far the effect is. -1 when no effect was triggered, and _textStripped.Length when the effect is finished or was not
        /// reset.
        /// </summary>
        protected int _characterEffectIndex = -1;

        #endregion

        public TypewriterRichText(Vector3 position, Vector2 size, Atlas atlas) : base(position, size, atlas)
        {
        }

        #region API

        /// <summary>
        /// Update the TypewriterRichText's typewriter effect.
        /// </summary>
        /// <param name="dt">The amount of time passed since the last update.</param>
        public void Update(float dt)
        {
            // Check if scrolling is complete.
            if (EffectFinished) return;

            // Update the timer.
            _timer += dt;

            // Unload timer.
            while (_timer >= _durationPerCharacter)
            {
                _timer -= _durationPerCharacter;
                _characterEffectIndex++;

                if (!EffectFinished) continue;
                _characterEffectIndex = _textStripped.Length;
                return;
            }
        }

        /// <inheritdoc />
        public override void SetText(string text)
        {
            base.SetText(text);

            if (_totalDuration != 0) _durationPerCharacter = _totalDuration / _textStripped.Length;
        }

        /// <summary>
        /// Set text without resetting the typewriter effect's progress.
        /// </summary>
        /// <param name="text">The text to append.</param>
        public void AppendText(string text)
        {
            base.SetText(text);

            if (_totalDuration != 0) _durationPerCharacter = _totalDuration / _textStripped.Length;
            if (_characterEffectIndex > _textStripped.Length) _characterEffectIndex = _textStripped.Length;
        }

        /// <summary>
        /// Set the typewriter effect.
        /// </summary>
        /// <param name="duration">The duration of the total effect.</param>
        public void SetTypewriterEffect(float duration)
        {
            _timer = 0;
            _characterEffectIndex = 0;
            _totalDuration = duration;
            _durationPerCharacter = _totalDuration / _textStripped.Length;
        }

        /// <summary>
        /// Instantly end the typewriter effect.
        /// </summary>
        public void EndTypewriterEffect()
        {
            _characterEffectIndex = _textStripped.Length;
        }

        #endregion

        #region RichText API

        public override void Render(Renderer renderer)
        {
            if (_updateRenderCache || !_renderCache.AnythingMapped)
            {
                MapBuffer();
                _updateRenderCache = false;
            }

            // todo: Weird issue, investigate. Maybe connected to multi-threading.
            if (!_updateRenderCache && !_renderCache.AnythingMapped && _textStripped != "")
            {
                MapBuffer();
                _updateRenderCache = false;
                Debugger.Log(MessageType.Warning, MessageSource.GL, "RichText buffer wasn't mapped, and it didn't intend to map it.");
            }

            // Check if anything is mapped in the cache buffer.
            if (!_renderCache.AnythingMapped) return;

            // Don't draw anything if the effect is before the first.
            if (_characterEffectIndex == 0) return;

            // Draw the buffer.
            if (!EffectFinished) _renderCache.SetRenderRange(0, _characterEffectIndex);
            else _renderCache.SetRenderRange(0, _textStripped.Length);

            renderer.Render(_renderCache, true);
        }

        #endregion
    }
}