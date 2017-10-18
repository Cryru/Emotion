// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Raya.Graphics;
using Raya.Primitives;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using System.Linq;

#endregion

namespace Soul.Engine.Objects
{
    public class Text : GameObject
    {
        #region Properties

        /// <summary>
        /// The text's primary color.
        /// </summary>
        public Color Color
        {
            get { return _nativeObject.FillColor; }
            set { _nativeObject.FillColor = value; }
        }

        /// <summary>
        /// The text's primary outline color.
        /// </summary>
        public Color OutlineColor
        {
            get { return _nativeObject.OutlineColor; }
            set { _nativeObject.OutlineColor = value; }
        }

        /// <summary>
        /// The thickness of the font's outline.
        /// </summary>
        public float OutlineThickness
        {
            get { return _nativeObject.OutlineThickness; }
            set { _nativeObject.OutlineThickness = value; }
        }

        public Raya.Graphics.Text.Styles Style
        {
            get { return _nativeObject.Style; }
            set { _nativeObject.Style = value; }
        }

        /// <summary>
        /// The raw text to draw.
        /// </summary>
        public string RawText
        {
            get { return _rawText; }
            set
            {
                // Redundancy check.
                if (_rawText == value) return;

                _rawText = value;

                FitBox();
            }
        }

        private string _rawText;
        #endregion

        #region Raya API

        /// <summary>
        /// The sprite object inside the Raya API.
        /// </summary>
        private Raya.Graphics.Text _nativeObject;

        /// <summary>
        /// The internal font object of the loaded font.
        /// </summary>
        private Font _font;
        #endregion

        #region Calculated

        private bool _loggedProcessing = false;
        private string _processedText;

#endregion

        /// <summary>
        /// Create a new text component.
        /// </summary>
        /// <param name="fontName">The loaded font's name. If it isn't loaded it will be.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="size">The text size.</param>
        public Text(string fontName, string text, int size = 20)
        {
            // Assign text.
            _rawText = text;

            // Load the size.
            Size = new Vector2(size, size);
            Position = new Vector2(0, 0);

            // Load the font.
            _font = AssetLoader.GetFont(fontName);
        }

        /// <summary>
        /// Initializes the texture.
        /// </summary>
        public override void Initialize()
        {
            // Load the native object.
            _nativeObject = new Raya.Graphics.Text(RawText, _font, (uint) Math.Max(Size.X, Size.Y));

            // Attach to parent events.
            ((GameObject) Parent).onSizeChanged += () =>
            {
                Console.Clear(); _loggedProcessing = false; };
            ((GameObject)Parent).onPositionChanged += UpdatePosition;
            ((GameObject)Parent).onRotationChanged += UpdateRotation;

            // Attach to own events.
            onSizeChanged += UpdateSize;
            onPositionChanged += UpdatePosition;
            onRotationChanged += UpdateRotation;

            UpdateSize();
            UpdatePosition();
            UpdateRotation();
        }

        #region Functions

        private void FitBox()
        {
            _processedText = "";

            int lineLimit = ((GameObject)Parent).Size.X;
            string line = "";
            int heightLimit = ((GameObject)Parent).Size.Y;

            for (int i = 0; i < _rawText.Length; i++)
            {
                // Calculate line width.
                int lineWidth = MeasureStringWidth(line + "_.");

                // If the character is a space...
                if (_rawText[i] == ' ')
                {
                    // If the only thing on the line, don't render it.
                    if (line == "") continue;

                    // Find location of the next space or next line character.

                }


                // Check if too much characters on this line, or the character is a new line.
                if (lineWidth > lineLimit || _rawText[i] == '\n')
                {
                    _processedText += "\n";
                    line = "";

                    // Skip adding if a new line symbol.
                    if (_rawText[i] == '\n')
                    {
                        continue;
                    }

                    // Test if we can fit another line.
                    int linesHeight = MeasureHeight(_processedText + "\n");

                    // Check if there are too many lines.
                    if (linesHeight >= heightLimit)
                    {
                        _loggedProcessing = true;
                        return;
                    }
                }

                // Add the current character to the processed text.
                _processedText += _rawText[i];
                // Add the character to the current line.
                line += _rawText[i];

            }

            _loggedProcessing = true;
        }

        private bool ShouldUpdate()
        {
            return !_loggedProcessing;
        }

        #endregion

        #region Syncronization Functions

        private void UpdateSize()
        {
            uint characterSize = (uint) Math.Max(Size.X, Size.Y);

            _nativeObject.CharacterSize = characterSize;

            // Force SFML to cache font glyphs for this size.
            for (int i = 1; i < 100; i++)
            {
                _nativeObject.Font.GetGlyph((uint)i, characterSize, false, 0);
            }
        }

        private void UpdatePosition()
        {
            Vector2f combined = (Vector2f)(((GameObject)Parent).Position + Position);

            // Set the position combined from this object and its parent.
            _nativeObject.Position = combined;
        }

        private void UpdateRotation()
        {
            float combined = ((GameObject)Parent).RotationDegree + RotationDegree;
            _nativeObject.Rotation = combined;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private int MeasureStringWidth(string text)
        {
            _nativeObject.DisplayedString = text;
            Vector2f temp = _nativeObject.FindCharacterPos((uint) text.Length);

            return (int) (temp.X);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private int MeasureHeight(string text)
        {
            _nativeObject.DisplayedString = text;
            Vector2f temp = _nativeObject.FindCharacterPos((uint)text.Length);

            return (int)(temp.Y);
        }

        #endregion

        public void Destroy()
        {
            // Unhook from parent.
            ((GameObject)Parent).onSizeChanged -= UpdateSize;
            ((GameObject)Parent).onPositionChanged -= UpdatePosition;
            ((GameObject)Parent).onRotationChanged -= UpdateRotation;
            Parent.RemoveChild(this);

            // Remove reference of the font.
            _font = null;

            // Dispose the native object.
            _nativeObject.Dispose();
        }

        public override void Update()
        {
            // Update the text processing if needed.
            if (ShouldUpdate())
            {
                FitBox();

                // Create a new native object, this is to work around a bug.
                _nativeObject = new Raya.Graphics.Text(_processedText, _font, (uint) Math.Max(Size.X, Size.Y));

                UpdatePosition();
                UpdateRotation();
            }

            Core.Draw(_nativeObject);
        }
    }
}