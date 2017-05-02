using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Base class for components.
    /// </summary>
    public abstract class DrawComponent : Component
    {
        #region "Declarations"
        #region "Rendering Data"
        /// <summary>
        /// The texture of the component, if any.
        /// </summary>
        public virtual Texture2D Texture { get; set; }
        /// <summary>
        /// The padding of the texture.
        /// </summary>
        public Vector2 Padding = new Vector2();
        /// <summary>
        /// The opacity of the texture.
        /// </summary>
        public float Opacity = 1;
        /// <summary>
        /// The mirror effects of the component.
        /// </summary>
        public SpriteEffects MirrorEffects = SpriteEffects.None;
        /// <summary>
        /// The color tint of the texture.
        /// </summary>
        public Color Tint = Color.White;
        #endregion
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// Draws the component's texture.
        /// </summary>
        public override void Draw()
        {
            Draw(attachedObject.Width, attachedObject.Height);
        }
        /// <summary>
        /// Draws the component's texture.
        /// </summary>
        /// <param name="Width">The drawing height of the object's texture.</param>
        /// <param name="Height">The drawing width of the object's texture.</param>
        public virtual void Draw(int Width, int Height)
        {
            Draw(Width, Height, attachedObject.X, attachedObject.Y, Texture);
        }
        /// <summary>
        /// Draws the component's texture.
        /// </summary>
        /// <param name="Width">The drawing height of the object's texture.</param>
        /// <param name="Height">The drawing width of the object's texture.</param>
        /// <param name="X">The X axis location to draw the texture to.</param>
        /// <param name="Y">The Y axis location to draw the texture to.</param>
        public virtual void Draw(int Width, int Height, int X, int Y)
        {
            Draw(Width, Height, X, Y, Texture);
        }
        /// <summary>
        /// Draws the component's texture.
        /// </summary>
        /// <param name="Texture">The texture to draw.</param>
        public virtual void Draw(Texture2D Texture)
        {
            Draw(attachedObject.Width, attachedObject.Height, attachedObject.X, attachedObject.Y, Texture);
        }
        /// <summary>
        /// Draws the component's texture.
        /// </summary>
        /// <param name="Width">The drawing height of the object's texture.</param>
        /// <param name="Height">The drawing width of the object's texture.</param>
        /// <param name="X">The X axis location to draw the texture to.</param>
        /// <param name="Y">The Y axis location to draw the texture to.</param>
        /// <param name="Texture">The texture to draw.</param>
        public virtual void Draw(int Width, int Height, int X, int Y, Texture2D Texture)
        {
            //Calculate texture position with padding.
            int XA = X + (int)Padding.X;
            int YA = Y + (int)Padding.Y;

            if (Width == -1) Width = attachedObject.Width;
            if (Height == -1) Height = attachedObject.Height;

            //Correct bounds to center origin.
            Rectangle DrawBounds = new Rectangle(new Point(XA + Width / 2,
                YA + Height / 2),
                new Point(Width, Height));

            //Check if the component has a texture to render.
            if (Texture != null)
            {
                //Draw the object through XNA's SpriteBatch.
                Context.ink.Draw(Texture,
                    DrawBounds,
                    null,
                    Tint * Opacity,
                    attachedObject.Rotation,
                    new Vector2((float)Texture.Width / 2, (float)Texture.Height / 2),
                    MirrorEffects,
                    1.0f);
            }

            //Check if drawing bounds.
            if (Settings.DrawTextureBounds)
            {
                Context.ink.DrawRectangle(new Rectangle(XA, YA, Width, Height), Math.Max(1, Functions.ManualRatio(1, 540)), Color.Yellow);
            }
        }
        #endregion

        //Other
        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //Free resources.
                attachedObject = null;
                Texture = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}
