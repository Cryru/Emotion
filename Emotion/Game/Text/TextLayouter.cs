#region Using

using System.Numerics;
using Emotion.Standard.Text;

#endregion

namespace Emotion.Game.Text
{
    public class TextLayouter
    {
        private FontAtlas _atlas;
        private bool _hasZeroGlyph;
        private Vector2 _pen;

        public TextLayouter(FontAtlas atlas)
        {
            _atlas = atlas;
            _hasZeroGlyph = atlas.Glyphs.ContainsKey((char) 0);
            _pen = Vector2.Zero;
        }

        /// <summary>
        /// Returns the position of the next glyph.
        /// </summary>
        /// <param name="c">The character which is the next glyph.</param>
        /// <param name="drawPosition">The position to draw the character at.</param>
        /// <param name="g">The atlas glyph corresponding to the provided character, or null if none.</param>
        /// <returns>The position of the next glyph along the pen.</returns>
        public Vector2 GetNextGlyphPosition(char c, out Vector2 drawPosition, out AtlasGlyph g)
        {
            return GetNextGlyphPosition(_pen, c, out drawPosition, out g);
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
                result.Y += _atlas.FontHeight;
            }
            else
            {
                if (!_atlas.Glyphs.TryGetValue(c, out g))
                {
                    if (!_hasZeroGlyph)
                        return result;
                    g = _atlas.Glyphs[(char) 0];
                }

                drawPosition = result;
                drawPosition.Y += g.YBearing;
                drawPosition.X = g.XMin + drawPosition.X;
            }

            return result;
        }

        /// <summary>
        /// Add a letter to the layouter.
        /// </summary>
        /// <param name="c">The letter to add.</param>
        /// <param name="g">The atlas glyph corresponding to the letter.</param>
        /// <returns>The draw position of the letter.</returns>
        public Vector2 AddLetter(char c, out AtlasGlyph g)
        {
            Vector2 position = GetNextGlyphPosition(c, out Vector2 drawPosition, out g);
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
            _pen.Y += _atlas.FontHeight;
        }

        /// <summary>
        /// Restart the layouter.
        /// </summary>
        public void Restart()
        {
            _pen = new Vector2(0, 0);
        }

        /// <summary>
        /// Measure the provided string with the loaded atlas.
        /// Does not modify the pen position.
        /// </summary>
        public Vector2 MeasureString(string text)
        {
            var sizeSoFar = new Vector2(0, _atlas.FontHeight);
            float largestLine = 0;
            float largestBearing = 0;
            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // If jumping on a new line, check if the current line is the largest so far.
                if (c == '\n')
                {
                    if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
                }

                // Spaces on the end of lines are not counted.
                if (c == ' ' && (i == text.Length - 1 || text[i + 1] == '\n')) continue;

                Vector2 pos = GetNextGlyphPosition(sizeSoFar, c, out Vector2 _, out AtlasGlyph g);
                sizeSoFar = pos;
                if (g != null)
                {
                    sizeSoFar.X += g.Advance;

                    if (g.YBearing > largestBearing)
                    {
                        largestBearing = g.YBearing;
                    }
                }
            }

            sizeSoFar.Y -= largestBearing;

            if(largestLine != 0)
                sizeSoFar.X = largestLine;
            return sizeSoFar;
        }
    }
}