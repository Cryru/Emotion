using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An object used to transform the way things are rendered.
    /// </summary>
    public abstract class ViewObject : GameObject
    {
        /// <summary>
        /// The transformation matrix of the view object.
        /// </summary>
        public abstract Matrix View { get; }

        /// <summary>
        /// Converts a view coordinate to a screen coordinate.
        /// </summary>
        /// <param name="x">The X axis coordinate of a point on the view to convert.</param>
        /// <param name="y">The Y axis coordinate of a point on the view to convert.</param>
        /// <returns>The screen coordinate that corresponds to the provided view coordinate.</returns>
        public Vector2 ViewToScreen(float x, float y)
        {
            return ViewToScreen(new Vector2(x, y));
        }
        /// <summary>
        /// Converts a view coordinate to a screen coordinate.
        /// </summary>
        /// <param name="ViewPoint">The view coordinate to convert.</param>
        /// <returns>The screen coordinate that corresponds to the provided view coordinate.</returns>
        public Vector2 ViewToScreen(Vector2 ViewPoint)
        {
            var viewport = Context.Graphics.Viewport;
            return Vector2.Transform(ViewPoint + new Vector2(viewport.X, viewport.Y), View);
        }

        /// <summary>
        /// Converts a screen coordinate to a view coordinate.
        /// </summary>
        /// <param name="x">The X axis coordinate of a point on the screen to convert./param>
        /// <param name="y">The Y axis coordinate of a point on the screen to convert.</param>
        /// <returns>The view coordinate that corresponds to the provided screen coordinate.</returns>
        public Vector2 ScreenToView(float x, float y)
        {
            return ScreenToView(new Vector2(x, y));
        }
        /// <summary>
        /// Converts a screen coordinate to a view coordinate.
        /// </summary>
        /// <param name="ScreenPoint">The screen coordinate to convert.</param>
        /// <returns>The view coordinate that corresponds to the provided screen coordinate.</returns>
        public Vector2 ScreenToView(Vector2 ScreenPoint)
        {
            var viewport = Context.Graphics.Viewport;
            return Vector2.Transform(ScreenPoint - new Vector2(viewport.X, viewport.Y), Matrix.Invert(View));
        }
    }
}
