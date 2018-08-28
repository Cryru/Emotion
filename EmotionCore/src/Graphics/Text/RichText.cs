// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Graphics.Batching;
using Emotion.Primitives;
using Convert = Soul.Convert;

#endregion

namespace Emotion.Graphics.Text
{
    /// <summary>
    /// A RichText object which manages text wrapping, styles, tagging, and more.
    /// </summary>
    public class RichText : Renderable
    {
        #region Properties

        /// <summary>
        /// The way to align text.
        /// </summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;

        /// <summary>
        /// The text to render.
        /// </summary>
        public string Text { get; private set; } = "";

        /// <summary>
        /// The size of the text to render.
        /// </summary>
        public uint TextSize { get; private set; } = 10;

        #endregion

        #region Configuration

        public static char[] CharactersToNotRender = {'\n'};
        public static char TagAttributeSeparator = '-';

        #endregion

        #region State

        // Model Matrix.
        protected Matrix4 _modelMatrix;

        // Objects
        protected Font _font { get; set; }
        protected QuadMapBuffer _renderCache { get; set; }
        protected bool _updateRenderCache { get; set; }

        // Text parsing.
        protected string _textToDisplay { get; set; }
        protected List<TextEffect> _effectCache { get; set; } = new List<TextEffect>();
        protected List<string> _wrapCache { get; set; } = new List<string>();
        protected List<int> _spaceSize { get; set; } = new List<int>();
        protected List<int> _initialLineIndent { get; set; } = new List<int>();

        // Mapping
        private float _penX;
        private float _penY;
        private char _prevChar;

        #endregion

        /// <summary>
        /// Create a new RichText object.
        /// </summary>
        /// <param name="bounds">The bounds of the RichText.</param>
        /// <param name="font">The font to use.</param>
        public RichText(Rectangle bounds, Font font)
        {
            Bounds = bounds;
            _font = font;
            _renderCache = new QuadMapBuffer(Renderer.MaxRenderable);
        }

        #region Public API

        /// <summary>
        /// Sets the RichText's text and size.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="size">The text size.</param>
        public void SetText(string text, uint size)
        {
            Text = text;
            TextSize = size;
            _updateRenderCache = true;

            // Start processing.
            _textToDisplay = "";

            // Find effects.
            ParseEffects();

            // Apply wrapping.
            Wrap();

            // Apply text alignment.
            ProcessAlignment();
        }

        /// <summary>
        /// Sets the RichText's text and size.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public void SetText(string text)
        {
            SetText(text, TextSize);
        }

        #endregion

        #region Text Processing

        /// <summary>
        /// Parse the raw string for tag effects, removes the tag string from the text and populates the effects cache.
        /// Uses:
        /// Text
        /// Populates:
        /// _textToDisplay - The text striped of tags.
        /// _effectsCache - A list of effects captured and the indices at which they begin and end within the _textToDisplay
        /// variable, regarded as "real" indices.
        /// </summary>
        private void ParseEffects()
        {
            _effectCache.Clear();

            bool tagOpened = false;
            string currentTag = "";

            // Iterate characters, and extract tags.
            foreach (char c in Text)
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
                        ProcessTag(currentTag, _textToDisplay.Length);
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
                            _textToDisplay += c;
                        break;
                }
            }
        }

        /// <summary>
        /// Processes the provided tag and add it to the effects cache. Used by ParseEffects. Can be overloaded.
        /// </summary>
        /// <param name="tag">The tag data captured.</param>
        /// <param name="position">The position of the tag within the text to display.</param>
        protected virtual void ProcessTag(string tag, int position)
        {
            // Check if closing tag.
            if (tag == "/")
            {
                // Close the last opened tag.
                TextEffect toClose = _effectCache.Last(t => t.End == -1);
                if (toClose != null) toClose.End = position;
            }
            else
            {
                string[] tagDataSplit = tag.Split('=');
                string[] attributes = tagDataSplit.Length > 1 ? tagDataSplit[1].Split(TagAttributeSeparator) : null;

                _effectCache.Add(new TextEffect
                {
                    Start = position,
                    Name = tagDataSplit[0].ToLower(),
                    Attributes = attributes
                });
            }
        }

        /// <summary>
        /// Wrap the text to fit the bounds of the RichText object.
        /// Uses:
        /// _textToDisplay - The text to wrap.
        /// Bounds - the RichText's bounds.
        /// Populates:
        /// _wrapCache - The text to display as a list of lines wrapped to fit within the object bounds.
        /// </summary>
        private void Wrap()
        {
            _wrapCache.Clear();

            string currentLine = "";
            bool breakSkipMode = false;
            int breakSkipModeLimit = -1;
            Atlas atlas = _font.GetFontAtlas(TextSize);

            // Loop through the text.
            for (int i = 0; i < _textToDisplay.Length; i++)
            {
                // Check if exiting break skip mode.
                if (breakSkipModeLimit == i) breakSkipMode = false;

                // Find the location of the next space or new line character.
                int nextSpaceLocation = _textToDisplay.IndexOf(' ', i);
                int nextNewLineLocation = _textToDisplay.IndexOf('\n', i);
                int nextBreakLocation;

                if (nextNewLineLocation == -1 && nextSpaceLocation == -1)
                    nextBreakLocation = _textToDisplay.Length;
                else if (nextSpaceLocation == -1)
                    nextBreakLocation = nextNewLineLocation;
                else if (nextNewLineLocation == -1)
                    nextBreakLocation = nextSpaceLocation;
                else
                    nextBreakLocation = Math.Min(nextNewLineLocation, nextSpaceLocation);

                // Get the text to the next break.
                string textToBreak = _textToDisplay.Substring(i, nextBreakLocation - i);

                // Measure the current line with the new characters.
                Vector2 textSize = atlas.MeasureString(currentLine + textToBreak);

                // Check if the textToBreak is too big to fit on one line.
                Vector2 overflowCheck = atlas.MeasureString(textToBreak);

                // Check if the whole textToBreak cannot fit on a single line.
                // This is a rare case, but when it happens characters must be printed without performing break checks as they will either cause
                // each character to go on a separate line or cause a line break in the text as soon as it can fit on the line.
                // To do this we switch to a break skipping mode which ensures this other method of printing until the whole text is printed.
                if (overflowCheck.X > Bounds.Width || breakSkipMode)
                {
                    textSize = atlas.MeasureString(currentLine + _textToDisplay[i]);
                    breakSkipMode = true;
                    breakSkipModeLimit = i + textToBreak.Length;
                }

                // Break line if we don't have enough space to fit all the text to the next break, or if the current character is a break.
                if (textSize.X > Bounds.Width || _textToDisplay[i] == '\n')
                {
                    // Push new line.
                    _wrapCache.Add(currentLine);
                    currentLine = "";

                    // If the current character is a new line break retroactively push it on the last line, and continue without adding it to the current line.
                    if (_textToDisplay[i] == '\n')
                    {
                        _wrapCache[_wrapCache.Count - 1] += '\n';
                        continue;
                    }
                }

                // Add the current character to the current line string.
                currentLine += _textToDisplay[i].ToString();
            }

            // If there is text left push in on a new line.
            if (!string.IsNullOrEmpty(currentLine)) _wrapCache.Add(currentLine);
        }

        /// <summary>
        /// Process text alignment.
        /// </summary>
        private void ProcessAlignment()
        {
            _spaceSize.Clear();
            _initialLineIndent.Clear();

            // Go through all lines.
            for (int i = 0; i < _wrapCache.Count; i++)
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
            int charSpacing = 0;
            Atlas atlas = _font.GetFontAtlas(TextSize);

            // Calculate and check.
            float lineSize = atlas.MeasureString(_wrapCache[i]).X;
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
                _spaceSize.Add(0);
                return;
            }

            float characterSpace = 1;
            while (characterSpace >= 0)
            {
                // Get the space left on the line by subtracting the total width from the line's width plus character spacing, minus one because of the last character.
                characterSpace = Bounds.Width - (lineSize + charSpacing * spaces);

                // If there is space, increase char spacing.
                charSpacing++;
            }

            // Decrease by one.
            if (charSpacing > 0) charSpacing--;

            _spaceSize.Add(charSpacing);
        }

        private void AlignCenter(int i)
        {
            Atlas atlas = _font.GetFontAtlas(TextSize);
            float spaceLeft = Bounds.Width - atlas.MeasureString(_wrapCache[i]).X;

            // Get justification character spacing. if any.
            if (i < _spaceSize.Count) spaceLeft -= _spaceSize[i] * CountSpaces(_wrapCache[i]);

            // Check if justifying by center, and not on the first line.
            if (Alignment == TextAlignment.JustifiedCenter && i != 0 && spaceLeft / 2 >= _initialLineIndent[0])
            {
                // Set the indent to be that of the first line.
                _initialLineIndent.Add(_initialLineIndent[0]);
            }
            else
            {
                if (spaceLeft > 0)
                    _initialLineIndent.Add((int) (spaceLeft / 2));
                else
                    _initialLineIndent.Add(0);
            }
        }

        private void AlignRight(int i)
        {
            Atlas atlas = _font.GetFontAtlas(TextSize);
            float spaceLeft = Bounds.Width - atlas.MeasureString(_wrapCache[i]).X;

            // To align right set the free space before the line.
            _initialLineIndent.Add((int) spaceLeft);
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw the RichText object. Can be overloaded.
        /// </summary>
        public override void Draw(Renderer _)
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
                _modelMatrix = Matrix4.CreateTranslation(Bounds.X, Bounds.Y, Z);
                _transformUpdated = false;
            }

            // Draw the buffer. The model matrix is set here so we don't have to remap the buffer when the position is changed.
            _renderCache.Draw(_modelMatrix);
        }

        /// <summary>
        /// Applies effects to the glyph and adds it to the map buffer. Can be overloaded.
        /// </summary>
        /// <param name="line">The virtual line in the wrapped text which the glyph is on.</param>
        /// <param name="charInLine">The id of the character within the specified line.</param>
        /// <param name="charGlobal">The index of the character globally.</param>
        /// <param name="glyphXOffset">The X offset of the glyph.</param>
        protected virtual void ProcessGlyph(int line, int charInLine, int charGlobal, int glyphXOffset)
        {
            // Get all active effects for the current index.
            IEnumerable<TextEffect> effects = GetEffectsAt(charGlobal);

            // Tag changeable properties.
            Color textColor = Color.White;

            // Apply effects.
            foreach (TextEffect e in effects)
            {
                if (e.Name == "color" && e.Attributes?.Length >= 3)
                    textColor = new Color(Convert.StringToInt(e.Attributes[0]), Convert.StringToInt(e.Attributes[1]), Convert.StringToInt(e.Attributes[2]));
            }

            // Render the glyph.
            AddGlyph(_wrapCache[line][charInLine], textColor, glyphXOffset);
        }

        #endregion

        #region Mapping Helpers

        private void MapBuffer()
        {
            // Start mapping.
            _renderCache.Start();
            _prevChar = '\0';

            // Iterate virtual lines.
            int characterCounter = 0;
            for (int line = 0; line < _wrapCache.Count; line++)
            {
                // Iterate virtual characters.
                for (int c = 0; c < _wrapCache[line].Length; c++)
                {
                    // Increment character counter.
                    characterCounter++;

                    int glyphXOffset = 0;

                    // Apply space size multiplication if the current character is a space.
                    if (line < _spaceSize.Count && _wrapCache[line][c] == ' ') glyphXOffset += _spaceSize[line];

                    // Check if applying initial indent.
                    if (line < _initialLineIndent.Count && c == 0) glyphXOffset += _initialLineIndent[line];

                    // Check if rendering a character we don't want visible, in which case we just increment the space.
                    if (CharactersToNotRender.Contains(_wrapCache[line][c]))
                        Push(glyphXOffset);
                    else
                        ProcessGlyph(line, c, characterCounter, glyphXOffset);
                }

                // Check if reached past the scroll line rendered threshold, and this isn't the last line.
                if (line == _wrapCache.Count - 1) continue;
                NewLine();
            }

            // Finish mapping.
            _renderCache.FinishMapping();
        }

        /// <summary>
        /// Push the pen position by the specified amounts.
        /// </summary>
        /// <param name="offsetX">The horizontal pen position.</param>
        /// <param name="offsetY">The vertical pen position.</param>
        private void Push(float offsetX = 0, float offsetY = 0)
        {
            _penX += offsetX;
            _penY += offsetY;
        }

        /// <summary>
        /// Move the pen to a new line. Resets the X pen.
        /// </summary>
        private void NewLine()
        {
            _penX = 0;
            _penY += _font.GetFontAtlas(TextSize).LineSpacing;
        }

        /// <summary>
        /// Add a glyph to the map buffer.
        /// </summary>
        /// <param name="c">The character to add.</param>
        /// <param name="color">The color of the character.</param>
        /// <param name="xOffset">The x offset.</param>
        private void AddGlyph(char c, Color color, float xOffset)
        {
            // Get atlas, glyph, and kerning.
            Atlas atlas = _font.GetFontAtlas(TextSize);
            Glyph glyph = atlas.Glyphs[c];
            float kerning = atlas.GetKerning(_prevChar, c);
            _prevChar = c;

            // Add kerning and offset.
            _penX += kerning;
            _penX += xOffset;

            // Calculate properties.
            Vector3 renderPos = new Vector3(_penX + glyph.MinX, _penY + glyph.YBearing, 0);
            Rectangle uv = new Rectangle(glyph.X, glyph.Y, glyph.Width, glyph.Height);

            _renderCache.Add(renderPos, uv.Size, color, atlas.Texture, uv);

            _penX += glyph.Advance;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the virtual lines which the real line corresponds to.
        /// </summary>
        /// <param name="realLineIndex">The real line index.</param>
        /// <returns>The virtual lines which the real line corresponds to.</returns>
        protected List<int> GetVirtualLinesIndexesFromReal(int realLineIndex)
        {
            // Get the number of real lines.
            int realLines = _textToDisplay.Split('\n').Length;

            // Find the index of all newlines.
            int currentLineEndIndex = 0;

            for (int i = 0; i < realLines; i++)
            {
                int currentLineStartIndex = currentLineEndIndex;
                currentLineEndIndex = _textToDisplay.IndexOf('\n', currentLineEndIndex + 1);
                if (currentLineEndIndex == -1) currentLineEndIndex = _textToDisplay.Length;

                // Check if the current real line is the one we are processing.
                if (i != realLineIndex) continue;

                // Find which virtual lines contain the real line start and end.
                int virtualStartLineIndex = GetVirtualLineIndexFromRealCharIndex(currentLineStartIndex);
                int virtualEndLineIndex = GetVirtualLineIndexFromRealCharIndex(currentLineEndIndex - 1);

                // Return all lines between them.
                List<int> result = new List<int>();
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
            int characterCounter = 0;

            for (int line = 0; line < _wrapCache.Count; line++)
            {
                for (int c = 0; c < _wrapCache[line].Length; c++)
                {
                    if (characterCounter == realCharIndex) return line;

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
            // todo: Check if index < t.End or index <= t.End
            return _effectCache.Where(t => index >= t.Start && (index < t.End || index == t.End && index == t.Start));
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
            bool lastCharacterIsSpace = text.Length > 0 && text[text.Length - 1] == ' ';
            if (lastCharacterIsSpace) spaces--;

            return spaces;
        }

        #endregion
    }
}