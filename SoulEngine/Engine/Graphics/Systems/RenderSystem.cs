// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Systems;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using System.Linq;

#endregion

namespace Soul.Engine.Graphics.Systems
{
    internal class RenderSystem : SystemBase
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
            // Hook up to drawing call of the scene.
            Parent.DrawHook = DrawHook;
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
                Links = Links.OrderBy((x) => x.GetComponent<RenderData>().Priority).ToList();
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
                    Vector2 calculatedSize = Helpers.CalculateSizeFromVertices(renderData.Vertices, transform.Size, out offset);

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

        #endregion

        #region Draw Code

        private void DrawHook()
        {
            // Draw all drawables.
            foreach (Entity link in Links)
            {
                RenderData renderData = link.GetComponent<RenderData>();

                // Compute the MVP for this object.
                Window.Current.SetModelMatrix(renderData.ModelMatrix);
                // If a texture is attached add the texture and model matrix.
                if (renderData.Texture != null)
                {
                    Window.Current.SetTextureModelMatrix(renderData.Texture.TextureModelMatrix);
                    Window.Current.SetTexture(renderData.Texture);
                }

                renderData.TextureVBO?.EnableShaderAttribute(2, 2);
                renderData.ColorVBO.EnableShaderAttribute(1, 4);
                renderData.VerticesVBO.EnableShaderAttribute(0, 2);
                renderData.VerticesVBO.Draw(renderData.GetPointCount() == 2
                    ? PrimitiveType.Lines
                    : PrimitiveType.TriangleFan); // Force line drawing when 2 vertices.
                renderData.VerticesVBO.DisableShaderAttribute(0);
                renderData.ColorVBO.DisableShaderAttribute(1);
                renderData.TextureVBO?.DisableShaderAttribute(2);

                Window.Current.StopUsingTexture();

                // Restore normal MVP. (Maybe this isn't needed)
                Window.Current.SetModelMatrix(Matrix4.Identity);
            }
        }

        #endregion
    }
}