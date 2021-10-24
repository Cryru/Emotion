#region Using

using System;
using System.Numerics;
using Emotion.Graphics.Text;

#endregion

namespace Emotion.Game.Text
{
    public class TextLayouter
    {
        public float LineGap { get; set; }

        /// <summary>
        /// Whether MeasureString should include any whitespace characters at the end of the string.
        /// </summary>
        public bool MeasureTrailingWhiteSpace { get; set; } = false;

        protected DrawableFontAtlas _atlas;
        protected bool _hasZeroGlyph;
        protected Vector2 _pen;

        public TextLayouter(DrawableFontAtlas atlas)
        {
            SetAtlas(atlas);
        }

        /// <summary>
        /// Returns the position of the next glyph.
        /// </summary>
        /// <param name="pen">The position of the pen.</param>
        /// <param name="c">The character which is the next glyph.</param>
        /// <param name="drawPosition">The position to draw the character at.</param>
        /// <param name="g">The atlas glyph corresponding to the provided character, or null if none.</param>
        /// <returns>The position of the next glyph along the pen.</returns>
        public Vector2 GetNextGlyphPosition(Vector2 pen, char c, out Vector2 drawPosition, out AtlasGlyph g)
        {
            var result = new Vector2(pen.X, pen.Y);
            drawPosition = Vector2.Zero;
            g = null;

            if (c == '\n')
            {
                result.X = 0;
                result.Y += _atlas.FontHeight + LineGap;
            }
            else
            {
                if (!_atlas.Glyphs.TryGetValue(c, out g))
                {
                    if (!_hasZeroGlyph)
                        return result;
                    g = _atlas.Glyphs[(char)0];
                }

                drawPosition = result;
                drawPosition.Y += g.YBearing;
                drawPosition.X += g.XMin;
            }

            return result;
        }

        /// <summary>
        /// Add a letter to the layouter.
        /// </summary>
        /// <param name="c">The letter to add.</param>
        /// <param name="g">The atlas glyph corresponding to the letter.</param>
        /// <returns>The draw position of the letter.</returns>
        public virtual Vector2 AddLetter(char c, out AtlasGlyph g)
        {
            Vector2 position = GetNextGlyphPosition(_pen, c, out Vector2 drawPosition, out g);
            _pen = position;
            if (g == null) return position;
            _pen.X += g.Advance;
            return drawPosition;
        }

        /// <summary>
        /// Add an offset to the pen's current position.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        public void AddToPen(Vector2 amount)
        {
            _pen += amount;
        }

        /// <summary>
        /// Force the pen to go on a new line.
        /// </summary>
        public void NewLine()
        {
            _pen.X = 0;
            _pen.Y += _atlas.FontHeight + LineGap;
        }

        /// <summary>
        /// Restart the layouter.
        /// </summary>
        public virtual void Restart()
        {
            _pen = new Vector2(0, 0);
        }

        public Vector2 GetPenLocation()
        {
            return _pen;
        }

        /// <summary>
        /// Set a new font atlas.
        /// </summary>
        /// <param name="atlas">The atlas to set.</param>
        public void SetAtlas(DrawableFontAtlas atlas)
        {
            Restart();
            _atlas = atlas;
            _hasZeroGlyph = atlas.Glyphs.ContainsKey((char)0);
        }

        /// <summary>
        /// Measure the provided string with the loaded atlas.
        /// Does not modify the pen position.
        /// The size returned is not the actual render size of the text, but it can be used to coordinate it.
        /// </summary>
        public Vector2 MeasureString(string text)
        {
            var sizeSoFar = new Vector2(0, 0);
            float largestLine = 0;
            float tallestOnLine = 0;
            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // If jumping on a new line, check if the current line is the largest so far.
                if (c == '\n')
                {
                    if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
                    sizeSoFar.Y += tallestOnLine;
                    tallestOnLine = 0;
                }

                // Spaces on the end of lines are not counted.
                if (!MeasureTrailingWhiteSpace && c == ' ' && (i == text.Length - 1 || text[i + 1] == '\n')) continue;

                Vector2 pos = GetNextGlyphPosition(sizeSoFar, c, out Vector2 _, out AtlasGlyph g);
                sizeSoFar = pos;
                if (g == null) continue;

                sizeSoFar.X += g.Advance;
                float verticalSize = g.Size.Y + g.YBearing + g.YMin;
                if (verticalSize > tallestOnLine) tallestOnLine = verticalSize;
            }

            sizeSoFar.Y += tallestOnLine;

            if (largestLine != 0)
                sizeSoFar.X = largestLine;
            return sizeSoFar;
        }

        /// <summary>
        /// Get the height of the tallest glyph in the string.
        /// </summary>
        public void MeasureStringsHeight(string text, out float largestHeight, out float smallestHeight, out float fontYOffset)
        {
            fontYOffset = 0;
            largestHeight = 0;
            smallestHeight = float.MaxValue;
            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // If going on a new line, stop checking and return current height.
                if (c == '\n') return;

                // Skip space as it has wacky height.
                if (c == ' ') continue;

                if (!_atlas.Glyphs.TryGetValue(c, out AtlasGlyph g))
                {
                    if (_hasZeroGlyph)
                        g = _atlas.Glyphs[(char)0];
                    else
                        continue;
                }

                fontYOffset = g.YOffset; // todo: move to font.

                largestHeight = MathF.Max(largestHeight, g.Height);
                smallestHeight = MathF.Min(smallestHeight, g.Height);
            }
        }
    }
}