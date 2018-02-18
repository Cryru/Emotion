// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Soul.Engine.ECS
{
    public partial class Entity
    {
        /// <summary>
        /// The position of the transform along the x axis.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The position of the transform along the y axis.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// The width of the transform.
        /// </summary>
        public float Width { get; set; } = 100;

        /// <summary>
        /// The height of the transform.
        /// </summary>
        public float Height { get; set; } = 100;

        /// <summary>
        /// The transform's rotation in radians.
        /// </summary>
        public float Rotation { get; set; } = 0;

        /// <summary>
        /// The position of the transform.
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// The size of the transform.
        /// </summary>
        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        /// <summary>
        /// The center of the transform.
        /// </summary>
        public Vector2 Center
        {
            get { return new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2); }
            set
            {
                X = (int) Math.Ceiling(value.X - Width / 2);
                Y = (int) Math.Ceiling(value.Y - Height / 2);
            }
        }

        /// <summary>
        /// The bounds of the transform.
        /// </summary>
        public Rectangle Bounds
        {
            get { return new Rectangle(Position.ToPoint(), Size.ToPoint()); }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }
    }
}