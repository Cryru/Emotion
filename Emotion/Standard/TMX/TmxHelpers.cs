#region Using

using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Standard.TMX
{
    public static class TmxHelpers
    {
        public static Color ParseTmxColor(string color)
        {
            if (string.IsNullOrEmpty(color)) return new Color();

            string colorStr = color.TrimStart("#".ToCharArray());

            byte r = byte.Parse(colorStr.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(colorStr.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(colorStr.Substring(4, 2), NumberStyles.HexNumber);
            return new Color(r, g, b);
        }

        public static TmxProperties GetPropertyDict(XMLReader containingElement)
        {
            if (containingElement == null) return new TmxProperties(null);
            var attributes = new Dictionary<string, string>();
            List<XMLReader> properties = containingElement.Elements("property");
            for (var i = 0; i < properties.Count; i++)
            {
                XMLReader p = properties[i];
                string name = p.Attribute("name");
                if (name == null) continue;
                string value = p.Attribute("value") ?? p.CurrentContents();
                attributes.Add(name, value);
            }

            return new TmxProperties(attributes);
        }

        public static Vector2 GetVector2(XMLReader element)
        {
            if (element == null) return Vector2.Zero;
            var v = new Vector2
            {
                X = element.AttributeInt("x"),
                Y = element.AttributeInt("y")
            };
            return v;
        }

        public static Vector2 GetObjectPoint(string s)
        {
            if (string.IsNullOrEmpty(s)) return Vector2.Zero;
            string[] pt = s.Split(',');
            var v = new Vector2
            {
                X = float.Parse(pt[0], NumberStyles.Float,
                    CultureInfo.InvariantCulture),
                Y = float.Parse(pt[1], NumberStyles.Float,
                    CultureInfo.InvariantCulture)
            };
            return v;
        }

        private const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        private const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        private const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public static int GetGidFlags(uint rawId, out bool horizontalFlip, out bool verticalFlip, out bool diagonalFlip)
        {
            // Scan for tile flip bit flags
            bool flip = (rawId & FLIPPED_HORIZONTALLY_FLAG) != 0;
            horizontalFlip = flip;

            flip = (rawId & FLIPPED_VERTICALLY_FLAG) != 0;
            verticalFlip = flip;

            flip = (rawId & FLIPPED_DIAGONALLY_FLAG) != 0;
            diagonalFlip = flip;

            // Zero the bit flags
            rawId &= ~(FLIPPED_HORIZONTALLY_FLAG |
                       FLIPPED_VERTICALLY_FLAG |
                       FLIPPED_DIAGONALLY_FLAG);

            // Save GID remainder to int
            return (int) rawId;
        }
    }
}