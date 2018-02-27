// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine.Components.UI
{
    public class Box : ComponentBase
    {
        public Texture2D Texture;
        public Rectangle TopLeft;
        public Rectangle HorizontalTop;
        public Rectangle TopRight;
        public Rectangle VerticalLeft;
        public Rectangle Fill;
        public Rectangle VerticalRight;
        public Rectangle BottomLeft;
        public Rectangle HorizontalBottom;
        public Rectangle BottomRight;
    }
}