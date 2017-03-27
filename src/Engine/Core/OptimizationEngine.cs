using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Detects performance problems and fixes them.
    /// </summary>
    public static class Opt
    {

        /// <summary>
        /// Prevents the creation of too many rendertargets that are the same.
        /// </summary>
        #region "RenderTarget Cluster Creation"
        private static int rcc_times = 0;
        private static Vector2 rcc_last_size;
        private static RenderTarget2D rcc_cache;

        public static void rcc(int w, int h)
        {
            if (w == rcc_last_size.X && h == rcc_last_size.Y) rcc_times++;
            else { rcc_last_size = new Vector2(w, h); rcc_times = 0; }
        }

        public static RenderTarget2D rcc_define(int w, int h)
        {
            if (rcc_times > 5)
            {
                if(rcc_cache == null) rcc_cache = new RenderTarget2D(Context.Graphics, w, h);
                return rcc_cache;
            }
            else
            {
                Debugging.Logger.Add("Allocating graphic memory for new buffer (" + w + ", " + h + ")");
                return new RenderTarget2D(Context.Graphics, w, h);
            }
           
        }
        #endregion

    }
}
