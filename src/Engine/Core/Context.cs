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

        public static Engine Engine { get; set; }
        public static Legacy.Core Core { get; set; }

        public static SpriteBatch ink { get; set; }
        public static GraphicsDeviceManager graphics { get; set; }

        public static Camera2D Camera;
        public static BoxingViewportAdapter Screen;
    }
}
