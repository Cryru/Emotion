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
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.ECS.Systems
{
    internal class ComponentUpdateCleanup : SystemBase
    {
        #region System API

        protected internal override Type[] GetRequirements()
        {
            // All components.
            return null;
        }

        protected internal override void Setup()
        {
            // Set priority to highest so it is run last.
            Priority = 9;
        }

        protected override void Update(Entity entity)
        {
            int componentCount = entity.GetComponentCount();

            for (int i = 0; i < componentCount; i++)
            {
                entity.GetComponent(i).HasUpdated = false;
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