#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.Text;

#endregion

namespace Emotion.Game.Text
{
    /// <summary>
    /// Layouts and fits and wraps text within a box.
    /// </summary>
    public class TextLayouterWrap : TextLayouter
    {
        private int _counter = 0;
        private List<int> _newLineIndices = new List<int>();

        public TextLayouterWrap(FontAtlas atlas) : base(atlas)
        {
        }

        public void SetupBox(string text, Vector2 bounds)
        {
            var currentLine = "";
            var breakSkipMode = false;
            int breakSkipModeLimit = -1;

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

                // Get the text to the next break.
                string textToBreak = text.Substring(i, nextBreakLocation - i);

                // Measure the current line with the new characters.
                Vector2 textSize = MeasureString(currentLine + textToBreak);

                // Check if the textToBreak is too big to fit on one line.
                Vector2 overflowCheck = MeasureString(textToBreak);

                // Check if the whole textToBreak cannot fit on a single line.
                // This is a rare case, but when it happens characters must be printed without performing break checks as they will either cause
                // each character to go on a separate line or cause a line break in the text as soon as it can fit on the line.
                // To do this we switch to a break skipping mode which ensures this other method of printing until the whole text is printed.
                if (overflowCheck.X > bounds.X || breakSkipMode)
                {
                    textSize = MeasureString(currentLine + text[i]);
                    breakSkipMode = true;
                    breakSkipModeLimit = i + textToBreak.Length;
                }

                // Break line if we don't have enough space to fit all the text to the next break, or if the current character is a break.
                if (textSize.X > bounds.X || text[i] == '\n')
                {
                    // Push new line.
                    //wrapResult.Add(currentLine);
                    _newLineIndices.Add(i);
                    currentLine = "";

                    // If the current character is a new line break retroactively push it on the last line, and continue without adding it to the current line.
                    if (text[i] == '\n' && _newLineIndices.Count > 0)
                    {
                        _newLineIndices[_newLineIndices.Count - 1] += 1;
                        //wrapResult[wrapResult.Count - 1] += '\n';
                        continue;
                    }
                }

                // Add the current character to the current line string.
                currentLine += text[i].ToString();
            }

            // If there is text left push in on a new line.
            //if (!string.IsNullOrEmpty(currentLine))
                //wrapResult.Add(currentLine);
        }

        /// <summary>
        /// Add a letter to the layouter.
        /// </summary>
        /// <param name="c">The letter to add.</param>
        /// <param name="g">The atlas glyph corresponding to the letter.</param>
        /// <returns>The draw position of the letter.</returns>
        public override Vector2 AddLetter(char c, out AtlasGlyph g)
        {
            if (_newLineIndices.IndexOf(_counter) != -1) NewLine();
            _counter++;

            return base.AddLetter(c, out g);
        }

        /// <summary>
        /// Restart the layouter.
        /// </summary>
        public override void Restart()
        {
            base.Restart();
            _newLineIndices.Clear();
            _counter = 0;
        }
    }
}