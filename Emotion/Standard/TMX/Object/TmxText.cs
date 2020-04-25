#region Using

using Emotion.Primitives;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX.Object
{
    public class TmxText
    {
        public string FontFamily { get; private set; }
        public int PixelSize { get; private set; }
        public bool Wrap { get; private set; }
        public Color Color { get; private set; }
        public bool Bold { get; private set; }
        public bool Italic { get; private set; }
        public bool Underline { get; private set; }
        public bool Strikeout { get; private set; }
        public bool Kerning { get; private set; }
        public TmxAlignment Alignment { get; private set; }
        public string Value { get; private set; }

        public TmxText(XMLReader xText)
        {
            FontFamily = xText.Attribute("fontfamily") ?? "sans-serif";
            PixelSize = xText.AttributeIntN("pixelsize") ?? 16;
            Wrap = xText.AttributeBool("wrap");
            Color = TmxHelpers.ParseTmxColor(xText.Attribute("color"));
            Bold = xText.AttributeBool("bold");
            Italic = xText.AttributeBool("italic");
            Underline = xText.AttributeBool("underline");
            Strikeout = xText.AttributeBool("strikeout");
            Kerning = xText.AttributeBoolN("kerning") ?? true;
            Alignment = new TmxAlignment(xText.Attribute("halign"), xText.Attribute("valign"));
            Value = xText.CurrentContents();
        }
    }
}