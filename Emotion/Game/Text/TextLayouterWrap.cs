#region Using

using Emotion.Graphics.Text;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Text
{
	public enum GlyphHeightMeasurement : byte
	{
		FullHeight,
		NoDescent,
		NoMinY,
		Underflow,
		UnderflowLittle
	}

	/// <summary>
	/// Layouts and fits and wraps text within a box.
	/// </summary>
	public class TextLayouterWrap : TextLayouter
	{
		public float NeededWidth { get; private set; }
		public float NeededHeight { get; private set; }

		private int _counter;
		private List<int> _newLineIndices = new List<int>();

		// Offset for extremely tight height. Used on one line strings in some cases.
		// The box must have been setup with tightHeight and underflow.
		private float _singleLineNegativeY;

		public TextLayouterWrap(DrawableFontAtlas atlas) : base(atlas)
		{
		}

		public void SetupBox(string text, Vector2 bounds, GlyphHeightMeasurement measureMode = GlyphHeightMeasurement.FullHeight)
		{
			_atlas.CacheGlyphs(text);

			var currentLineStart = 0;
			var currentLineLength = 0;

			var breakSkipMode = false;
			int breakSkipModeLimit = -1;
			float lineHeight = _atlas.FontHeight;
			float longestLine = 0;

			// Loop through the text.
			for (var i = 0; i < text.Length; i++)
			{
				// Check if exiting break skip mode.
				if (breakSkipModeLimit == i) breakSkipMode = false;

				// Find the location of the next space or new line character.
				int nextSpaceLocation = text.IndexOf(' ', i);
				int nextNewLineLocation = text.IndexOf('\n', i);
				int nextBreakLocation;

				if (nextNewLineLocation == -1 && nextSpaceLocation == -1)
					nextBreakLocation = text.Length;
				else if (nextSpaceLocation == -1)
					nextBreakLocation = nextNewLineLocation;
				else if (nextNewLineLocation == -1)
					nextBreakLocation = nextSpaceLocation;
				else
					nextBreakLocation = Math.Min(nextNewLineLocation, nextSpaceLocation);

				// Measure the current line with the new characters.
				Vector2 textSize = MeasureString(text, currentLineStart, nextBreakLocation - currentLineStart);

				// Check if the whole textToBreak cannot fit on a single line.
				// This is a rare case, but when it happens characters must be printed without performing break checks as they will either cause
				// each character to go on a separate line or cause a line break in the text as soon as it can fit on the line.
				// To do this we switch to a break skipping mode which ensures this other method of printing until we reach the next break.
				Vector2 overflowCheck = MeasureString(text, i, nextBreakLocation - i);
				if (overflowCheck.X > bounds.X || breakSkipMode)
				{
					textSize = MeasureString(text, currentLineStart, currentLineLength + 1);
					breakSkipMode = true;
					breakSkipModeLimit = nextBreakLocation;
				}

				// Break line if we don't have enough space to fit all the text to the next break, or if the current character is a break.
				bool lineBreakChar = text[i] == '\n';
				if ((textSize.X > bounds.X || lineBreakChar) && currentLineLength != 0)
				{
					// Remove trailing space when breaking to a new line.
					if (text[currentLineStart + currentLineLength - 1] == ' ') currentLineLength--;

					// Update measures.
					Vector2 lineSize = MeasureString(text, currentLineStart, currentLineLength);
					if (lineSize.X > longestLine) longestLine = lineSize.X;
					NeededHeight += MathF.Max(lineHeight, MathF.Floor(lineSize.Y));
					Assert(lineHeight >= MathF.Floor(lineSize.Y));

					// Push new line.
					if (!lineBreakChar) _newLineIndices.Add(i); // The new line here is handled by the TextLayouter.
					currentLineStart = lineBreakChar ? i + 1 : i;
					currentLineLength = 0;
				}

				// Add the current character to the current line string.
				if (!lineBreakChar) currentLineLength++;
			}

			// If there is text left, push it onto the measurement too.
			if (currentLineLength != 0)
			{
				Vector2 lastLine = MeasureString(text, currentLineStart, currentLineLength);
				switch (measureMode)
				{
					case GlyphHeightMeasurement.FullHeight:
						NeededHeight += lineHeight;
						_singleLineNegativeY = 0;
						break;
					case GlyphHeightMeasurement.NoDescent:
						NeededHeight = lastLine.Y;
						_singleLineNegativeY = 0;
						break;
					case GlyphHeightMeasurement.UnderflowLittle:
					case GlyphHeightMeasurement.NoMinY:
					case GlyphHeightMeasurement.Underflow:
						MeasureStringsHeight(
							out float largestHeight, out float smallestHeight, out float yOffset,
							text, currentLineStart, currentLineLength
						);
						if (measureMode == GlyphHeightMeasurement.Underflow)
						{
							NeededHeight += smallestHeight;
							_singleLineNegativeY = yOffset - smallestHeight;
						}
						else if (measureMode == GlyphHeightMeasurement.UnderflowLittle)
						{
							float halfWay = Maths.Lerp(smallestHeight, largestHeight, 0.5f);
							NeededHeight += halfWay;
							_singleLineNegativeY = yOffset - halfWay;
						}
						else
						{
							NeededHeight += largestHeight;
							_singleLineNegativeY = yOffset - largestHeight;
						}

						break;
				}

				if (lastLine.X > longestLine) longestLine = lastLine.X;
			}

			NeededWidth = longestLine;
		}

		/// <summary>
		/// Add a letter to the layouter.
		/// </summary>
		/// <param name="c">The letter to add.</param>
		/// <param name="g">The atlas glyph corresponding to the letter.</param>
		/// <returns>The draw position of the letter.</returns>
		public override Vector2 AddLetter(char c, out DrawableGlyph g)
		{
			if (_newLineIndices.IndexOf(_counter) != -1) NewLine();
			_counter++;

			Vector2 position = base.AddLetter(c, out g);
			if (_singleLineNegativeY != 0) position.Y -= _singleLineNegativeY;
			return position;
		}

		public bool IsNextCharacterGoingToWrap()
		{
			return _newLineIndices.IndexOf(_counter) != -1;
		}

		/// <summary>
		/// Restart only the pen position.
		/// </summary>
		public void RestartPen()
		{
			base.Restart();
			_counter = 0;
		}

		/// <summary>
		/// Restart the layouter.
		/// </summary>
		public override void Restart()
		{
			base.Restart();
			_newLineIndices.Clear();
			_counter = 0;
			NeededHeight = 0;
			_singleLineNegativeY = 0;
		}
	}
}