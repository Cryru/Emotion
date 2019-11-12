#region Using

using System;
using System.Numerics;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command
{
    public class RenderSpriteCommand : RecyclableCommand
    {
        public Vector3 Position;
        public Vector2 Size;
        public uint Color;
        public Texture Texture { get; set; }
        public Rectangle? UV;
        public Matrix4x4? TextureModifier;

        /// <summary>
        /// The vertices of the sprite. Are set when it is processed.
        /// </summary>
        public VertexData[] Vertices { get; protected set; } = {new VertexData(), new VertexData(), new VertexData(), new VertexData()};

        /// <summary>
        /// Whether the sprite has been processed.
        /// If within a batch, this also means it was mapped.
        /// </summary>
        public bool Processed { get; protected set; }

        public override void Recycle()
        {
            Processed = false;
        }

        public override void Process()
        {
            Vertices[0].Vertex = Position;
            Vertices[1].Vertex = new Vector3(Position.X + Size.X, Position.Y, Position.Z);
            Vertices[2].Vertex = new Vector3(Position.X + Size.X, Position.Y + Size.Y, Position.Z);
            Vertices[3].Vertex = new Vector3(Position.X, Position.Y + Size.Y, Position.Z);

            Vertices[0].Color = Color;
            Vertices[1].Color = Color;
            Vertices[2].Color = Color;
            Vertices[3].Color = Color;

            // Note: Texture id is set by the Texturable batch.
            if (Texture == null) return;

            // If no UV specified - fill entire.
            if (UV == null) UV = new Rectangle(0, 0, Texture.Size);

            Rectangle uvRect = UV.Value;

            // Multiply UVs by the texture matrix to get the right values.
            Matrix4x4 matrix;
            if (TextureModifier != null)
            {
                matrix =  Texture.TextureMatrix * (Matrix4x4) TextureModifier;
            }
            else
            {
                matrix = Texture.TextureMatrix;
            }

            Vector2 nnUV = Vector2.Transform(uvRect.Position, matrix);
            Vector2 pnUV = Vector2.Transform(new Vector2(uvRect.X + uvRect.Width, uvRect.Y), matrix);
            Vector2 ppUV = Vector2.Transform(new Vector2(uvRect.X + uvRect.Width, uvRect.Y + uvRect.Height), matrix);
            Vector2 npUV = Vector2.Transform(new Vector2(uvRect.X, uvRect.Y + uvRect.Height), matrix);

            // Add a small epsilon to prevent the wrong UVs from being sampled.
            nnUV = new Vector2(nnUV.X + MathFloat.EPSILON, nnUV.Y - MathFloat.EPSILON);
            pnUV = new Vector2(pnUV.X - MathFloat.EPSILON, pnUV.Y - MathFloat.EPSILON);
            ppUV = new Vector2(ppUV.X + MathFloat.EPSILON, ppUV.Y + MathFloat.EPSILON);
            npUV = new Vector2(npUV.X - MathFloat.EPSILON, npUV.Y + MathFloat.EPSILON);

            Vertices[0].UV = nnUV;
            Vertices[1].UV = pnUV;
            Vertices[2].UV = ppUV;
            Vertices[3].UV = npUV;

            Processed = true;
        }

        public override void Execute(RenderComposer composer)
        {
            Span<VertexData> vertMapper = composer.VertexBuffer.CreateMapper<VertexData>(0, Vertices.Length * VertexData.SizeInBytes);
            for (var v = 0; v < Vertices.Length; v++)
            {
                vertMapper[v] = Vertices[v];
            }

            composer.VertexBuffer.FinishMapping();

            // Prepare to draw.
            VertexArrayObject.EnsureBound(composer.CommonVao);
            IndexBuffer.EnsureBound(IndexBuffer.QuadIbo.Pointer);
            Texture.EnsureBound(Texture.Pointer);

            Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }
    }
}