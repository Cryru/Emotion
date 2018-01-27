// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Soul.Engine.ECS;
using Soul.Engine.Graphics.Components;

#endregion

namespace Soul.Engine.Graphics.Systems
{
    internal class TextRenderer : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(RenderData), typeof(TextData)};
        }

        protected internal override void Setup()
        {
            // Set priority to before the render system.
            Priority = 7;
        }

        protected override void Update(Entity entity)
        {
            // Get components.
            RenderData renderData = entity.GetComponent<RenderData>();
            TextData textData = entity.GetComponent<TextData>();

            // Check if the render data vertices have updated.
            if (textData.HasUpdated)
            {
                // render text texture

                // set texture.
            }
        }

        #endregion
    }
}