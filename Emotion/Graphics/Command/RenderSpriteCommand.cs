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

            // Convert input from texture coordinates to UV coordinates.
            Rectangle uvRect = UV.Value;
            // 0, 1    1, 1
            // 0, 0    1, 0
            float uvXP = uvRect.X + uvRect.Width;
            float uvYn = Texture.Size.Y - uvRect.Y;
            float uvYp = Texture.Size.Y - (uvRect.Y + uvRect.Height);
            Vector2 npUV = new Vector2(uvRect.X, uvYn) / Texture.Size;
            Vector2 ppUV = new Vector2(uvXP, uvYn) / Texture.Size;
            Vector2 pnUV = new Vector2(uvXP, uvYp) / Texture.Size;
            Vector2 nnUV = new Vector2(uvRect.X, uvYp) / Texture.Size;

            // Add a small epsilon to prevent the wrong UVs from being sampled.
            nnUV = new Vector2(nnUV.X + MathFloat.EPSILON, nnUV.Y - MathFloat.EPSILON);
            pnUV = new Vector2(pnUV.X - MathFloat.EPSILON, pnUV.Y - MathFloat.EPSILON);
            ppUV = new Vector2(ppUV.X + MathFloat.EPSILON, ppUV.Y + MathFloat.EPSILON);
            npUV = new Vector2(npUV.X - MathFloat.EPSILON, npUV.Y + MathFloat.EPSILON);

            // Same order as vertices.
            Vertices[0].UV = npUV;
            Vertices[1].UV = ppUV;
            Vertices[2].UV = pnUV;
            Vertices[3].UV = nnUV;

            Matrix4x4 texMatrix = Texture.TextureMatrix;
            // Add sprite modifier - if any.
            if (TextureModifier != null)
            {
                texMatrix *= TextureModifier.Value;
            }

            for (var i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].UV = Vector2.Transform(Vertices[i].UV, texMatrix);
            }

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
            if (Texture != null) Texture.EnsureBound(Texture.Pointer);

            Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }
    }
}