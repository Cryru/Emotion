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

        public static Dictionary<string, string> GetPropertyDict(XMLReader containingElement)
        {
            var attributes = new Dictionary<string, string>();
            if (containingElement == null) return attributes;
            List<XMLReader> properties = containingElement.Elements("property");
            for (var i = 0; i < properties.Count; i++)
            {
                XMLReader p = properties[i];
                string value = p.Attribute("value") ?? p.CurrentContents();
                attributes.Add(p.Attribute("name"), value);
            }

            return attributes;
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
    }
}