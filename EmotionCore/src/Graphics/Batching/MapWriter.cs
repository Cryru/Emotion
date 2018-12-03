// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <summary>
    /// Assists with mapping buffers.
    /// </summary>
    public static class MapWriter
    {
        /// <summary>
        /// Moves the pointer to the specified index and maps the vertex.
        /// </summary>
        /// <param name="buffer">The buffer to map.</param>
        /// <param name="index">The index of the vertex to map.</param>
        /// <param name="vertex">The location of the vertex AKA the vertex itself.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex, if any.</param>
        /// <param name="uv">The uv of the vertex's texture, if any.</param>
        public static void MapVertexAt(this MapBuffer buffer, int index, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Check if mapping has started.
            if (!buffer.Mapping) buffer.StartMapping();

            // Move the pointer and map the vertex.
            buffer.MovePointerToVertex(index);
            MapNextVertex(buffer, vertex, color, texture, uv);
        }

        /// <summary>
        /// Maps the current vertex and advanced the current index by one.
        /// </summary>
        /// <param name="buffer">The buffer to map.</param>
        /// <param name="vertex">The location of the vertex AKA the vertex itself.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex, if any.</param>
        /// <param name="uv">The uv of the vertex's texture, if any.</param>
        public static void MapNextVertex(this MapBuffer buffer, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Check if mapping has started.
            if (!buffer.Mapping) buffer.StartMapping();

            buffer.UnsafeMapVertex(color.ToUint(), buffer.GetTid(texture), Verify2dUV(texture, uv), vertex);
            buffer.IncrementPointer(1);
        }

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="buffer">The buffer to map.</param>
        /// <param name="index">The index of the quad to map.</param>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad.</param>
        /// <param name="textureArea">The texture area (UV) of the quad.</param>
        public static void MapQuadAt(this MapBuffer buffer, int index, Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Check if mapping has started.
            if (!buffer.Mapping) buffer.StartMapping();

            // Move the pointer and map.
            buffer.MovePointerToVertex(index * buffer.ObjectSize);
            MapNextQuad(buffer, location, size, color, texture, textureArea);
        }

        /// <summary>
        /// Maps the current quad and advances the current index by one quad.
        /// </summary>
        /// <param name="buffer">The buffer to map.</param>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad.</param>
        /// <param name="textureArea">The texture area (UV) of the quad.</param>
        public static void MapNextQuad(this MapBuffer buffer, Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Check if mapping has started.
            if (!buffer.Mapping) buffer.StartMapping();

            Rectangle uv = VerifyRectUV(texture, textureArea);
            float tid = buffer.GetTid(texture);
            uint c = color.ToUint();

            // Calculate UV positions
            Vector2 nnUV = texture == null ? Vector2.Zero : Vector2.TransformPosition(uv.Location, texture.TextureMatrix);
            Vector2 pnUV = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uv.X + uv.Width, uv.Y), texture.TextureMatrix);
            Vector2 npUV = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uv.X, uv.Y + uv.Height), texture.TextureMatrix);
            Vector2 ppUV = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uv.X + uv.Width, uv.Y + uv.Height), texture.TextureMatrix);
            
            // Calculate vert positions.
            Vector3 pnV = new Vector3(location.X + size.X, location.Y, location.Z);
            Vector3 npV = new Vector3(location.X, location.Y + size.Y, location.Z);
            Vector3 ppV = new Vector3(location.X + size.X, location.Y + size.Y, location.Z);

            buffer.UnsafeMapVertex(c, tid, nnUV, location);
            buffer.IncrementPointer(1);

            buffer.UnsafeMapVertex(c, tid, pnUV, pnV);
            buffer.IncrementPointer(1);

            buffer.UnsafeMapVertex(c, tid, ppUV, ppV);
            buffer.IncrementPointer(1);

            buffer.UnsafeMapVertex(c, tid, npUV, npV);
            buffer.IncrementPointer(1);
        }

        /// <summary>
        /// Maps the current line and advances the pointer.
        /// </summary>
        /// <param name="buffer">The buffer to map.</param>
        /// <param name="pointOne">The location of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public static void MapNextLine(this MapBuffer buffer, Vector3 pointOne, Vector3 pointTwo, Color color, int thickness = 1)
        {
            // Check if mapping has started.
            if (!buffer.Mapping) buffer.StartMapping();

            uint c = color.ToUint();
            Vector2 normal = new Vector2(pointTwo.Y - pointOne.Y, -(pointTwo.X - pointOne.X)).Normalized() * thickness;
            float z = Math.Max(pointOne.Z, pointTwo.Z);

            buffer.UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X + normal.X, pointOne.Y + normal.Y, z));
            buffer.IncrementPointer(1);

            buffer.UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X + normal.X, pointTwo.Y + normal.Y, z));
            buffer.IncrementPointer(1);

            buffer.UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X - normal.X, pointTwo.Y - normal.Y, z));
            buffer.IncrementPointer(1);

            buffer.UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X - normal.X, pointOne.Y - normal.Y, z));
            buffer.IncrementPointer(1);
        }

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="buffer">The buffer to map.</param>
        /// <param name="index">The index of the vertex to map.</param>
        /// <param name="pointOne">The location of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public static void MapLineAt(this MapBuffer buffer, int index, Vector3 pointOne, Vector3 pointTwo, Color color, int thickness = 1)
        {
            // Check if mapping has started.
            if (!buffer.Mapping) buffer.StartMapping();

            // Move the pointer and map.
            buffer.MovePointerToVertex(index * buffer.ObjectSize);
            MapNextLine(buffer, pointOne, pointTwo, color, thickness);
        }

        #region Helpers

        /// <summary>
        /// Verifies the uv.
        /// </summary>
        /// <param name="texture">The texture the uv is for.</param>
        /// <param name="uv">The uv to verify.</param>
        /// <returns></returns>
        private static Vector2 Verify2dUV(Texture texture, Vector2? uv)
        {
            // If no texture, the uv is empty.
            if (texture == null) return Vector2.Zero;

            return uv ?? Vector2.One;
        }

        /// <summary>
        /// Verifies the uv.
        /// </summary>
        /// <param name="texture">The texture the uv is for.</param>
        /// <param name="uvRect">The uv rectangle to verify.</param>
        /// <returns></returns>
        private static Rectangle VerifyRectUV(Texture texture, Rectangle? uvRect)
        {
            if (texture == null) return Rectangle.Empty;

            // Get the UV rectangle. If none specified then the whole texture area is chosen.
            if (uvRect == null)
                return new Rectangle(0, 0, texture.Size.X, texture.Size.Y);
            return (Rectangle) uvRect;
        }

        #endregion
    }
}