using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Events;
using SoulEngine.Enums;
using System;
using System.Linq;
using System.Globalization;

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
        /// <param name="Width">The desired width of the render target.</param>
        /// <param name="Height">The desired height of the render target.</param>
        public static void StartRenderTarget(this SpriteBatch ink, ref RenderTarget2D Target, int Width = 0, int Height = 0)
        {
            if (!Context.Core.__composeAllowed) throw new Exception("Cannot compose outside of the frame start sequence.");

            //Redefine target if needed.
            if (Width != 0 && Height != 0) DefineTarget(ref Target, Width, Height);

            //Set the current rendertarget to the drawer.
            Context.Graphics.SetRenderTarget(Target);

            //Clear the rendertarget.
            Context.Graphics.Clear(Color.Transparent);

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

        /// <summary>
        /// Creates a color from a string.
        /// </summary>
        /// <param name="color">The color object.</param>
        /// <param name="String">The string to parse.</param>
        /// <param name="Separator"></param>
        /// <returns></returns>
        public static Color fromString(this Color color, string String, char Separator = '-')
        {
            try
            {
                if (String.Contains("#"))
                {
                    String = String.Replace("#", "");
                    byte r = byte.Parse(String.Substring(0, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(String.Substring(2, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(String.Substring(4, 2), NumberStyles.HexNumber);
                    return new Color(r, g, b);
                }
                else
                {
                    int[] args = String.Split(Separator).Select(x => int.Parse(x)).ToArray();
                    return color = new Color(args[0], args[1], args[2]);
                }
            }
            catch (Exception)
            {
                Debugging.Logger.Add("Invalid color argument: " + String);
                return color = new Color(0, 0, 0, 0);
            }
        }
        #endregion

        #region "Others"
        /// <summary>
        /// Defines or redefines the provided render target to it's specified dimensions.
        /// </summary>
        /// <param name="Target">The render target to define or redefine.</param>
        /// <param name="Width">The desired width of the target.</param>
        /// <param name="Height">The desired height of the target.</param>
        public static void DefineTarget(ref RenderTarget2D Target, int Width = 0, int Height = 0)
        {
            //Check if the render target is the same size as the draw area, because if it's not we need to redefine it.
            if (!(Target == null ||
                Width != Target.Bounds.Size.X ||
                Height != Target.Bounds.Size.Y)) return;

            //Destroy previous render target safely, if any.
            if (Target != null) Target.Dispose();

            //Generate a new rendertarget with the specified size.
            Target = new RenderTarget2D(Context.Graphics, Width, Height);
            Debugging.Logger.Add("Allocating graphic memory for new buffer (" + Width + ", " + Height + ")");
        }
        #endregion
    }
}