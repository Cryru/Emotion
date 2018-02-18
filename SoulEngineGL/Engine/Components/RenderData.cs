// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.ECS;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Components
{
    public class RenderData : ComponentBase
    {
        public Texture2D Texture;
        public Rectangle? TextureArea;
        public DrawLocation DrawLocation = DrawLocation.Screen;
        public Color Tint = Color.White;
        public bool Visible = true;
    }
}