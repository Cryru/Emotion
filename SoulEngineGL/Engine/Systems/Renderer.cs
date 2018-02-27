using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Components;
using Soul.Engine.ECS;
using Soul.Engine.Modules;

namespace Soul.Engine.Systems
{
    internal class Renderer : SystemBase
    {
        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(RenderData) };
        }

        protected internal override void Setup()
        {
            // Should be run after other stuff.
            Order = 9;
        }

        internal override void Update(Entity link)
        {

        }

        internal override void Draw(Entity link)
        {
            // Get component.
            RenderData renderData = link.GetComponent<RenderData>();

            // Correct texture if missing.
            Texture2D texture = renderData.Texture ?? AssetLoader.BlankTexture;
            // Correct texture area if missing.
            Rectangle textureArea = renderData.TextureArea ?? new Rectangle(0, 0, texture.Width, texture.Height);
            // Offset location for centered rotation.
            Rectangle drawBounds = new Rectangle((int) (link.X + link.Width / 2), (int) (link.Y + link.Height / 2), (int) link.Width, (int) link.Height);

            Core.Context.Ink.Start(renderData.DrawLocation);

            Core.Context.Ink.Draw(
                texture,
                drawBounds,
                textureArea,
                renderData.Tint * renderData.Opacity,
                link.Rotation,
                new Vector2((float) textureArea.Width / 2, (float) textureArea.Height / 2),
                SpriteEffects.None,
                1f);

            Core.Context.Ink.End();
        }

    }
}
