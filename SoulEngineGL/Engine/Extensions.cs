// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine
{
    public static class Extensions
    {
        #region Rendering

        /// <summary>
        /// Starts drawing.
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        /// <param name="drawLocation">The matrix to render on.</param>
        public static void Start(this SpriteBatch ink, DrawLocation drawLocation = DrawLocation.Terminus)
        {
            // Determine the transformation matrix based on the draw location.
            Matrix? transformationMatrix = null;
            switch (drawLocation)
            {
                case DrawLocation.Screen:
                    transformationMatrix = WindowManager.GetScreenMatrix();
                    break;
                case DrawLocation.World:
                    transformationMatrix = WindowManager.Camera.GetMatrix();
                    break;
            }

            // Set the current drawing location.
            WindowManager.CurrentLocation = drawLocation;

            // Set a tag showing that drawing has started.
            ink.Tag = true;

            // Start drawing.
            ink.Begin(SpriteSortMode.Deferred, null, Settings.Smooth ? null : SamplerState.PointClamp, null,
                RasterizerState.CullNone, null, transformationMatrix);
        }

        /// <summary>
        /// Stops drawing.
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        public static void Stop(this SpriteBatch ink)
        {
            // Set the drawing location to none.
            WindowManager.CurrentLocation = DrawLocation.None;

            // Set the tag to false to show that drawing has ended.
            ink.Tag = false;

            ink.End();
        }

        #endregion

        #region RenderTarget

        /// <summary>
        /// Start drawing on the provided render target.
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        /// <param name="target">The render target to render on.</param>
        public static void SetRenderTarget(this SpriteBatch ink, ref RenderTarget2D target)
        {
            //Set the current render target to the drawer.
            Core.Context.GraphicsDevice.SetRenderTarget(target);

            //Clear the render target.
            Core.Context.GraphicsDevice.Clear(Color.Transparent);
        }

        /// <summary>
        /// Stop drawing on the render target.
        /// </summary>
        /// <param name="ink">The spritebatch to use.</param>
        public static void UnsetRenderTarget(this SpriteBatch ink)
        {
            // Return to the default render target.
            Core.Context.GraphicsDevice.SetRenderTarget(null);

            // Update the screen because render targets mess with the viewport settings.
            WindowManager.UpdateWindow();
        }

        #endregion

        #region Primitives

        /// <summary>
        /// Creates a color from a string.
        /// </summary>
        /// <param name="color">The color object.</param>
        /// <param name="htmlString">The string to parse.</param>
        /// <param name="separator"></param>
        /// <returns></returns>
        // ReSharper disable once RedundantAssignment
        // ReSharper disable once UnusedParameter.Global
        public static Color FromString(this Color color, string htmlString, char separator = '-')
        {
            try
            {
                if (htmlString.Contains("#"))
                {
                    htmlString = htmlString.Replace("#", "");
                    byte r = byte.Parse(htmlString.Substring(0, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(htmlString.Substring(2, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(htmlString.Substring(4, 2), NumberStyles.HexNumber);
                    return new Color(r, g, b);
                }
                int[] args = htmlString.Split(separator).Select(int.Parse).ToArray();
                // ReSharper disable once RedundantAssignment
                return color = new Color(args[0], args[1], args[2]);
            }
            catch (Exception)
            {
                ErrorHandling.Raise(DiagnosticMessageType.Error, "Invalid color argument: " + htmlString);
                // ReSharper disable once RedundantAssignment
                return color = new Color(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Creates a rectangle from two vectors.
        /// </summary>
        /// <param name="rectangle">The rectangle object.</param>
        /// <param name="location">The coordinate location of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public static Rectangle FromVectors(this Rectangle rectangle, Vector2 location, Vector2 size)
        {
            return new Rectangle(location.ToPoint(), size.ToPoint());
        }

        /// <summary>
        /// Returns whether the Vector2 is within the Rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle object.</param>
        /// <param name="point">The point to check for intersection.</param>
        public static bool Intersects(this Rectangle rectangle, Vector2 point)
        {
            return rectangle.Intersects(new Rectangle().FromVectors(point, new Vector2(1, 1)));
        }

        #endregion
    }
}