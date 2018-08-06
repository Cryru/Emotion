// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Soul;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Color" /> struct.
        /// </summary>
        /// <param name="color">A <see cref="Color" /> for RGB values of new <see cref="Color" /> instance.</param>
        /// <param name="a">Alpha component value.</param>
        public Color(Color color, byte a)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = a;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Emotion.Primitives.Color" /> struct.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value</param>
        /// <param name="a">Alpha component value.</param>
        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new instance of <see cref="T:Emotion.Primitives.Color" /> struct.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value</param>
        /// <param name="a">Alpha component value.</param>
        public Color(int r, int g, int b, int a = 255) : this((byte) r, (byte) g, (byte) b, (byte) a)
        {
        }

        /// <summary>
        /// Create a new instance of <see cref="Color" /> struct from an HTML hex string.
        /// </summary>
        /// <param name="htmlFormat">An HTML hex string containing the color.</param>
        public Color(string htmlFormat)
        {
            htmlFormat = htmlFormat.Replace("#", "");
            R = byte.Parse(htmlFormat.Substring(0, 2), NumberStyles.HexNumber);
            G = byte.Parse(htmlFormat.Substring(2, 2), NumberStyles.HexNumber);
            B = byte.Parse(htmlFormat.Substring(4, 2), NumberStyles.HexNumber);
            A = 255;
        }

        #endregion

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

        /// <summary>
        /// Gets the hash code of this <see cref="Color" />.
        /// </summary>
        /// <returns>Hash code of this <see cref="Color" />.</returns>
        public override int GetHashCode()
        {
            return R ^ G ^ B ^ A;
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

        #region Functions

        /// <summary>
        /// Performs linear interpolation of <see cref="Color" />.
        /// </summary>
        /// <param name="value1">Source <see cref="Color" />.</param>
        /// <param name="value2">Destination <see cref="Color" />.</param>
        /// <param name="amount">Interpolation factor.</param>
        /// <returns>Interpolated <see cref="Color" />.</returns>
        public static Color Lerp(Color value1, Color value2, float amount)
        {
            amount = MathHelper.Clamp(amount, 0, 1);
            return new Color(
                (byte) MathHelper.Lerp(value1.R, value2.R, amount),
                (byte) MathHelper.Lerp(value1.G, value2.G, amount),
                (byte) MathHelper.Lerp(value1.B, value2.B, amount),
                (byte) MathHelper.Lerp(value1.A, value2.A, amount));
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

        public static Color operator *(Color c1, float f1)
        {
            c1.A = (byte) (c1.A * f1);
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

        #endregion
    }
}