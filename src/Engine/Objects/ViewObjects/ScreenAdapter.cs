using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Enums;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An adapter for scaling and boxing the screen when the window doesn't fit the resolution.
    /// </summary>
    public class ScreenAdapter
    {
        #region "Declarations"
        /// <summary>
        /// The transformation matrix to render through.
        /// </summary>
        public Matrix View
        {
            get
            {
                return Matrix.CreateScale((float)Context.Graphics.Viewport.Width / Settings.Width,
                    (float)Context.Graphics.Viewport.Height / Settings.Height, 1.0f);
            }
        }
        #endregion

        /// <summary>
        /// Updates the screen adapter. Called by a system event when the window size changes.
        /// </summary>
        public void Update()
        {
            Viewport viewport = Context.Graphics.Viewport;

            //Get the scale ratio between the viewport and the game's resolution, based on whichever the bigger side is.
            float scale = MathHelper.Min((float)viewport.Width / (Settings.Width), (float)viewport.Height / (Settings.Height));

            //Scale the width and height based on the calculated scale.
            int width = (int)(scale * Settings.Width); //These two values had + 0.5f, research why.
            int height = (int)(scale * Settings.Height);

            /* 
             * Generate the new viewport by enlarging the smaller side as much as possible,
             * and moving the viewport in order to center the visible area.
             */
            Context.Graphics.Viewport = new Viewport(
                (viewport.Width / 2) - (width / 2),
                (viewport.Height / 2) - (height / 2),
                width, height);
        }
    }
}