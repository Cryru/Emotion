#region Using

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A struct representing a four channel color.
    /// Stored as RGBA.
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
        /// Creates a new instance of <see cref="Color" /> struct.
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
        /// <param name="v">The vector to create a color from.</param>
        public Color(Vector4 v)
        {
            R = (byte) (v.X * 255);
            G = (byte) (v.Y * 255);
            B = (byte) (v.Z * 255);
            A = (byte) (v.W * 255);
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
            return (int) ToUint();
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
        /// <param name="value1">Source <see cref="Color" />.</param>
        /// <param name="value2">Destination <see cref="Color" />.</param>
        /// <param name="amount">Interpolation factor.</param>
        /// <returns>Interpolated <see cref="Color" />.</returns>
        public static Color Lerp(Color value1, Color value2, float amount)
        {
            amount = Maths.Clamp(amount, 0, 1);
            return new Color(
                (byte) Maths.Lerp(value1.R, value2.R, amount),
                (byte) Maths.Lerp(value1.G, value2.G, amount),
                (byte) Maths.Lerp(value1.B, value2.B, amount),
                (byte) Maths.Lerp(value1.A, value2.A, amount));
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

        public static readonly uint WhiteUint = White.ToUint();

        #endregion

        /// <summary>
        /// Converts the color object to an uint.
        /// </summary>
        /// <returns>A uint packed value representing the color.</returns>
        public uint ToUint()
        {
            return ((uint) A << 24) | ((uint) B << 16) | ((uint) G << 8) | R;
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
            return ((uint) a << 24) | ((uint) b << 16) | ((uint) g << 8) | r;
        }
    }
}