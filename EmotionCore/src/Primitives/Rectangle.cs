// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;

#endregion

#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team
All rights reserved.
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A rectangle object
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        #region Public Properties

        public static Rectangle Empty { get; } = new Rectangle();

        public float Left
        {
            get => X;
            set => X = value;
        }

        public float Right
        {
            get => X + Width;
            set => X = value - Width;
        }

        public float Top
        {
            get => Y;
            set => Y = value;
        }

        public float Bottom
        {
            get => Y + Height;
            set => Y = value - Height;
        }

        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }

        #endregion

        #region Constructors

        public Rectangle(Vector2 location, Vector2 size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        #endregion Constructors

        #region Public Methods

        public Vector3 LocationZ(float z)
        {
            return new Vector3(X, Y, z);
        }

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        public bool Contains(float x, float y)
        {
            return X <= x && x < X + Width && Y <= y && y < Y + Height;
        }

        public bool Contains(Vector2 value)
        {
            return X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
        }

        public bool Contains(Rectangle value)
        {
            return X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        public Rectangle Inflate(float horizontalValue, float verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;

            return this;
        }

        public Rectangle Deflate(float horizontalValue, float verticalValue)
        {
            X += horizontalValue;
            Y += verticalValue;
            Width -= horizontalValue * 2;
            Height -= verticalValue * 2;

            return this;
        }

        public bool IsEmpty
        {
            get => Width == 0 && Height == 0 && X == 0 && Y == 0;
        }

        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle && this == rectangle;
        }

        public override string ToString()
        {
            return $"[X:{X} Y:{Y} Width:{Width} Height:{Height}]";
        }

        public override int GetHashCode()
        {
            return (int) Math.Pow(Math.Pow(X, Y), Math.Pow(Width, Height));
        }

        public bool Intersects(Rectangle r2)
        {
            return !(r2.Left > Right
                     || r2.Right < Left
                     || r2.Top > Bottom
                     || r2.Bottom < Top
            );
        }

        #endregion
    }
}