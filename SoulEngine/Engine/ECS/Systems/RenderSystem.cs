using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breath.Graphics;
using Soul.Engine.ECS.Components;

namespace Soul.Engine.ECS.Systems
{
    internal class RenderSystem : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            return new [] { typeof(RenderData), typeof(Transform) };
        }

        protected internal override void Setup()
        {
            // Hook up to drawing call of the scene.
            Parent.DrawHook = DrawHook;
        }

        protected override void Update(Entity entity)
        {
            // Get components.
            RenderData renderData = entity.GetComponent<RenderData>();
            Transform transform = entity.GetComponent<Transform>();

            // Check if a drawable has been generated for the entity.
            if (renderData.DrawableObject == null)
            {
                // Create a drawable.
                renderData.DrawableObject = Polygon.Rectangle;
                // Link with the color reference from the render data.
                renderData.DrawableObject.Color = renderData.DrawColor;
            }

            // Sync the transform with the render data if it has updated.
            if(transform.HasUpdated) SyncTransform(renderData, transform);
        }

        #endregion

        #region Helpers

        private void SyncTransform(RenderData renderData, Transform transform)
        {
            renderData.DrawableObject.Position = transform.Location;
            renderData.DrawableObject.Size = transform.Size;
            renderData.DrawableObject.RotationDegree = transform.Rotation;
        }

        #endregion

        #region Draw Code

        private void DrawHook()
        {
            // Draw all drawables.
            foreach (Entity link in Links)
            {
                link.GetComponent<RenderData>().DrawableObject.Draw();
            }
        }

        #endregion
    }
}
