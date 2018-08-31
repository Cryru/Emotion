// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

using System;
using Emotion.Primitives;
using System.Linq;
using Emotion.Debug;

namespace Emotion.Graphics.Text
{
    /// <inheritdoc />
    public sealed class TypewriterRichText : RichText
    {
        #region Properties

        public bool EffectFinished
        {
            get => _effectCharacterLimit == -1 || _characterMapNext == _textStripped.Length;
        }

        private float _totalDuration;
        private float _durationPerCharacter;
        private float _timer;
        private int _effectCharacterLimit = -1;
        private int _characterMapNext = -1;

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
            if (_effectCharacterLimit >= _textStripped.Length) return;

            // Update the timer.
            _timer += dt;

            // Unload timer.
            while (_timer >= _durationPerCharacter)
            {
                _timer -= _durationPerCharacter;
                _effectCharacterLimit++;
                if (_effectCharacterLimit >= _textStripped.Length) return;
            }
        }

        /// <inheritdoc />
        public override void SetText(string text)
        {
            base.SetText(text);

            if(_totalDuration != 0) _durationPerCharacter = _totalDuration / _textStripped.Length;
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
            _effectCharacterLimit = 0;
        }

        /// <summary>
        /// Instantly end the typewriter effect.
        /// </summary>
        public void EndTypewriterEffect()
        {
            _effectCharacterLimit = -1;
        }

        #endregion

        #region RichText API

        protected override void MapBuffer()
        {
            // Get virtual positions for resume.
            int virtualLine = GetVirtualLineIndexFromRealCharIndex(_characterMapNext);
            int virtualChar = GetVirtualCharacterIndexFromRealCharIndex(_characterMapNext);

            // Check if any invalid positions were returned.
            if (virtualLine == -1 || virtualChar == -1) return;

            // Start mapping.
            _renderCache.Start(false);
            _renderCache.FastForward(_characterMapNext);

            // Iterate virtual lines.
            for (int line = virtualLine; line < _wrapCache.Count; line++)
            {
                // Iterate virtual characters.
                for (int c = virtualChar; c < _wrapCache[line].Length; c++)
                {
                    // Check if past typewriter effect threshold.
                    if (_characterMapNext > _effectCharacterLimit)
                    {
                        _renderCache.FinishMapping();
                        return;
                    }

                    int glyphXOffset = 0;

                    // Apply space size multiplication if the current character is a space.
                    if (line < _spaceWeight.Count && _wrapCache[line][c] == ' ') glyphXOffset += _spaceWeight[line];

                    // Check if applying initial indent.
                    if (line < _initialLineIndent.Count && c == 0) glyphXOffset += _initialLineIndent[line];

                    // Check if rendering a character we don't want visible, in which case we just increment the pen.
                    if (CharactersToNotRender.Contains(_wrapCache[line][c]))
                        Push(glyphXOffset);
                    else
                        ProcessGlyph(line, c, _characterMapNext, glyphXOffset);

                    // Increment character counter.
                    _characterMapNext++;
                }

                virtualChar = 0;
                NewLine();
            }

            // Finish mapping.
            _renderCache.FinishMapping();
        }

        public override void Draw(Renderer _)
        {
            if (_updateRenderCache)
            {
                ResetMapping();
                _updateRenderCache = false;
            }

            // Check if any new characters to map.
            if (_characterMapNext <= _effectCharacterLimit)
            {
                MapBuffer();
            }

            // Check if anything is mapped in the cache buffer.
            if (!_renderCache.AnythingMapped) return;

            // Check if the model matrix needs to be calculated.
            if (_transformUpdated)
            {
                _modelMatrix = Matrix4.CreateTranslation(Bounds.X, Bounds.Y, Z);
                _transformUpdated = false;
            }

            // Draw the buffer. The model matrix is set here so we don't have to remap the buffer when the position is changed.
            _renderCache.Draw(_modelMatrix);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Resets and stops the Typewriter effect.
        /// </summary>
        private void ResetEffect()
        {
            _timer = 0;
            _effectCharacterLimit = 0;
        }

        /// <summary>
        /// Resets the character mapping.
        /// </summary>
        private void ResetMapping()
        {
            // If any effect was already done, reset it.
            if (_effectCharacterLimit != -1) ResetEffect();
            _characterMapNext = 0;
            _penX = 0;
            _penY = 0;
        }

        #endregion
    }
}