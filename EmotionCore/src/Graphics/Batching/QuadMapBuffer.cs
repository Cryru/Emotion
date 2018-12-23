// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <summary>
    /// A map buffer optimized for drawing quads.
    /// </summary>
    public sealed unsafe class QuadMapBuffer : MapBuffer
    {
        /// <inheritdoc />
        public QuadMapBuffer(int size) : base(size, 4, null, 6, PrimitiveType.Triangles)
        {
        }

        #region Friendly Mapping

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="index">The index of the vertex to map.</param>
        /// <param name="pointOne">The location of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void MapLineAt(int index, Vector3 pointOne, Vector3 pointTwo, Color color, int thickness = 1)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map.
            MovePointerToVertex(index * ObjectSize);
            MapNextLine(pointOne, pointTwo, color, thickness);
        }

        /// <summary>
        /// Maps the current line and advances the pointer.
        /// </summary>
        /// <param name="pointOne">The location of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void MapNextLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            uint c = color.ToUint();
            Vector2 normal = Vector2.Normalize(new Vector2(pointTwo.Y - pointOne.Y, -(pointTwo.X - pointOne.X))) * thickness;
            float z = Math.Max(pointOne.Z, pointTwo.Z);

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X + normal.X, pointOne.Y + normal.Y, z));
            _dataPointer++;

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X + normal.X, pointTwo.Y + normal.Y, z));
            _dataPointer++;

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X - normal.X, pointTwo.Y - normal.Y, z));
            _dataPointer++;

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X - normal.X, pointOne.Y - normal.Y, z));
            _dataPointer++;
        }

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="index">The index of the quad to map.</param>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad.</param>
        /// <param name="textureArea">The texture area (UV) of the quad.</param>
        public void MapQuadAt(int index, Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map.
            MovePointerToVertex(index * ObjectSize);
            MapNextQuad(location, size, color, texture, textureArea);
        }

        /// <summary>
        /// Maps the current quad and advances the current index by one quad.
        /// </summary>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad.</param>
        /// <param name="textureArea">The texture area (UV) of the quad.</param>
        public void MapNextQuad(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            Rectangle uv = VerifyRectUV(texture, textureArea);
            float tid = GetTid(texture);
            uint c = color.ToUint();

            // Calculate UV positions
            Vector2 nnUV = texture == null ? Vector2.Zero : Vector2.Transform(uv.Location, texture.TextureMatrix);
            Vector2 pnUV = texture == null ? Vector2.Zero : Vector2.Transform(new Vector2(uv.X + uv.Width, uv.Y), texture.TextureMatrix);
            Vector2 npUV = texture == null ? Vector2.Zero : Vector2.Transform(new Vector2(uv.X, uv.Y + uv.Height), texture.TextureMatrix);
            Vector2 ppUV = texture == null ? Vector2.Zero : Vector2.Transform(new Vector2(uv.X + uv.Width, uv.Y + uv.Height), texture.TextureMatrix);

            // Calculate vert positions.
            Vector3 pnV = new Vector3(location.X + size.X, location.Y, location.Z);
            Vector3 npV = new Vector3(location.X, location.Y + size.Y, location.Z);
            Vector3 ppV = new Vector3(location.X + size.X, location.Y + size.Y, location.Z);

            UnsafeMapVertex(c, tid, nnUV, location);
            _dataPointer++;

            UnsafeMapVertex(c, tid, pnUV, pnV);
            _dataPointer++;

            UnsafeMapVertex(c, tid, ppUV, ppV);
            _dataPointer++;

            UnsafeMapVertex(c, tid, npUV, npV);
            _dataPointer++;
        }

        #endregion
    }
}