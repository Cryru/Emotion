// Emotion - https://github.com/Cryru/Emotion

#region Using

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
        protected int _characterEffectIndex = -1;

        #endregion

        public TypewriterRichText(Rectangle bounds, Atlas atlas) : base(bounds, atlas)
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
            if (_characterEffectIndex == -1) return;

            // Update the timer.
            _timer += dt;

            // Unload timer.
            while (_timer >= _durationPerCharacter)
            {
                _timer -= _durationPerCharacter;
                _characterEffectIndex++;

                if (_characterEffectIndex < _textStripped.Length) continue;
                _characterEffectIndex = -1;
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
        /// Set the typewriter effect.
        /// </summary>
        /// <param name="duration"></param>
        public void SetTypewriterEffect(float duration)
        {
            ResetEffect();
            ResetMapping();

            _totalDuration = duration;
            _durationPerCharacter = _totalDuration / _textStripped.Length;
            _characterEffectIndex = 0;
        }

        /// <summary>
        /// Instantly end the typewriter effect.
        /// </summary>
        public void EndTypewriterEffect()
        {
            _characterEffectIndex = -1;
        }

        #endregion

        #region RichText API

        internal override void Render(Renderer renderer)
        {
            if (_updateRenderCache)
            {
                MapBuffer();
                _updateRenderCache = false;
            }

            // Check if anything is mapped in the cache buffer.
            if (!_renderCache.AnythingMapped) return;

            // Check if the model matrix needs to be calculated.
            if (_transformUpdated)
            {
                ModelMatrix = Matrix4.CreateTranslation(Position);
                _transformUpdated = false;
            }

            // Don't draw anything if the effect is before the first.
            if (_characterEffectIndex == 0) return;

            // Draw the buffer.
            if (_characterEffectIndex != -1) _renderCache.SetMappedIndices(_characterEffectIndex * 6);
            else _renderCache.SetMappedIndices(_textStripped.Length * 6);

            renderer.Render(_renderCache, true);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Resets and stops the Typewriter effect.
        /// </summary>
        protected void ResetEffect()
        {
            _timer = 0;
            _characterEffectIndex = 0;
        }

        /// <summary>
        /// Resets the character mapping.
        /// </summary>
        protected void ResetMapping()
        {
            // If any effect was already done, reset it.
            if (_characterEffectIndex != -1) ResetEffect();
            _penX = 0;
            _penY = 0;
        }

        #endregion
    }
}