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
    public sealed unsafe class LineMapBuffer : MapBuffer
    {
        #region Properties

        /// <summary>
        /// The ibo to be used by all QuadMapBuffers.
        /// </summary>
        private static readonly IndexBuffer _ibo;

        #endregion

        static LineMapBuffer()
        {
            // Generate indices.
            ushort[] indices = new ushort[Renderer.MaxRenderable * 8];
            uint offset = 0;
            for (int i = 0; i < indices.Length; i += 8)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 1);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 2);
                indices[i + 5] = (ushort) (offset + 3);
                indices[i + 6] = (ushort) (offset + 3);
                indices[i + 7] = (ushort) (offset + 0);

                offset += 4;
            }

            _ibo = new IndexBuffer(indices);

            Helpers.CheckError("map buffer - creating ibo");
        }

        /// <inheritdoc />
        public LineMapBuffer(int size) : base(size, 4, _ibo, 8, PrimitiveType.Lines)
        {
        }

        #region Mapping

        /// <summary>
        /// Maps the current quad and advances the current index by one quad.
        /// </summary>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        public void MapNextQuad(Vector3 location, Vector2 size, Color color)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();
            uint c = ColorToUint(color);

            InternalMapVertex(c, -1, Vector2.Zero, location);
            _dataPointer++;

            InternalMapVertex(c, -1, Vector2.Zero, new Vector3(location.X + size.X, location.Y, location.Z));
            _dataPointer++;

            InternalMapVertex(c, -1, Vector2.Zero, new Vector3(location.X + size.X, location.Y + size.Y, location.Z));
            _dataPointer++;

            InternalMapVertex(c, -1, Vector2.Zero, new Vector3(location.X, location.Y + size.Y, location.Z));
            _dataPointer++;
        }

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="index">The index of the quad to map.</param>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        public void MapQuadAt(int index, Vector3 location, Vector2 size, Color color)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map.
            MovePointerToVertex(index * ObjectSize);
            MapNextQuad(location, size, color);
        }

        /// <summary>
        /// Maps the current line and adv
        /// </summary>
        /// <param name="pointOne">The location of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        public void MapNextLine(Vector3 pointOne, Vector3 pointTwo, Color color)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            uint c = ColorToUint(color);

            InternalMapVertex(c, -1, Vector2.Zero, pointOne);
            _dataPointer++;

            InternalMapVertex(c, -1, Vector2.Zero, pointOne);
            _dataPointer++;

            InternalMapVertex(c, -1, Vector2.Zero, pointTwo);
            _dataPointer++;

            InternalMapVertex(c, -1, Vector2.Zero, pointTwo);
            _dataPointer++;
        }

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        public void MapLineAt(int index, Vector3 pointOne, Vector3 pointTwo, Color color)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map.
            MovePointerToVertex(index * ObjectSize);
            MapNextLine(pointOne, pointTwo, color);
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

        #endregion
    }
}