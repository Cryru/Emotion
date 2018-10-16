// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Debug;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.System;
using Emotion.Utils;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    public sealed unsafe class QuadMapBuffer : MapBuffer
    {
        #region Properties

        /// <summary>
        /// The number of quads mapped. Also the index of the highest mapped quad.
        /// </summary>
        public int MappedQuads
        {
            get => MappedVertices / 4;
        }

        #endregion

        /// <inheritdoc />
        public QuadMapBuffer(int size) : base(size)
        {
        }

        #region Mapping

        /// <summary>
        /// Maps the current quad and advanced the current index by one quad.
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

            Rectangle uv = VerifyUV(texture, textureArea);
            float tid = GetTid(texture);
            uint c = ColorToUint(color);

            // Calculate UV positions
            Vector2 nn = texture == null ? Vector2.Zero : Vector2.TransformPosition(uv.Location, texture.TextureMatrix);
            Vector2 pn = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uv.X + uv.Width, uv.Y),  texture.TextureMatrix);
            Vector2 np = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uv.X, uv.Y + uv.Height),  texture.TextureMatrix);
            Vector2 pp = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uv.X + uv.Width, uv.Y + uv.Height),  texture.TextureMatrix);

            InternalMapVertex(c, tid, nn, location);
            _dataPointer++;
            InternalMapVertex(c, tid, pn, new Vector3(location.X + size.X, location.Y, location.Z));
            _dataPointer++;
            InternalMapVertex(c, tid, pp, new Vector3(location.X + size.X, location.Y + size.Y, location.Z));
            _dataPointer++;
            InternalMapVertex(c, tid, np, new Vector3(location.X, location.Y + size.Y, location.Z));
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

            // Move the pointer and map the vertex.
            MovePointerToVertex(index * 4);
            MapNextQuad(location, size, color, texture, textureArea);
        }

        #endregion

        #region Helpers

        private Rectangle VerifyUV(Texture texture, Rectangle? uvRect)
        {
            if (texture == null) return Rectangle.Empty;

            // Get the UV rectangle. If none specified then the whole texture area is chosen.
            if (uvRect == null)
                return new Rectangle(0, 0, texture.Size.X, texture.Size.Y);
            return (Rectangle) uvRect;
        }

        /// <summary>
        /// Set the render range for the buffer.
        /// </summary>
        /// <param name="startIndex">The index of the quad to start drawing from.</param>
        /// <param name="endIndex">The index of the quad to stop drawing at. If -1 will draw to MappedVertices.</param>
        public new void SetRenderRange(int startIndex = 0, int endIndex = -1)
        {
            base.SetRenderRange(startIndex == 0 ? startIndex : startIndex * 4, endIndex == -1 ? endIndex : endIndex * 4);
        }

        #endregion
    }
}