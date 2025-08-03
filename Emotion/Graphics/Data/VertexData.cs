#region Using

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics.Data
{
    /// <summary>
    /// Represents the data of a single vertex.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        /// <summary>
        /// The size of vertex data in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new VertexData());

        /// <summary>
        /// The vertex itself.
        /// </summary>
        [VertexAttribute(3, false)] public Vector3 Vertex;

        /// <summary>
        /// The UV of the vertex's texture.
        /// </summary>
        [VertexAttribute(2, false)] public Vector2 UV;

        /// <summary>
        /// The packed color of the vertex.
        /// </summary>
        [VertexAttribute(4, true, typeof(byte))]
        public uint Color;

        /// <summary>
        /// Convert common sprite parameters to VertexData vertices.
        /// </summary>
        /// <param name="vertices">The data to fill.</param>
        /// <param name="position">The sprite's position.</param>
        /// <param name="size">The sprite's size.</param>
        /// <param name="color">The sprite's color.</param>
        /// <param name="texture">The sprite's texture - if any.</param>
        /// <param name="textureArea">The texture UV - or what part of the texture the sprite should use.</param>
        /// <param name="flipX">Whether to flip the texture horizontally.</param>
        /// <param name="flipY">Whether to flip the texture vertically.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SpriteToVertexData(Span<VertexData> vertices, Vector3 position, Vector2 size, Color color,
            Texture texture = null, Rectangle? textureArea = null, bool flipX = false, bool flipY = false
        )
        {
            vertices[3].Vertex = position;
            vertices[2].Vertex = new Vector3(position.X + size.X, position.Y, position.Z);
            vertices[1].Vertex = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);
            vertices[0].Vertex = new Vector3(position.X, position.Y + size.Y, position.Z);

            uint c = color.ToUint();
            vertices[3].Color = c;
            vertices[2].Color = c;
            vertices[1].Color = c;
            vertices[0].Color = c;

            texture ??= Texture.EmptyWhiteTexture;

            Vector2 textureSize = texture.Size;
            if (texture is FrameBufferTexture fbT) textureSize = fbT.FrameBuffer.Size;

            // If no UV specified - use entire texture.
            textureArea ??= new Rectangle(0, 0, textureSize);

            if (texture.FlipY)
            {
                Rectangle r = textureArea.Value;
                r.Y = textureSize.Y - (textureArea.Value.Y + textureArea.Value.Height);
                textureArea = r;
            }

            // Convert input from texture coordinates to UV coordinates.
            TransformUVs(vertices, texture, (Rectangle)textureArea);

            if (texture.FlipY != flipY) FlipHorizontallySpriteUVs(vertices);

            // ReSharper disable once InvertIf
            if (flipX) FlipVerticallySpriteUVs(vertices);
        }

        public static void WriteDefaultQuadUV(Span<VertexData> vertices)
        {
            vertices[3].UV = Vector2.Zero;
            vertices[2].UV = new Vector2(1, 0);
            vertices[1].UV = Vector2.One;
            vertices[0].UV = new Vector2(0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TransformUVs(Span<VertexData> vertices, Texture texture, Rectangle uvRect)
        {
            uvRect.Y = texture.Size.Y - uvRect.Height - uvRect.Y;

            // Add a small epsilon to prevent the wrong UVs from being sampled from floating point errors.
            // Crucial for pixel art as every pixel matters, and camera can cause "half-pixels" to be sampled.
            uvRect.X += Maths.EPSILON_BIGGER;
            uvRect.Y += Maths.EPSILON_BIGGER;
            uvRect.Width -= Maths.EPSILON_BIGGER_2;
            uvRect.Height -= Maths.EPSILON_BIGGER_2;

            // 0, 1    1, 1
            // 0, 0    1, 0
            float uvXP = uvRect.X + uvRect.Width;
            float uvYn = texture.Size.Y - uvRect.Y;
            float uvYp = texture.Size.Y - (uvRect.Y + uvRect.Height);
            Vector2 npUV = new Vector2(uvRect.X, uvYn) / texture.Size;
            Vector2 ppUV = new Vector2(uvXP, uvYn) / texture.Size;
            Vector2 pnUV = new Vector2(uvXP, uvYp) / texture.Size;
            Vector2 nnUV = new Vector2(uvRect.X, uvYp) / texture.Size;

            // Same order as vertices.
            // [3] 0, 0  [2] 1, 0
            // [0] 0, 1  [1] 1, 1
            vertices[3].UV = nnUV;
            vertices[2].UV = pnUV;
            vertices[1].UV = ppUV;
            vertices[0].UV = npUV;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FlipHorizontallySpriteUVs(Span<VertexData> vertices)
        {
            // Flipped Y
            // 0, 1    1, 1
            // 0, 0    1, 0
            Vector2 temp = vertices[0].UV;
            vertices[0].UV = vertices[3].UV;
            vertices[3].UV = temp;
            temp = vertices[1].UV;
            vertices[1].UV = vertices[2].UV;
            vertices[2].UV = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FlipVerticallySpriteUVs(Span<VertexData> vertices)
        {
            // Flipped X
            // 1, 0    0, 0
            // 1, 1    0, 1
            Vector2 temp = vertices[0].UV;
            vertices[0].UV = vertices[1].UV;
            vertices[1].UV = temp;
            temp = vertices[3].UV;
            vertices[3].UV = vertices[2].UV;
            vertices[2].UV = temp;
        }

        public override string ToString()
        {
            return $"Vertex: {Vertex}";
        }
    }
}