#region Using

using System;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Game.ThreeDee
{
    public class Object3D : Transform3D, IRenderable
    {
        public MeshEntity Entity;

        public void Render(RenderComposer c)
        {
            // Render using the render stream.
            // todo: larger meshes should create their own data buffers.
            // todo: culling state.
            if (Entity?.Meshes == null) return;

            c.FlushRenderStream();

            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);
            Gl.FrontFace(FrontFaceDirection.Ccw);

            c.PushModelMatrix(_rotationMatrix * _scaleMatrix * _translationMatrix);
            Mesh[] meshes = Entity.Meshes;
            for (var i = 0; i < meshes.Length; i++)
            {
                Mesh obj = meshes[i];
                VertexData[] vertData = obj.Vertices;
                ushort[] indices = obj.Indices;
                Texture texture = null;
                if (obj.Material?.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
                RenderStreamBatch<VertexData>.StreamData memory = c.RenderStream.GetStreamMemory((uint) vertData.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

                vertData.CopyTo(memory.VerticesData);
                indices.CopyTo(memory.IndicesData);

                ushort structOffset = memory.StructIndex;
                for (var j = 0; j < memory.IndicesData.Length; j++)
                {
                    memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
                }
            }

            c.PopModelMatrix();

            c.FlushRenderStream();
            Gl.Disable(EnableCap.CullFace);
        }
    }
}