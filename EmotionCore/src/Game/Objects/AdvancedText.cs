// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Game.Components;
using Emotion.Game.Pieces;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Assets;
using Emotion.Platform.Base.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Objects
{
    /// <inheritdoc />
    public class AdvancedText : Transform
    {
        #region Properties

        /// <summary>
        /// The text to render. Resets the current state. Uses DisplayText(value, true).
        /// </summary>
        public string Text
        {
            get => _rawText;
            set => DisplayText(value);
        }

        /// <summary>
        /// The duration to scroll the text for. Is reset when text is.
        /// </summary>
        public int ScrollDuration { get; set; }

        /// <summary>
        /// Whether the scrolling effect is complete and the whole string is rendered.
        /// </summary>
        public bool DoneScrolling
        {
            get => string.IsNullOrEmpty(_textToDisplay) || _scrollRenderedTo == _textToDisplay.Length;
        }

        /// <summary>
        /// The size of the text to render.
        /// </summary>
        public int TextSize { get; set; }

        #endregion

        #region Configuration

        public static char[] CharactersToNotRender = {'\n'};
        public static char TagAttributeSeparator = '-';

        #endregion

        #region State

        protected Font _font { get; set; }
        protected TextDrawingSession _drawingSession { get; set; }
        protected Texture _cachedTextRender { get; set; }

        // Text parsing.
        protected string _rawText { get; set; }
        protected string _textToDisplay { get; set; }
        protected List<TextEffect> _effectCache { get; set; } = new List<TextEffect>();
        protected List<string> _wrapCache { get; set; } = new List<string>();

        // Scrolling.
        protected int _scrollPosition { get; set; }
        protected float _scrollTimer { get; set; }
        protected int _scrollRenderedTo { get; set; }
        protected int _scrollLineRenderedTo { get; set; }

        #endregion

        /// <summary>
        /// Create a new advanced text object which manages text wrapping, scrolling, and more.
        /// </summary>
        /// <param name="bounds">The bounds of the text object.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="textSize">The font size to use. Can be changed later.</param>
        public AdvancedText(Rectangle bounds, Font font, int textSize) : base(bounds)
        {
            _font = font ?? throw new Exception("Invalid font provided to an advanced text object.");
            TextSize = textSize;
        }

        #region API

        /// <summary>
        /// Set a string to be displayed.
        /// </summary>
        /// <param name="text">The string to draw.</param>
        /// <param name="fullReset">
        /// Whether to reset the full state. Would be set to false if you want to append text.
        /// </param>
        public void DisplayText(string text, bool fullReset = true)
        {
            // Check if any text is set.
            if (string.IsNullOrEmpty(text)) return;

            // Check whether to reset scroll state.
            if (fullReset) Reset();

            // Set text.
            _rawText = text;

            // Find effects.
            ParseEffects();

            // Apply wrapping.
            Wrap();
        }

        /// <summary>
        /// Stop scrolling.
        /// </summary>
        public void EndScroll()
        {
            _scrollPosition = _textToDisplay.Length;
            ScrollDuration = 0;
        }

        public void ForceRender()
        {
            // Check if size changed.
            if (_drawingSession?.Size != Bounds.Size)
            {
                _drawingSession?.Destroy();
                _drawingSession = null;
            }
            else
            {
                _drawingSession?.Reset();
            }

            _scrollLineRenderedTo = 0;
            _scrollRenderedTo = 0;
        }

        #endregion

        #region Drawing and Updating

        public void Update(float frameTime)
        {
            // Check if any text is set.
            if (string.IsNullOrEmpty(_textToDisplay)) return;

            // Scroll.
            UpdateScrolling(frameTime);
        }

        public void Draw(Renderer renderer)
        {
#if DEBUG
            // Debug draw text bounds.
            renderer.DrawRectangleOutline(Bounds, Color.Red);
#endif
            // Draw to the text rendering cache.
            CacheDraw(renderer);

            // Check if anything is cached.
            if (_cachedTextRender == null) return;

            // Draw the cached render.
            renderer.DrawTexture(_cachedTextRender, Bounds);
        }

        protected void UpdateScrolling(float frameTime)
        {
            // Check if scrolling.
            if (DoneScrolling) return;
            if (ScrollDuration <= 0)
            {
                EndScroll();
                return;
            }

            // Check for an overflow. This can be caused when setting text to a shorter string without resetting scrolling.
            if (_scrollPosition > _textToDisplay.Length) _scrollPosition = _textToDisplay.Length;

            // Add to the scroll timer.
            _scrollTimer += frameTime;

            // Calculate scroll duration per character.
            float durationPerCharacter = ScrollDuration / _textToDisplay.Length;

            // Check if enough time has passed.
            if (!(_scrollTimer > durationPerCharacter)) return;

            _scrollTimer -= durationPerCharacter;
            _scrollPosition++;
        }

        protected void CacheDraw(Renderer renderer)
        {
            // Check if a session hasn't been created.
            if (_drawingSession == null) _drawingSession = renderer.StartTextSession(_font, TextSize, (int) Bounds.Width, (int) Bounds.Height);

            // Check for a scroll overflow. This can happen when scrolling isn't reset but the text is.
            if (_scrollRenderedTo > _scrollPosition)
            {
                _scrollRenderedTo = _scrollPosition;
                _scrollLineRenderedTo = GetVirtualLineIndexFromRealCharIndex(_scrollPosition);
            }

            // Check if done scrolling, and all is rendered.
            if (DoneScrolling) return;

            // Iterate virtual lines.
            int characterCounter = 0;
            for (int line = 0; line < _wrapCache.Count; line++)
            {
                // Iterate virtual characters.
                for (int c = 0; c < _wrapCache[line].Length; c++)
                {
                    // Increment character counter.
                    characterCounter++;
                    // Check if reached past the scroll threshold.
                    if (characterCounter > _scrollPosition) break;
                    // Check if reached past the rendered threshold.
                    if (characterCounter <= _scrollRenderedTo) continue;

                    // Check if rendering a character we don't want visible.
                    if (!CharactersToNotRender.Contains(_wrapCache[line][c])) AddGlyph(line, c);

                    // Move the rendered threshold.
                    _scrollRenderedTo++;
                }

                // Check if reached past the scroll threshold. Second check for second break.
                if (characterCounter > _scrollPosition) break;

                // Check if reached past the scroll line rendered threshold, and this isn't the last line.
                if (line >= _scrollLineRenderedTo && line != _wrapCache.Count - 1)
                {
                    _drawingSession.NewLine();
                    _scrollLineRenderedTo++;
                }
            }

            // Get the render.
            _cachedTextRender = _drawingSession.GetTexture();
        }

        /// <summary>
        /// Adds the glyph to the drawing session and applies any effects.
        /// </summary>
        /// <param name="line">The line in the wrapped text which the glyph is on.</param>
        /// <param name="c">The id of the character within the specified line.</param>
        protected virtual void AddGlyph(int line, int c)
        {
            // Get all active effects for the current index.
            IEnumerable<TextEffect> effects = GetEffectsAt(_scrollRenderedTo);

            // Tag changeable properties.
            Color textColor = Color.White;

            // Apply effects.
            foreach (TextEffect e in effects)
            {
                if (e.Name == "color" && e.Attributes?.Length >= 3)
                    textColor = new Color(Helpers.SafeIntParse(e.Attributes[0]), Helpers.SafeIntParse(e.Attributes[1]), Helpers.SafeIntParse(e.Attributes[2]));
            }

            // Render the glyph.
            _drawingSession.AddGlyph(_wrapCache[line][c], textColor);
        }

        #endregion

        #region Text Processing

        /// <summary>
        /// Parse the raw string for markdown effects, clearing the string off of them and clearing an effects cache.
        /// Uses:
        /// _rawText
        /// Populates:
        /// _textToDisplay - The text striped of tags.
        /// _effectsCache - A list of effects captured and the indexes at which they begin and end within the _textToDisplay
        /// variable, regarded as "real" indexes.
        /// </summary>
        protected void ParseEffects()
        {
            _effectCache.Clear();
            _textToDisplay = "";

            bool tagOpened = false;
            string currentTag = "";

            // Iterate characters, and extract tags.
            foreach (char c in _rawText)
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
        /// Processes the provided tag and add it to the effects cache.
        /// </summary>
        /// <param name="tag">The tag data captured.</param>
        /// <param name="position">The position of the tag within the text to display.</param>
        protected void ProcessTag(string tag, int position)
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
        /// Wrap the text to fit the bounds of the object.
        /// Uses:
        /// _textToDisplay - The text to render.
        /// Populates:
        /// _wrapCache - The text to display as a list of lines wrapped to fit within the object bounds.
        /// </summary>
        protected void Wrap()
        {
            _wrapCache.Clear();

            string currentLine = "";
            bool breakSkipMode = false;
            int breakSkipModeLimit = -1;

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
                Vector2 textSize = _font.MeasureString(currentLine + textToBreak, TextSize);

                // Check if the textToBreak is too big to fit on one line.
                Vector2 overflowCheck = _font.MeasureString(textToBreak, TextSize);

                // Check if the whole textToBreak cannot fit on a single line.
                // This is a rare case, but when it happens characters must be printed without performing break checks as they will either case
                // each character to go on a separate line or cause a line break in the text as soon as it can fit on the line.
                // To do this we switch to a break skipping mode which ensures this other method of printing until the whole text is printed.
                if (overflowCheck.X > Bounds.Width || breakSkipMode)
                {
                    textSize = _font.MeasureString(currentLine + _textToDisplay[i], TextSize);
                    breakSkipMode = true;
                    breakSkipModeLimit = i + textToBreak.Length;
                }

                // Break line if we don't have enough space to fit all the text to the next break, or if the current character is a break.
                if (textSize.X > Bounds.Width || _textToDisplay[i] == '\n')
                {
                    // Push new line.
                    _wrapCache.Add(currentLine);
                    currentLine = "";
                }

                // Add the current character to the current line string.
                currentLine += _textToDisplay[i].ToString();
            }

            _wrapCache.Add(currentLine);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Reset all states.
        /// </summary>
        protected void Reset()
        {
            _cachedTextRender?.Destroy();
            _cachedTextRender = null;

            ForceRender();

            ScrollDuration = 0;
            _scrollPosition = 0;
            _scrollTimer = 0;
        }

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
            return _effectCache.Where(t => index >= t.Start && index < t.End);
        }

        #endregion
    }
}