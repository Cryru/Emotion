#region Using

using System;
using System.Numerics;
using Adfectus.Primitives;

#endregion

namespace Adfectus.Graphics
{
    public abstract class RenderTarget : IDisposable
    {
        /// <summary>
        /// The render target's target.
        /// </summary>
        public Texture Texture { get; protected set; }

        /// <summary>
        /// The size of the target.
        /// </summary>
        public Vector2 Size { get; protected set; }

        /// <summary>
        /// The target's viewport.
        /// </summary>
        public Rectangle Viewport { get; set; }

        protected RenderTarget(Vector2 size, Texture texture)
        {
            Size = size;
            Texture = texture;
            Viewport = new Rectangle(0, 0, size);
        }

        public abstract void Dispose();
    }
}