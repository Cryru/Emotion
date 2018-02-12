// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Linq;
using Breath.Objects;
using Breath.Systems;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;

#endregion

namespace Soul.Engine.Graphics.Systems
{
    internal class Renderer : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            return new[] {typeof(RenderData), typeof(Transform)};
        }

        protected internal override void Setup()
        {
            // Set priority to highest so it is run almost last.
            Priority = 8;

            // Declare drawing intention.
            Draws = true;
        }

        protected override void Update(Entity entity)
        {
            // Get components.
            RenderData renderData = entity.GetComponent<RenderData>();
            Transform transform = entity.GetComponent<Transform>();

            // Check if the render data vertices have updated.
            if (renderData.HasUpdated)
            {
                renderData.UpdateVertices();
                Links = Links.OrderBy(x => x.GetComponent<RenderData>().Priority).ToList();
            }

            // Update the model matrix if the transform has updated.
            if (transform.HasUpdated)
            {
                Vector2 center;
                Vector2 offset = new Vector2(0, 0);

                // Check if a line.
                if (renderData.Vertices.Length == 2)
                {
                    // If a line the center is the first point.
                    center = renderData.Vertices[0];
                }
                else
                {
                    // Calculate the bounds of the vertices.
                    Vector2 calculatedSize =
                        Helpers.CalculateSizeFromVertices(renderData.Vertices, transform.Size, out offset);

                    // Calculate center.
                    center = new Vector2(transform.X + calculatedSize.X / 2, transform.Y + calculatedSize.Y / 2);
                }

                // Perform moving.
                Matrix4 translation = Matrix4.CreateTranslation(transform.X + offset.X, transform.Y + offset.Y, 0);
                // Perform rotation translation for rotating around the center.
                Matrix4 rotation =
                    Matrix4.CreateTranslation(-(center.X - transform.X), -(center.Y - transform.Y), 0) *
                    Matrix4.CreateRotationZ(transform.Rotation) *
                    Matrix4.CreateTranslation(center.X - transform.X, center.Y - transform.Y, 0);
                // Scale it up.
                Matrix4 scale = Matrix4.CreateScale(transform.Width, transform.Height, 1);

                renderData.ModelMatrix = scale * rotation * translation;
            }
        }

        protected override void Draw(Entity link)
        {
            RenderData renderData = link.GetComponent<RenderData>();

            // Check whether to render.
            if(!renderData.Enabled) return;

            // Render the drawable.
            Core.BreathWin.Draw(renderData.BreathDrawable, renderData.ModelMatrix, renderData.GetPointCount() == 2
                ? PrimitiveType.Lines
                : PrimitiveType.TriangleFan);
        }

        #endregion
    }
}