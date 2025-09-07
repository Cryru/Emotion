#nullable enable

#region Using

using OpenGL;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.InteropServices;
using HSL = (int h, int s, int l);
using RGB = (byte r, byte g, byte b);

#endregion

namespace Emotion.Primitives;

/// <summary>
/// A struct representing a four channel color.
/// Stored as RGBA.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Color
{
    /// <summary>
    /// Gets or sets the red component.
    /// </summary>
    public byte R;

    /// <summary>
    /// Gets or sets the green component.
    /// </summary>
    public byte G;

    /// <summary>
    /// Gets or sets the blue component.
    /// </summary>
    public byte B;

    /// <summary>
    /// Gets or sets the alpha component.
    /// </summary>
    public byte A;

    #region Constructors

    /// <summary>
    /// Create a new instance of a color by changing the alpha of another color.
    /// </summary>
    public Color(Color color, byte a)
    {
        R = color.R;
        G = color.G;
        B = color.B;
        A = a;
    }

    /// <summary>
    /// Creates a new instance of color from RGBA bytes.
    /// </summary>
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Creates a new instance of color from RGBA ints.
    /// </summary>
    public Color(int r, int g, int b, int a = 255) : this((byte) r, (byte) g, (byte) b, (byte) a)
    {
    }

    /// <summary>
    /// Create a new instance of color from a HSL tuple.
    /// </summary>
    public Color(HSL hsl)
    {
        RGB rgb = HSLToRGB(hsl);
        R = rgb.r;
        G = rgb.g;
        B = rgb.b;
        A = 255;
    }

    /// <summary>
    /// Create a new instance of color from a RGB tuple.
    /// </summary>
    public Color(RGB rgb)
    {
        R = rgb.r;
        G = rgb.g;
        B = rgb.b;
        A = 255;
    }

    /// <summary>
    /// Create a new instance of color from a HTML code, the "#" symbol is optional.
    /// </summary>
    public Color(string htmlFormat) : this(htmlFormat.AsSpan())
    {
    }

    /// <summary>
    /// Create a new instance of color from a HTML code, the "#" symbol is optional.
    /// </summary>
    public Color(ReadOnlySpan<char> htmlFormat)
    {
        if (htmlFormat.Length < 6)
        {
            R = 255;
            G = 255;
            B = 255;
            A = 255;
            return;
        }

        bool hasHashtag = htmlFormat[0] == '#';
        if (hasHashtag && htmlFormat.Length < 7)
        {
            R = 255;
            G = 255;
            B = 255;
            A = 255;
            return;
        }

        int offset = hasHashtag ? 1 : 0;
        byte.TryParse(htmlFormat.Slice(0 + offset, 2), NumberStyles.HexNumber, null, out R);
        byte.TryParse(htmlFormat.Slice(2 + offset, 2), NumberStyles.HexNumber, null, out G);
        byte.TryParse(htmlFormat.Slice(4 + offset, 2), NumberStyles.HexNumber, null, out B);

        if (htmlFormat.Length > 6 + offset)
            byte.TryParse(htmlFormat.Slice(6 + offset, 2), NumberStyles.HexNumber, null, out A);
        else
            A = 255;
    }

    /// <summary>
    /// Creates a new color, from a packed RGBA uint.
    /// </summary>
    /// <param name="packed">The packed uint.</param>
    public Color(uint packed)
    {
        R = (byte) packed;
        G = (byte) (packed >> 8);
        B = (byte) (packed >> 16);
        A = (byte) (packed >> 24);
    }

    /// <summary>
    /// Creates a new color, from a normalized vector4.
    /// </summary>
    public Color(Vector4 v)
    {
        R = (byte) (v.X * 255);
        G = (byte) (v.Y * 255);
        B = (byte) (v.Z * 255);
        A = (byte) (v.W * 255);
    }

    /// <summary>
    /// Creates a new color, from a normalized vector3.
    /// </summary>
    public Color(Vector3 v)
    {
        R = (byte) (v.X * 255);
        G = (byte) (v.Y * 255);
        B = (byte) (v.Z * 255);
        A = 255;
    }

    /// <summary>
    /// Create a color from four components and a pixel format.
    /// </summary>
    public Color(byte c1, byte c2, byte c3, byte c4, PixelFormat format)
    {
        R = 0;
        G = 0;
        B = 0;
        A = 0;
        switch (format)
        {
            case PixelFormat.Bgra:
                R = c3;
                G = c2;
                B = c1;
                A = c4;
                break;
            case PixelFormat.Rgba:
                R = c1;
                G = c2;
                B = c3;
                A = c4;
                break;
        }
    }

    #endregion

    /// <summary>
    /// Gets the hash code of this <see cref="Color" />.
    /// </summary>
    public override int GetHashCode()
    {
        return (int) ToUint();
    }

    /// <summary>
    /// Create a copy of this color object.
    /// </summary>
    /// <returns></returns>
    [Pure]
    public Color Clone()
    {
        return (Color) MemberwiseClone();
    }

    /// <summary>
    /// Compares whether current instance is equal to specified object.
    /// </summary>
    /// <param name="obj">The <see cref="Color" /> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public override bool Equals(object obj)
    {
        return obj is Color color && Equals(color);
    }

    /// <summary>
    /// Compares whether current instance is equal to specified object.
    /// </summary>
    /// <param name="color">The <see cref="Color" /> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public bool Equals(Color color)
    {
        return color.R == R && color.G == G && color.B == B && color.A == A;
    }

    #region Functions

    /// <summary>
    /// Performs linear interpolation of <see cref="Color" />.
    /// </summary>
    public static Color Lerp(Color value1, Color value2, float amount)
    {
        amount = Maths.Clamp01(amount);
        Vector4 vec1 = new Vector4(value1.R, value1.G, value1.B, value1.A);
        Vector4 vec2 = new Vector4(value2.R, value2.G, value2.B, value2.A);
        Vector4 lerped = Vector4.Lerp(vec1, vec2, amount);

        return new Color(
            (byte) lerped.X,
            (byte) lerped.Y,
            (byte) lerped.Z,
            (byte) lerped.W
        );
    }

    public static Color LerpHSL(Color value1, Color value2, float amount)
    {
        (int h, int s, int l) hsl1 = RGBToHSL(value1.AsRGBTuple());
        (int h, int s, int l) hsl2 = RGBToHSL(value2.AsRGBTuple());

        int newH = (int)Maths.Lerp(hsl1.h, hsl2.h, amount);
        int newS = (int)Maths.Lerp(hsl1.s, hsl2.s, amount);
        int newL = (int)Maths.Lerp(hsl1.l, hsl2.l, amount);
        return new Color(new HSL(newH, newS, newL));
    }

    /// <summary>
    /// Multiply <see cref="Color" /> by value.
    /// </summary>
    /// <param name="value">Source <see cref="Color" />.</param>
    /// <param name="scale">Multiplicator.</param>
    /// <returns>Multiplication result.</returns>
    public static Color Multiply(Color value, float scale)
    {
        return new Color((byte) (value.R * scale), (byte) (value.G * scale), (byte) (value.B * scale), (byte) (value.A * scale));
    }

    /// <summary>
    /// Multiply <see cref="Color" /> by value.
    /// </summary>
    public static Color Add(Color value, byte amount)
    {
        return new Color((byte) (value.R + amount), (byte) (value.G + amount), (byte) (value.B + amount), value.A);
    }

    /// <summary>
    /// Translate a non-premultipled alpha <see cref="Color" /> to a <see cref="Color" /> that contains premultiplied alpha.
    /// </summary>
    /// <param name="r">Red component value.</param>
    /// <param name="g">Green component value.</param>
    /// <param name="b">Blue component value.</param>
    /// <param name="a">Alpha component value.</param>
    /// <returns>A <see cref="Color" /> which contains premultiplied alpha data.</returns>
    public static Color FromNonPremultiplied(int r, int g, int b, int a)
    {
        return new Color((byte) (r * a / 255), (byte) (g * a / 255), (byte) (b * a / 255), (byte) a);
    }

    /// <summary>
    /// Set the color's alpha.
    /// Since this is a struct you will get a copy of this color with the alpha set.
    /// </summary>
    /// <param name="a">The alpha to set to.</param>
    /// <returns>Returns a copy of the color with the set alpha - for chaining.</returns>
    public Color SetAlpha(byte a)
    {
        A = a;
        return this;
    }

    public Color SetAlpha(float a)
    {
        A = (byte) (255f * a);
        return this;
    }

    public Color CloneWithAlpha(byte a)
    {
        return new Color(R, G, B, a);
    }


    #endregion

    #region Static Operators

    /// <summary>
    /// Compares whether two <see cref="Color" /> instances are equal.
    /// </summary>
    /// <param name="a"><see cref="Color" /> instance on the left of the equal sign.</param>
    /// <param name="b"><see cref="Color" /> instance on the right of the equal sign.</param>
    /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(Color a, Color b)
    {
        return a.A == b.A &&
               a.R == b.R &&
               a.G == b.G &&
               a.B == b.B;
    }

    /// <summary>
    /// Compares whether two <see cref="Color" /> instances are not equal.
    /// </summary>
    /// <param name="a"><see cref="Color" /> instance on the left of the not equal sign.</param>
    /// <param name="b"><see cref="Color" /> instance on the right of the not equal sign.</param>
    /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(Color a, Color b)
    {
        return !(a == b);
    }

    public static Color operator +(Color c1, Color c2)
    {
        return new Color((byte) Math.Min(c1.R + c2.R, 255),
            (byte) Math.Min(c1.G + c2.G, 255),
            (byte) Math.Min(c1.B + c2.B, 255),
            (byte) Math.Min(c1.A + c2.A, 255));
    }

    public static Color operator -(Color c1, Color c2)
    {
        return new Color((byte) Math.Max(c1.R - c2.R, 0),
            (byte) Math.Max(c1.G - c2.G, 0),
            (byte) Math.Max(c1.B - c2.B, 0),
            (byte) Math.Max(c1.A - c2.A, 0));
    }

    public static Color operator *(Color c1, Color c2)
    {
        return new Color((byte) (c1.R * c2.R / 255),
            (byte) (c1.G * c2.G / 255),
            (byte) (c1.B * c2.B / 255),
            (byte) (c1.A * c2.A / 255));
    }

    public static Color operator *(Color c1, float a)
    {
        c1.A = (byte) (c1.A * a);
        return c1;
    }

    public static Color operator *(Color c1, byte a)
    {
        c1.A = (byte) (c1.A * (a / 255.0f));
        return c1;
    }

    #endregion

    /// <summary>
    /// Formats the color as a string. The format is R-G-B-A
    /// </summary>
    /// <returns><see cref="string" /> representation of this <see cref="Color" />.</returns>
    public override string ToString()
    {
        return R + "-" + G + "-" + B + "-" + A;
    }

    #region Predefined

    public static readonly Color Black = new Color(0, 0, 0);
    public static readonly Color White = new Color(255, 255, 255);
    public static readonly Color Red = new Color(255, 0, 0);
    public static readonly Color Green = new Color(0, 255, 0);
    public static readonly Color Blue = new Color(0, 0, 255);
    public static readonly Color Yellow = new Color(255, 255, 0);
    public static readonly Color Magenta = new Color(255, 0, 255);
    public static readonly Color Cyan = new Color(0, 255, 255);
    public static readonly Color CornflowerBlue = new Color(100, 149, 237);
    public static readonly Color Pink = new Color(237, 100, 149);
    public static readonly Color Transparent = new Color(0, 0, 0, 0);
    public static readonly Color TransparentWhite = new Color(255, 255, 255, 0);

    public static readonly uint WhiteUint = White.ToUint();

    public static readonly Color PrettyPurple = new Color("#8062D6");
    public static readonly Color PrettyPink = new Color("#E966A0");
    public static readonly Color PrettyOrange = new Color("#FD8D14");
    public static readonly Color PrettyYellow = new Color("#FFE17B");
    public static readonly Color PrettyRed = new Color("#EF6262");
    public static readonly Color PrettyBlue = new Color("#39B5E0");
    public static readonly Color PrettyGreen = new Color("#A4BE7B");
    public static readonly Color PrettyBrown = new Color("#693b10");

    #endregion

    /// <summary>
    /// Converts the color object to an uint.
    /// </summary>
    /// <returns>A uint packed value representing the color.</returns>
    public readonly uint ToUint()
    {
        return (uint) A << 24 | (uint) B << 16 | (uint) G << 8 | R;
    }

    /// <summary>
    /// Create a normalized vec4 from the color.
    /// </summary>
    /// <returns>A normalized vec4 from the color.</returns>
    public Vector4 ToVec4()
    {
        return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
    }

    /// <summary>
    /// Converts a four component color to an uint.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    /// <returns>A uint packed value representing the color.</returns>
    public static uint ToUint(byte r, byte g, byte b, byte a)
    {
        return (uint) a << 24 | (uint) b << 16 | (uint) g << 8 | r;
    }

    public static Color RandomColor()
    {
        RGB rgb = HSLToRGB((Helpers.GenerateRandomNumber(0, 359), 59, 61));
        return new Color(rgb);
    }

    public static implicit operator Color(string htmlCode)
    {
        return new Color(htmlCode);
    }

    #region HSL

    public static HSL RGBToHSL(RGB rgb)
    {
        float rd = rgb.r / 255.0f;
        float gd = rgb.g / 255.0f;
        float bd = rgb.b / 255.0f;

        float max = Math.Max(rd, Math.Max(gd, bd));
        float min = Math.Min(rd, Math.Min(gd, bd));
        float delta = max - min;

        float l = (max + min) / 2.0f;

        // Achromatic
        if (delta == 0)
            return new HSL(0, 0, (int)MathF.Round(l * 100f));

        // Calculate saturation
        float s;
        if (l < 0.5)
            s = delta / (max + min);
        else
            s = delta / (2.0f - max - min);

        // Calculate hue
        float h;
        if (rd == max)
            h = (gd - bd) / delta + (gd < bd ? 6 : 0);
        else if (gd == max)
            h = (bd - rd) / delta + 2;
        else
            h = (rd - gd) / delta + 4;

        h /= 6;

        return new HSL(
            (int)MathF.Round(h * 360),
            (int)MathF.Round(s * 100),
            (int)MathF.Round(l * 100)
        );
    }

    private static float HueToRGB(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1 / 6.0f) return p + (q - p) * 6 * t;
        if (t < 1 / 2.0f) return q;
        if (t < 2 / 3.0f) return p + (q - p) * (2 / 3.0f - t) * 6;
        return p;
    }

    public static RGB HSLToRGB(HSL hsl)
    {
        float h = hsl.h / 360.0f;
        float s = hsl.s / 100.0f;
        float l = hsl.l / 100.0f;

        if (s == 0) // achromatic
        {
            byte v = (byte)(l * 255);
            return new RGB(v, v, v);
        }

        float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
        float p = 2f * l - q;

        byte r = (byte)Math.Round(HueToRGB(p, q, h + 1 / 3.0f) * 255);
        byte g = (byte)Math.Round(HueToRGB(p, q, h) * 255);
        byte b = (byte)Math.Round(HueToRGB(p, q, h - 1 / 3.0f) * 255);
        return new RGB(r, g, b);
    }

    #endregion

    public RGB AsRGBTuple()
    {
        return (R, G, B);
    }
}