#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UIText : UIBaseWindow
    {
        /// <summary>
        /// The name of the font asset.
        /// </summary>
        public string FontFile
        {
            get => _fontFileName;
            set
            {
                _fontFileName = value;
                InvalidateLoaded();
            }
        }

        private string _fontFileName;

        /// <summary>
        /// The unscaled font size of the text.
        /// </summary>
        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                InvalidateLoaded();
            }
        }

        private int _fontSize;

        /// <summary>
        /// Whether to smoothen the drawing of the font by using bilinear filtering.
        /// </summary>
        public bool Smooth = true;

        /// <summary>
        /// Whether the font is a pixel font and we want it to scale integerly.
        /// </summary>
        public bool FontSizePixelPerfect;

        /// <summary>
        /// The text to display.
        /// </summary>
        public virtual string Text
        {
            get => _text;
            set
            {
                _text = value;
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Text shadow to draw, if any.
        /// </summary>
        public Color? TextShadow { get; set; }

        /// <summary>
        /// Used to modify the way the height of glyphs are measured and rendered.
        /// Used in producing better looking text when it is expected to span a single line, or positioned above/below/centered on
        /// something.
        /// </summary>
        public GlyphHeightMeasurement TextHeightMode = GlyphHeightMeasurement.FullHeight;

        /// <summary>
        /// The offset of the shadow from the text.
        /// </summary>
        public Vector2 ShadowOffset = new Vector2(2, 2);

        /// <summary>
        /// Whether to underline.
        /// </summary>
        public bool Underline;

        /// <summary>
        /// The offset of the underline.
        /// </summary>
        public Vector2 UnderlineOffset = Vector2.Zero;

        /// <summary>
        /// The thickness of the underline.
        /// </summary>
        public float UnderlineThickness = 0.5f;

        protected string _text;
        protected FontAsset _fontFile;
        protected DrawableFontAtlas _atlas;
        protected TextLayouterWrap _layouter;
        protected DrawableFontAtlas _layouterAtlas;
        protected Vector2 _scaledUnderlineOffset;
        protected float _scaledUnderlineThickness;

        protected override async Task LoadContent()
        {
            // Load font if not loaded.
            if (_fontFile == null || _fontFile.Name != FontFile || _fontFile.Disposed) _fontFile = await Engine.AssetLoader.GetAsync<FontAsset>(FontFile);
            if (_fontFile == null) return;

            if (FontSize == 0)
            {
                _atlas = null;
                return;
            }

            // Load atlas as well. This one will change based on UI scale.
            // Todo: Split scaled atlas from drawing so that metrics don't need the full thing.
            float scale = GetScale();
            _atlas = _fontFile.GetAtlas((int)MathF.Ceiling(FontSize * scale), Smooth, FontSizePixelPerfect);

            // Reload the layouter if needed. Changing this means the text needs to be relayouted.
            if (_layouterAtlas != _atlas)
            {
                _layouter = new TextLayouterWrap(_atlas);
                _layouterAtlas = _atlas;
                InvalidateLayout();
            }
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            if (_fontFile == null || _layouter == null) return Vector2.Zero;

            float scale = GetScale();
            _scaledUnderlineOffset = UnderlineOffset * scale;
            _scaledUnderlineThickness = UnderlineThickness * scale;

            _layouter.Restart();
            _layouter.SetupBox(_text ?? "", space, TextHeightMode);
            return new Vector2(_layouter.NeededWidth, _layouter.NeededHeight);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            if (_text == null || _fontFile == null || _layouter == null) return true;

            if (TextShadow != null)
            {
                _layouter.RestartPen();
                c.RenderString(Position + ShadowOffset.ToVec3(), TextShadow.Value * _calculatedColor.A, _text, _atlas, _layouter);
            }

            if (Underline)
            {
                float y = Y + Height + _scaledUnderlineOffset.Y;
                c.RenderLine(new Vector3(X, y, Z), new Vector3(X + Width, y, Z), _calculatedColor, _scaledUnderlineThickness);
            }

            _layouter.RestartPen();
            c.RenderString(Position, _calculatedColor, _text, _atlas, _layouter);
            return true;
        }
    }
}