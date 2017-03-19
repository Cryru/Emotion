using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Events;
using SoulEngine.Enums;
using System;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Engine functions.
    /// </summary>
    static class Functions
    {
        #region "Screen Functions"
        /// <summary>
        /// Returns the size of the primary physical screen.
        /// </summary>
        public static Vector2 GetScreenSize()
        {
            return new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        }
        #endregion

        #region "Extensions"
        /// <summary>
        /// Starts a pixel perfect draw stack on the specified channel. 
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        /// <param name="DrawChannel">The channel to render on.</param>
        /// <param name="Parallax">Parallax factor for the World channel.</param>
        public static void Start(this SpriteBatch ink, DrawChannel DrawChannel = DrawChannel.Terminus, Vector2? Parallax = null)
        {
            //Define a render matrix to determine later, by default it's null.
            Matrix? transformationMatrix = null;

            //Determine which channel we are rendering on, and get its matrix.
            switch (DrawChannel)
            {
                case DrawChannel.Screen:
                    transformationMatrix = Context.Screen.View;
                    break;
                case DrawChannel.World: //If on the world channel then check for parallax.
                    if (Parallax != null)
                        transformationMatrix = Context.Camera.ViewParallax(Parallax.Value);
                    else
                        transformationMatrix = Context.Camera.View;
                    break;
            }

            //Start drawing.
            ink.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, RasterizerState.CullNone, null, transformationMatrix);
        }

        /// <summary>
        /// Start drawing on the provided render target.
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        /// <param name="Target">The render target to render on.</param>
        public static void StartRenderTarget(this SpriteBatch ink, RenderTarget2D Target)
        {
            //Set the current rendertarget to the drawer.
            Context.Graphics.SetRenderTarget(Target);

            //Clear the rendertarget.
            Context.Graphics.Clear(Color.Aqua);

            //Start drawing.
            ink.Start();
        }

        /// <summary>
        /// Stop drawing and return the render target. 
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        public static void EndRenderTarget(this SpriteBatch ink)
        {
            //Start drawing.
            ink.End();

            //Return to the default render target.
            Context.Graphics.SetRenderTarget(null);

            //Return the viewport holder.
            Context.Screen.Update();
        }

        /// <summary>
        /// Returns a portion of an array.
        /// </summary>
        /// <param name="data">The array.</param>
        /// <param name="index">The starting location, included.</param>
        /// <param name="length">The length of the range, if -1 then until the length is until the end array.</param>
        public static T[] SubArray<T>(this T[] data, int index, int length = -1)
        {
            if (length == -1) length = data.Length - index;

            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        #endregion
    }
}