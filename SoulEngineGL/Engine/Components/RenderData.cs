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
        public Texture2D Texture { get; set; }
        public Rectangle? TextureArea { get; set; }
        public DrawLocation DrawLocation { get; set; } = DrawLocation.Screen;
        public Color Tint { get; set; } = Color.White;
        public float Opacity { get; set; } = 1f;
        public bool Visible { get; set; } = true;
    }
}