#region Using

using System;
using System.Collections.Generic;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command
{
    public class RenderVerticesCommand : RecyclableCommand
    {
        public List<VertexData> Vertices = new List<VertexData>();
        public int VerticesUtilization;

        public void AddVertex(VertexData vert)
        {
            if (VerticesUtilization == Vertices.Count)
                Vertices.Add(vert);
            else
                Vertices[VerticesUtilization] = vert;

            VerticesUtilization++;
        }

        public override void Recycle()
        {
            VerticesUtilization = 0;
        }

        public override void Process()
        {
        }

        public override void Execute(RenderComposer composer)
        {
            Span<VertexData> vertMapper = composer.VertexBuffer.CreateMapper<VertexData>(0, VerticesUtilization * VertexData.SizeInBytes);
            for (var v = 0; v < VerticesUtilization; v++)
            {
                vertMapper[v] = Vertices[v];
            }

            composer.VertexBuffer.FinishMapping();

            // Prepare to draw.
            VertexArrayObject.EnsureBound(composer.CommonVao);
            IndexBuffer.EnsureBound(IndexBuffer.SequentialIbo.Pointer);

            Gl.DrawElements(PrimitiveType.TriangleFan, VerticesUtilization, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }
    }
}