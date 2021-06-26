#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics;
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
        public string FontFile;

        /// <summary>
        /// The unscaled font size of the text.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Whether to smoothen the drawing of the font by using bilinear filtering.
        /// </summary>
        public bool Smooth;

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
        public float UnderlineThickness = 1f;

        protected string _text;
        protected FontAsset _fontFile;
        protected DrawableFontAtlas _atlas;
        protected TextLayouterWrap _layouter;
        protected Vector2 _scaledUnderlineOffset;
        protected float _scaledUnderlineThickness;

        public override async Task Preload()
        {
            await base.Preload();
            _fontFile = await Engine.AssetLoader.GetAsync<FontAsset>(FontFile);

            if (_fontFile == null) return;

            // Preload atlas as well.
            // Todo: Split scaled atlas from drawing so that metrics don't need the full thing.
            float scale = GetScale();
            _atlas = _fontFile.GetAtlas((int) MathF.Ceiling(FontSize * scale), 0, -1, Smooth, FontSizePixelPerfect);
            _layouter = new TextLayouterWrap(_atlas.Atlas);
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            if (_fontFile == null) return Vector2.Zero;

            float scale = GetScale();
            _scaledUnderlineOffset = UnderlineOffset * scale;
            _scaledUnderlineThickness = UnderlineThickness * scale;

            _layouter.Restart();
            _layouter.SetupBox(_text ?? "", space, TextHeightMode);
            return new Vector2(_layouter.NeededWidth, _layouter.NeededHeight);
        }

        protected override bool RenderInternal(RenderComposer c, ref Color windowColor)
        {
            if (_text == null || _layouter == null) return true;

            if (TextShadow != null)
            {
                _layouter.RestartPen();
                c.RenderString(Position + ShadowOffset.ToVec3(), TextShadow.Value * windowColor.A, _text, _atlas, _layouter);
            }

            if (Underline)
            {
                float y = Y + Height + _scaledUnderlineOffset.Y;
                c.RenderLine(new Vector2(X + _scaledUnderlineOffset.X, y), new Vector2(X + Width - _scaledUnderlineOffset.X * 2, y), windowColor, _scaledUnderlineThickness);
            }

            _layouter.RestartPen();
            c.RenderString(Position, windowColor, _text, _atlas, _layouter);
            return true;
        }
    }
}