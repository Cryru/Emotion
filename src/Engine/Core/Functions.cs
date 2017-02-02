using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Events;

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
    }
}
//TODO: EXTENSION FOR GENRENDERTARGET ON THE GRAPHICSDEVICE