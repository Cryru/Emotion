#region Using

using Emotion.Graphics.Text;

#endregion

namespace Emotion.Game.Text
{
	public class TextLayouter
	{
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
		public Vector2 GetNextGlyphPosition(Vector2 pen, char c, out Vector2 drawPosition, out DrawableGlyph g)
		{
			var result = new Vector2(pen.X, pen.Y);
			drawPosition = Vector2.Zero;
			g = null;

			if (c == '\n')
			{
				result.X = 0;
				result.Y += MathF.Round(_atlas.FontHeight);
			}
			else
			{
				if (!_atlas.Glyphs.TryGetValue(c, out g))
				{
					if (!_hasZeroGlyph) return result;
					g = _atlas.Glyphs[(char) 0];
				}

				drawPosition = result;
				drawPosition.X += g.XBearing;
			}

			return result;
		}

		/// <summary>
		/// Add a letter to the layouter.
		/// </summary>
		/// <param name="c">The letter to add.</param>
		/// <param name="g">The atlas glyph corresponding to the letter.</param>
		/// <returns>The draw position of the letter.</returns>
		public virtual Vector2 AddLetter(char c, out DrawableGlyph g)
		{
			Vector2 position = GetNextGlyphPosition(_pen, c, out Vector2 drawPosition, out g);
			_pen = position;
			if (g == null) return position;
			_pen.X += g.XAdvance;
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
			_pen.Y += MathF.Round(_atlas.FontHeight);
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
			_hasZeroGlyph = atlas.Glyphs.ContainsKey((char) 0);
		}

		/// <summary>
		/// Measure the provided string with the loaded atlas.
		/// Does not modify the pen position.
		/// The size returned is not the actual render size of the text, but it can be used to coordinate it.
		/// </summary>
		public Vector2 MeasureString(string text, int stringStart = 0, int length = -1)
		{
			if (length == -1) length = text.Length - stringStart;

			var sizeSoFar = new Vector2(0, 0);
			float largestLine = 0;
			float tallestOnLine = 0;
			for (int i = stringStart; i < stringStart + length; i++)
			{
				char c = text[i];

				// If jumping on a new line, check if the current line is the largest so far.
				if (c == '\n')
				{
					if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
					sizeSoFar.Y += tallestOnLine;
					tallestOnLine = 0;
				}

				Vector2 pos = GetNextGlyphPosition(sizeSoFar, c, out Vector2 _, out DrawableGlyph g);
				sizeSoFar = pos;
				if (g == null) continue;

				sizeSoFar.X += g.XAdvance;
				float verticalSize = g.Height + (_atlas.Ascent - g.Height) + g.Descent;
				if (verticalSize > tallestOnLine) tallestOnLine = verticalSize;
			}

			sizeSoFar.Y += tallestOnLine;

			if (sizeSoFar.X > largestLine) largestLine = sizeSoFar.X;
			if (largestLine != 0) sizeSoFar.X = largestLine;

			return sizeSoFar;
		}

		/// <summary>
		/// Get the height of the tallest glyph in the string.
		/// </summary>
		public void MeasureStringsHeight(out float largestHeight, out float smallestHeight, out float fontYOffset, string text, int stringStart = 0, int length = -1)
		{
			if (length == -1) length = text.Length - stringStart;

			// Replace space with a wide lowercase character (such as w) since
			// some fonts specify space as having 0 height.
			_atlas.CacheGlyphs("w");
			bool replaceSpace = _atlas.Glyphs.ContainsKey('w');

			fontYOffset = _atlas.Ascent;
			largestHeight = 0;
			smallestHeight = float.MaxValue;
			for (int i = stringStart; i < stringStart + length; i++)
			{
				char c = text[i];

				// If going on a new line, stop checking and return current height.
				if (c == '\n') return;

				if (c == ' ' && replaceSpace) c = 'w';

				if (!_atlas.Glyphs.TryGetValue(c, out DrawableGlyph g))
				{
					if (_hasZeroGlyph)
						g = _atlas.Glyphs[(char) 0];
					else
						continue;
				}

				largestHeight = MathF.Max(largestHeight, g.Height);
				smallestHeight = MathF.Min(smallestHeight, g.Height);
			}

			if (smallestHeight == float.MaxValue) smallestHeight = 0;
		}
	}
}