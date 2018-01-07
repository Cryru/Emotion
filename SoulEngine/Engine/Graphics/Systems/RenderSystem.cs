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
                renderData.UpdateVertices();

            // Arrange priority if changed.
            if (renderData.PriorityUpdated)
                Links = Links.OrderBy((x) => x.GetComponent<RenderData>().Priority).ToList();

            // Update the model matrix if the transform has updated.
            if (transform.HasUpdated)
            {
                Vector2 center;
                Vector2 offset = new Vector2(0, 0);

                // Check if a line.
                if (renderData._vertices.Length == 2)
                {
                    // If a line the center is the first point.
                    center = renderData._vertices[0];
                }
                else // If not calculate bounds and center.
                {
                    // Calculate size.
                    Vector2 calculatedSize = new Vector2(0, 0);

                    for (int i = 0; i < renderData._vertices?.Length; i++)
                    {
                        if (renderData._vertices[i].X * transform.Size.X > calculatedSize.X)
                            calculatedSize.X = (int) (renderData._vertices[i].X * transform.Size.X);
                        else if (renderData._vertices[i].X * transform.Size.X < offset.X)
                            offset.X = renderData._vertices[i].X * transform.Size.X;

                        if (renderData._vertices[i].Y * transform.Size.Y > calculatedSize.Y)
                            calculatedSize.Y = (int) (renderData._vertices[i].Y * transform.Size.Y);
                        else if (renderData._vertices[i].Y * transform.Size.Y < offset.Y)
                            offset.Y = renderData._vertices[i].Y * transform.Size.Y;
                    }

                    // Reverse offset.
                    offset *= -1;
                    // Subtract offset.
                    calculatedSize = calculatedSize - offset;

                    // Calculate center.
                    center = new Vector2(transform.X + calculatedSize.X / 2, transform.Y + calculatedSize.Y / 2);
                }

                // Perform moving.
                Matrix4 translation = Matrix4.CreateTranslation(transform.X + offset.X, transform.Y + offset.Y, 0);
                // Perform rotation translation for rotating around the center.
                Matrix4 rotation = 
                    Matrix4.CreateTranslation(-(center.X - transform.X), -(center.Y - transform.Y), 0) *
                    Matrix4.CreateRotationZ(Convert.DegreesToRadians(transform.Rotation)) *
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
                //if (_texture != null)
                //{
                //    Window.Current.SetTextureModelMatrix(_texture.TextureModelMatrix);
                //    Window.Current.SetTexture(_texture);
                //}

                // _textureVBO?.EnableShaderAttribute(2, 2);
                renderData.ColorVBO.EnableShaderAttribute(1, 4);
                renderData.VerticesVBO.EnableShaderAttribute(0, 2);
                renderData.VerticesVBO.Draw(renderData.GetPointCount() == 2
                    ? PrimitiveType.Lines
                    : PrimitiveType.TriangleFan); // Force line drawing when 2 vertices.
                renderData.VerticesVBO.DisableShaderAttribute(0);
                renderData.ColorVBO.DisableShaderAttribute(1);
                //_textureVBO?.DisableShaderAttribute(2);


                // Restore normal MVP. (Maybe this isn't needed)
                Window.Current.SetModelMatrix(Matrix4.Identity);
            }
        }

        #endregion
    }
}