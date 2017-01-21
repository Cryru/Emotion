using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    static class Context
    {
        /// <summary>
        /// 
        /// </summary>
        public static Core Core { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static SpriteBatch ink { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static GraphicsDeviceManager GraphicsManager { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static GraphicsDevice Graphics
        {
            get
            {
                return Core.GraphicsDevice;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Camera2D Camera;
        /// <summary>
        /// 
        /// </summary>
        public static BoxingViewportAdapter Screen;
    }
}
