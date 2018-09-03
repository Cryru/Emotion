// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    public sealed unsafe class LineMapBuffer : MapBuffer
    {
        /// <summary>
        /// The IBO holding the buffer indices for all LineMapBuffers.
        /// </summary>
        private static IndexBuffer _lineIBO;

        /// <summary>
        /// Generate the IBO used by all LineMapBuffers.
        /// </summary>
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

            _lineIBO = new IndexBuffer(indices);

            Helpers.CheckError("map buffer - creating ibo");
        }

        /// <inheritdoc />
        public LineMapBuffer(int size) : base(size)
        {
        }

        #region Mapping

        /// <summary>
        /// Map a part of the buffer as an outlined quad.
        /// </summary>
        /// <param name="location">The location of the vertices.</param>
        /// <param name="size">The size of the vertices.</param>
        /// <param name="color">The color of the vertices.</param>
        public void Add(Vector3 location, Vector2 size, Color color)
        {
            // Convert the color to an int.
            uint c = ((uint) color.A << 24) | ((uint) color.B << 16) | ((uint) color.G << 8) | color.R;

            // Check if render limit reached.
            if (_indicesCount / 8 >= Size) throw new Exception("Render limit of " + Size + " reached.");

            // Set four vertices.
            _dataPointer->Vertex = location;
            _dataPointer->Color = c;
            _dataPointer->Tid = -1;
            _dataPointer->UV = new Vector2(0, 0);
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(location.X + size.X, location.Y, location.Z);
            _dataPointer->Color = c;
            _dataPointer->Tid = -1;
            _dataPointer->UV = new Vector2(0, 0);
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(location.X + size.X, location.Y + size.Y, location.Z);
            _dataPointer->Color = c;
            _dataPointer->Tid = -1;
            _dataPointer->UV = new Vector2(0, 0);
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(location.X, location.Y + size.Y, location.Z);
            _dataPointer->Color = c;
            _dataPointer->Tid = -1;
            _dataPointer->UV = new Vector2(0, 0);
            _dataPointer++;

            // Increment indices count.
            _indicesCount += 8;
        }

        /// <summary>
        /// Map a part of the buffer as a line.
        /// </summary>
        /// <param name="pointOne">The location of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        public void Add(Vector3 pointOne, Vector3 pointTwo, Color color)
        {
            // Convert the color to an int.
            uint c = ((uint) color.A << 24) | ((uint) color.B << 16) | ((uint) color.G << 8) | color.R;

            // Check if render limit reached.
            if (_indicesCount / 8 >= Size) throw new Exception("Render limit of " + Size + " reached.");

            // Set four vertices.
            _dataPointer->Vertex = pointOne;
            _dataPointer->Color = c;
            _dataPointer->Tid = -1;
            _dataPointer->UV = new Vector2(0, 0);
            _dataPointer++;

            _dataPointer->Vertex = pointTwo;
            _dataPointer->Color = c;
            _dataPointer->Tid = -1;
            _dataPointer->UV = new Vector2(0, 0);
            _dataPointer++;

            // Increment indices count.
            _indicesCount += 2;
        }

        /// <inheritdoc />
        public override void FastForward(int count)
        {
            base.FastForward(count);
            _dataPointer += 4 * count;
            _indicesCount += 8 * count;
        }

        #endregion

        /// <inheritdoc />
        internal override void Render(Renderer _)
        {
            if (!AnythingMapped)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Tried to draw buffer that wasn't mapped.");
                return;
            }

            ThreadManager.ForceGLThread();

            Helpers.CheckError("map buffer - before draw");

            _vao.Bind();
            _lineIBO.Bind();
            Helpers.CheckError("map buffer - bind");

            GL.DrawElements(PrimitiveType.Lines, _indicesCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
            Helpers.CheckError("map buffer - draw");

            _lineIBO.Unbind();
            _vao.Unbind();
            Helpers.CheckError("map buffer - unbind");
        }
    }
}