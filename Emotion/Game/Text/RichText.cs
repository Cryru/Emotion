#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Text
{
    /// <summary>
    /// A RichText object which manages text wrapping, styles, tagging, and more.
    /// </summary>
    public class RichText : TransformRenderable
    {
        #region Global Settings

        /// <summary>
        /// Characters not to render. Updates when the RichText text is changed.
        /// </summary>
        public char[] CharactersToNotRender { get; set; } = { '\n' };

        /// <summary>
        /// The character to separate a tag from its attributes. Updates when the RichText parses tags.
        /// </summary>
        public char TagAttributeSeparator { get; set; } = '-';

        #endregion

        #region Properties

        /// <summary>
        /// The way to align text.
        /// </summary>
        public TextAlignment Alignment
        {
            get => _alignment;
            set
            {
                _alignment = value;
                SetText(Text);
            }
        }

        private TextAlignment _alignment = TextAlignment.Left;

        /// <summary>
        /// The text to render.
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetText(value);
        }

        private string _text = "";

        /// <summary>
        /// The text to render with tags removed.
        /// </summary>
        public string ProcessedText
        {
            get => _textStripped;
        }

        /// <summary>
        /// The font atlas to use when rendering.
        /// </summary>
        public DrawableFontAtlas FontAtlas { get; set; }

        #endregion

        #region State

        #region Objects

        /// <summary>
        /// The text layouter.
        /// </summary>
        protected TextLayouter _layouter { get; set; }

        #endregion

        #region Text Parsing

        /// <summary>
        /// Parsed effects.
        /// </summary>
        protected List<TextEffect> _effectCache { get; set; } = new List<TextEffect>();

        /// <summary>
        /// The text stripped of effect tags.
        /// </summary>
        protected string _textStripped { get; set; } = "";

        /// <summary>
        /// The lines of the text after wrap processing.
        /// </summary>
        protected List<string> _wrapCache { get; set; } = new List<string>();

        /// <summary>
        /// Used by justification alignment to make spaces larger.
        /// </summary>
        protected List<int> _spaceWeight { get; set; } = new List<int>();

        /// <summary>
        /// // Used by alignments to push the line.
        /// </summary>
        protected List<int> _initialLineIndent { get; set; } = new List<int>();

        #endregion

        #region Mapping

        protected char _prevChar;

        #endregion

        #endregion

        /// <summary>
        /// Create a new RichText object.
        /// </summary>
        /// <param name="position">The position of the RichText.</param>
        /// <param name="size">The size of the RichText.</param>
        /// <param name="fontAtlas">The font atlas to use.</param>
        public RichText(Vector3 position, Vector2 size, DrawableFontAtlas fontAtlas) : base(position, size)
        {
            FontAtlas = fontAtlas;
            _layouter = new TextLayouter(fontAtlas);
        }

        /// <summary>
        /// Sets the RichText's text.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public virtual void SetText(string text)
        {
            _text = text;

            // Find effects.
            _textStripped = ParseEffects(Text, _effectCache);

            // Apply wrapping.
            Wrap(_textStripped, Size, _wrapCache);

            // Apply text alignment.
            ProcessAlignment();
        }

        #region Text Processing

        /// <summary>
        /// Parse the provided string for tags and effects. Returns the string stripped of them, and populates a list with them.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="parsedEffects">The list to populate with the parsing results.</param>
        /// <returns>The string without any tags in it.</returns>
        protected virtual string ParseEffects(string text, List<TextEffect> parsedEffects)
        {
            parsedEffects.Clear();

            var tagOpened = false;
            var currentTag = "";
            var strippedText = "";

            // Iterate characters, and extract tags.
            foreach (char c in text)
            {
                switch (c)
                {
                    // Check if the character is an opening tag.
                    case '<':
                        // Check if a tag is already opened.
                        tagOpened = !tagOpened;
                        break;
                    // Check if the character is a closing tag, and a tag is opened.
                    case '>' when tagOpened:
                        // Process the tag.
                        ProcessTag(currentTag, strippedText.Length, parsedEffects);
                        tagOpened = false;
                        currentTag = "";
                        break;
                    // All other characters.
                    default:
                        // Check if reading a tag, in which case add the character to the tag text.
                        if (tagOpened)
                            currentTag += c;
                        // If not, add the character to the text to display.
                        else
                            strippedText += c;
                        break;
                }
            }

            return strippedText;
        }

        /// <summary>
        /// Processes the provided tag and add it to the effects cache. Used by ParseEffects. Can be overloaded.
        /// </summary>
        /// <param name="tag">The tag data captured.</param>
        /// <param name="position">The position of the tag within the text to display.</param>
        /// <param name="parsedEffects">Effects parsed thus far.</param>
        protected virtual void ProcessTag(string tag, int position, List<TextEffect> parsedEffects)
        {
            // Check if closing tag.
            if (tag == "/")
            {
                // Close the last opened tag.
                TextEffect toClose = parsedEffects.Last(t => t.End == -1);
                if (toClose != null) toClose.End = position - 1;
            }
            else
            {
                string[] tagDataSplit = tag.Split('=');
                string[] attributes = tagDataSplit.Length > 1 ? tagDataSplit[1].Split(TagAttributeSeparator) : null;

                parsedEffects.Add(new TextEffect
                {
                    Start = position,
                    Name = tagDataSplit[0].ToLower(),
                    Attributes = attributes
                });
            }
        }

        /// <summary>
        /// Wraps the text to fit the provided bounds, making sure it doesn't exceed them.
        /// </summary>
        /// <param name="text">The text to wrap.</param>
        /// <param name="wrapBounds">The bounds to wrap it in.</param>
        /// <param name="wrapResult">The list to populate with the wrapping result. Each line is a new entry.</param>
        protected virtual void Wrap(string text, Vector2 wrapBounds, List<string> wrapResult)
        {
            wrapResult.Clear();

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
                Vector2 textSize = _layouter.MeasureString(currentLine + textToBreak);

                // Check if the textToBreak is too big to fit on one line.
                Vector2 overflowCheck = _layouter.MeasureString(textToBreak);

                // Check if the whole textToBreak cannot fit on a single line.
                // This is a rare case, but when it happens characters must be printed without performing break checks as they will either cause
                // each character to go on a separate line or cause a line break in the text as soon as it can fit on the line.
                // To do this we switch to a break skipping mode which ensures this other method of printing until the whole text is printed.
                if (overflowCheck.X > wrapBounds.X || breakSkipMode)
                {
                    textSize = _layouter.MeasureString(currentLine + text[i]);
                    breakSkipMode = true;
                    breakSkipModeLimit = i + textToBreak.Length;
                }

                // Break line if we don't have enough space to fit all the text to the next break, or if the current character is a break.
                if (textSize.X > wrapBounds.X || text[i] == '\n')
                {
                    // Push new line.
                    wrapResult.Add(currentLine);
                    currentLine = "";

                    // If the current character is a new line break retroactively push it on the last line, and continue without adding it to the current line.
                    if (text[i] == '\n')
                    {
                        wrapResult[wrapResult.Count - 1] += '\n';
                        continue;
                    }
                }

                // Add the current character to the current line string.
                currentLine += text[i].ToString();
            }

            // If there is text left push in on a new line.
            if (!string.IsNullOrEmpty(currentLine))
                wrapResult.Add(currentLine);
        }

        /// <summary>
        /// Process text alignment.
        /// </summary>
        private void ProcessAlignment()
        {
            _spaceWeight.Clear();
            _initialLineIndent.Clear();

            // Go through all lines.
            for (var i = 0; i < _wrapCache.Count; i++)
            {
                if (Alignment == TextAlignment.Justified || Alignment == TextAlignment.JustifiedCenter) AlignJustify(i);
                switch (Alignment)
                {
                    case TextAlignment.Centered:
                    case TextAlignment.JustifiedCenter:
                        AlignCenter(i);
                        break;
                    case TextAlignment.Right:
                        AlignRight(i);
                        break;
                }
            }
        }

        #endregion

        #region Alignments

        private void AlignJustify(int i)
        {
            var charSpacing = 0;

            // Calculate and check.
            float lineSize = _layouter.MeasureString(_wrapCache[i]).X;
            int spaces = CountSpaces(_wrapCache[i]);
            bool lastLine = i == _wrapCache.Count - 1;
            bool manualNewline = !lastLine && _wrapCache[i][_wrapCache[i].Length - 1] == '\n';

            // Do not attempt to justify if the line:
            // - Has no size.
            // - Is one character or less.
            // - Has no space characters.
            // - Is the last line.
            // - Is a manual break.
            if (lineSize == 0 || _wrapCache[i].Length <= 1 || spaces == 0 || lastLine || manualNewline)
            {
                _spaceWeight.Add(0);
                return;
            }

            float characterSpace = 1;
            while (characterSpace >= 0)
            {
                // Get the space left on the line by subtracting the total width from the line's width plus character spacing, minus one because of the last character.
                characterSpace = Width - (lineSize + charSpacing * spaces);

                // If there is space, increase char spacing.
                charSpacing++;
            }

            // Decrease by one.
            if (charSpacing > 0) charSpacing--;

            _spaceWeight.Add(charSpacing);
        }

        private void AlignCenter(int i)
        {
            float spaceLeft = Width - _layouter.MeasureString(_wrapCache[i]).X;

            // Get justification character spacing. if any.
            if (i < _spaceWeight.Count) spaceLeft -= _spaceWeight[i] * CountSpaces(_wrapCache[i]);

            // Check if justifying by center, and not on the first line.
            if (Alignment == TextAlignment.JustifiedCenter && i != 0 && spaceLeft / 2 >= _initialLineIndent[0])
            {
                // Set the indent to be that of the first line.
                _initialLineIndent.Add(_initialLineIndent[0]);
            }
            else
            {
                if (spaceLeft > 0)
                    _initialLineIndent.Add((int)(spaceLeft / 2));
                else
                    _initialLineIndent.Add(0);
            }
        }

        private void AlignRight(int i)
        {
            float spaceLeft = Width - _layouter.MeasureString(_wrapCache[i]).X;

            // To align right set the free space before the line.
            _initialLineIndent.Add((int)spaceLeft);
        }

        #endregion

        #region Drawing

        public override void Render(RenderComposer composer)
        {
            Render(composer, -1);
        }

        public void Render(RenderComposer composer, int upToChar)
        {
            // Start mapping.
            _layouter.Restart();
            _prevChar = '\0';

            FontAtlas.SetupDrawing(composer, _text);
            Vector2 drawPadding = FontAtlas.GlyphDrawPadding;
            Vector2 drawPaddingT2 = drawPadding * 2;
            var drawPadding3 = new Vector3(drawPadding.X, drawPadding.Y, 0);

            // Iterate virtual lines.
            var characterCounter = 0;
            for (var line = 0; line < _wrapCache.Count; line++)
            {
                // Iterate virtual characters.
                for (var c = 0; c < _wrapCache[line].Length; c++)
                {
                    var glyphXOffset = 0;

                    // Apply space size multiplication if the current character is a space.
                    if (line < _spaceWeight.Count && _wrapCache[line][c] == ' ') glyphXOffset += _spaceWeight[line];

                    // Check if applying initial indent.
                    if (line < _initialLineIndent.Count && c == 0) glyphXOffset += _initialLineIndent[line];

                    // Get all active effects for the current index.
                    IEnumerable<TextEffect> effects = GetEffectsAt(characterCounter);

                    // Tag changeable properties.
                    Color textColor = Color.White;

                    // Apply effects.
                    foreach (TextEffect e in effects)
                    {
                        if (e.Name == "color" && e.Attributes?.Length >= 3)
                            textColor = new Color(Helpers.StringToInt(e.Attributes[0]), Helpers.StringToInt(e.Attributes[1]), Helpers.StringToInt(e.Attributes[2]));
                    }

                    char charUnicode = _wrapCache[line][c];

                    // Check if rendering a character we don't want visible, in which case we replace it with a space.
                    if (CharactersToNotRender.Contains(charUnicode)) charUnicode = ' ';

                    _layouter.AddToPen(new Vector2(glyphXOffset, 0));
                    Vector2 drawPos = _layouter.AddLetter(charUnicode, out AtlasGlyph g);

                    if (g != null)
                    {
                        var uv = new Rectangle(g.UVLocation, g.UVSize);
                        composer.RenderSprite(Position + drawPos.ToVec3() - drawPadding3, g.Size + drawPaddingT2, textColor, FontAtlas.Texture, uv);
                    }

                    // Increment character counter.
                    characterCounter++;
                    if (characterCounter == upToChar) return;
                }

                _layouter.NewLine();
            }

            FontAtlas.FinishDrawing(composer);
        }

        #endregion

        #region Position Translation API

        /// <summary>
        /// Returns the virtual lines which the real line corresponds to.
        /// </summary>
        /// <param name="realLineIndex">The real line index.</param>
        /// <returns>The virtual lines which the real line corresponds to.</returns>
        protected List<int> GetVirtualLinesIndexesFromReal(int realLineIndex)
        {
            // Get the number of real lines.
            int realLines = _textStripped.Split('\n').Length;

            // Find the index of all newlines.
            var currentLineEndIndex = 0;

            for (var i = 0; i < realLines; i++)
            {
                int currentLineStartIndex = currentLineEndIndex;
                currentLineEndIndex = _textStripped.IndexOf('\n', currentLineEndIndex + 1);
                if (currentLineEndIndex == -1) currentLineEndIndex = _textStripped.Length;

                // Check if the current real line is the one we are processing.
                if (i != realLineIndex) continue;

                // Find which virtual lines contain the real line start and end.
                int virtualStartLineIndex = GetVirtualLineIndexFromRealCharIndex(currentLineStartIndex);
                int virtualEndLineIndex = GetVirtualLineIndexFromRealCharIndex(currentLineEndIndex - 1);

                // Return all lines between them.
                var result = new List<int>();
                for (int j = virtualStartLineIndex; j <= virtualEndLineIndex; j++)
                {
                    result.Add(j);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Returns the index of the virtual line containing the real character index.
        /// </summary>
        /// <param name="realCharIndex">The real character index to find.</param>
        /// <returns>The index of the virtual line containing the real character index.</returns>
        protected int GetVirtualLineIndexFromRealCharIndex(int realCharIndex)
        {
            var characterCounter = 0;

            for (var line = 0; line < _wrapCache.Count; line++)
            {
                for (var c = 0; c < _wrapCache[line].Length; c++)
                {
                    if (characterCounter == realCharIndex) return line;

                    characterCounter++;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the virtual character on its virtual line corresponding to the real character index.
        /// </summary>
        /// <param name="realCharIndex">The real character index to find.</param>
        /// <returns>The index of the virtual character on its virtual line corresponding to the real character index.</returns>
        protected int GetVirtualCharacterIndexFromRealCharIndex(int realCharIndex)
        {
            var characterCounter = 0;

            foreach (string line in _wrapCache)
            {
                for (var c = 0; c < line.Length; c++)
                {
                    if (characterCounter == realCharIndex) return c;

                    characterCounter++;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns a list of all active effects at the specified character index.
        /// </summary>
        /// <param name="index">The index to return all active effects at.</param>
        /// <returns>A list of all active effects at the specified index.</returns>
        protected IEnumerable<TextEffect> GetEffectsAt(int index)
        {
            return _effectCache.Where(t => index >= t.Start && (index <= t.End || index == t.End && index == t.Start));
        }

        /// <summary>
        /// Returns the number of spaces found in the string.
        /// </summary>
        /// <param name="text">The string to count spaces in.</param>
        /// <returns>The number of spaces found in the string.</returns>
        private static int CountSpaces(string text)
        {
            int spaces = text.Count(x => x == ' ');

            // Decrease spaces by one if the last character is a space.
            bool lastCharacterIsSpace = text.Length > 0 && text[^1] == ' ';
            if (lastCharacterIsSpace) spaces--;

            return spaces;
        }

        #endregion
    }
}