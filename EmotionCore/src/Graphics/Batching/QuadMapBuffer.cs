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
        /// The IBO holding the buffer indices for all QuadMapBuffers.
        /// </summary>
        private static IndexBuffer _ibo;

        /// <summary>
        /// The list of textures the buffer TIDs (Texture IDs) require, in the correct order.
        /// </summary>
        private List<Texture> _textureList;

        #endregion

        /// <summary>
        /// Generate the IBO used by all QuadMapBuffers.
        /// </summary>
        static QuadMapBuffer()
        {
            // Generate indices.
            ushort[] indices = new ushort[Renderer.MaxRenderable * 6];
            uint offset = 0;
            for (int i = 0; i < indices.Length; i += 6)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 2);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 3);
                indices[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }

            _ibo = new IndexBuffer(indices);

            Helpers.CheckError("map buffer - creating ibo");
        }

        /// <inheritdoc />
        public QuadMapBuffer(int size) : base(size)
        {
            _textureList = new List<Texture>();
        }

        #region Mapping

        /// <summary>
        /// Start mapping the buffer.
        /// </summary>
        /// <param name="resetLoadedTextures">Whether to reset the list of loaded textures.</param>
        public void Start(bool resetLoadedTextures)
        {
            // Reset loaded textures if needed.
            if (resetLoadedTextures) _textureList.Clear();

            // Start mapping.
            Start();
        }

        /// <summary>
        /// Map a part of the buffer as a quad.
        /// </summary>
        /// <param name="location">The location of the vertices.</param>
        /// <param name="size">The size of the vertices.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="texture">The texture of the vertices.</param>
        /// <param name="textureArea">The texture area (UV) of the vertices.</param>
        public void Add(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Convert the color to an int.
            uint c = ((uint) color.A << 24) | ((uint) color.B << 16) | ((uint) color.G << 8) | color.R;

            // Map texture to texture list.
            int tid = -1;
            Matrix4 textureMatrix = Matrix4.Identity;
            Rectangle uvRect = Rectangle.Empty;

            // Check if the renderable has a texture.
            if (texture != null)
            {
                // Get the texture matrix.
                textureMatrix = texture.TextureMatrix;

                // Get the UV rectangle. If none specified then the whole texture area is chosen.
                if (textureArea == null)
                    uvRect = new Rectangle(0, 0, texture.Size.X, texture.Size.Y);
                else
                    uvRect = (Rectangle) textureArea;

                // Check if the texture of the renderable is loaded into the list of this buffer.
                for (int i = 0; i < _textureList.Count; i++)
                {
                    if (_textureList[i].Pointer != texture.Pointer) continue; // todo: Try comparing references instead of pointers.
                    tid = i;
                    break;
                }

                // If it wasn't found, add it.
                if (tid == -1)
                    // Check if reached texture limit, in which case the draw calls must be split.
                    if (_textureList.Count >= 16)
                    {
                        throw new Exception("Texture limit of 16 (32 usually but MacGL implementation is lower) reached.");
                    }
                    else
                    {
                        _textureList.Add(texture);
                        tid = _textureList.Count - 1;
                    }
            }

            // Check if render limit reached.
            if (_indicesCount / 6 >= Size) throw new Exception("Render limit of " + Size + " reached.");

            Vector2 nn = texture == null ? Vector2.Zero : Vector2.TransformPosition(uvRect.Location, textureMatrix);
            Vector2 pn = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X + uvRect.Width, uvRect.Y), textureMatrix);
            Vector2 np = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X, uvRect.Y + uvRect.Height), textureMatrix);
            Vector2 pp = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X + uvRect.Width, uvRect.Y + uvRect.Height), textureMatrix);

            // Set four vertices.
            _dataPointer->Vertex = location;
            _dataPointer->UV = nn;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(location.X + size.X, location.Y, location.Z);
            _dataPointer->UV = pn;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(location.X + size.X, location.Y + size.Y, location.Z);
            _dataPointer->UV = pp;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = new Vector3(location.X, location.Y + size.Y, location.Z);
            _dataPointer->UV = np;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            // Increment indices count.
            _indicesCount += 6;
        }

        /// <inheritdoc />
        public override void FastForward(int count)
        {
            base.FastForward(count);
            _dataPointer += 4 * count;
            _indicesCount += 6 * count;
        }

        #endregion

        /// <inheritdoc />
        public override void Render(Renderer _)
        {
            if (!AnythingMapped)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Tried to draw buffer that wasn't mapped.");
                return;
            }

            ThreadManager.ForceGLThread();

            Helpers.CheckError("map buffer - before draw");

            // Bind textures.
            for (int i = 0; i < _textureList.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                _textureList[i].Bind();
            }

            Helpers.CheckError("map buffer - texture binding");

            _vao.Bind();
            _ibo.Bind();
            Helpers.CheckError("map buffer - bind");

            GL.DrawElements(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
            Helpers.CheckError("map buffer - draw");

            _ibo.Unbind();
            _vao.Unbind();
            Helpers.CheckError("map buffer - unbind");
        }
    }
}