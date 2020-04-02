﻿#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Utility;

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
        /// The texture's id within the loaded textures.
        /// </summary>
        [VertexAttribute(1, true)] public float Tid;

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
        /// <param name="texture">The sprite's texture - if any. Take note that this function doesn't set the Tid.</param>
        /// <param name="textureArea">The texture UV - or what part of the texture the sprite should use.</param>
        /// <param name="flipX">Whether to flip the texture horizontally.</param>
        /// <param name="flipY">Whether to flip the texture vertically.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SpriteToVertexData(Span<VertexData> vertices, Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, bool flipX = false,
            bool flipY = false)
        {
            vertices[0].Vertex = position;
            vertices[1].Vertex = new Vector3(position.X + size.X, position.Y, position.Z);
            vertices[2].Vertex = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);
            vertices[3].Vertex = new Vector3(position.X, position.Y + size.Y, position.Z);

            uint c = color.ToUint();
            vertices[0].Color = c;
            vertices[1].Color = c;
            vertices[2].Color = c;
            vertices[3].Color = c;

            // Note: Texture id is set by the batch.
            if (texture == null) return;

            // If no UV specified - fill entire.
            if (textureArea == null) textureArea = new Rectangle(0, 0, texture.Size);

            // Convert input from texture coordinates to UV coordinates.
            var uvRect = (Rectangle) textureArea;
            uvRect.Y = texture.Size.Y - uvRect.Height - uvRect.Y;

            // 0, 1    1, 1
            // 0, 0    1, 0
            float uvXP = uvRect.X + uvRect.Width;
            float uvYn = texture.Size.Y - uvRect.Y;
            float uvYp = texture.Size.Y - (uvRect.Y + uvRect.Height);
            Vector2 npUV = new Vector2(uvRect.X, uvYn) / texture.Size;
            Vector2 ppUV = new Vector2(uvXP, uvYn) / texture.Size;
            Vector2 pnUV = new Vector2(uvXP, uvYp) / texture.Size;
            Vector2 nnUV = new Vector2(uvRect.X, uvYp) / texture.Size;

            // Add a small epsilon to prevent the wrong UVs from being sampled.
            npUV = new Vector2(npUV.X + Maths.EPSILON, npUV.Y - Maths.EPSILON);
            ppUV = new Vector2(ppUV.X - Maths.EPSILON, ppUV.Y - Maths.EPSILON);
            pnUV = new Vector2(pnUV.X - Maths.EPSILON, pnUV.Y + Maths.EPSILON);
            nnUV = new Vector2(nnUV.X + Maths.EPSILON, nnUV.Y + Maths.EPSILON);

            // Same order as vertices.
            // 0, 0    1, 0
            // 0, 1    1, 1
            vertices[0].UV = nnUV;
            vertices[1].UV = pnUV;
            vertices[2].UV = ppUV;
            vertices[3].UV = npUV;

            if (texture.FlipY != flipY)
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
            
            // ReSharper disable once InvertIf
            if (flipX)
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
        }
    }
}