// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
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
                indices[i] = (ushort)(offset + 0);
                indices[i + 1] = (ushort)(offset + 1);
                indices[i + 2] = (ushort)(offset + 2);
                indices[i + 3] = (ushort)(offset + 2);
                indices[i + 4] = (ushort)(offset + 3);
                indices[i + 5] = (ushort)(offset + 0);

                offset += 4;
            }

            _ibo = new IndexBuffer(indices);

            Helpers.CheckError("map buffer - creating ibo");
        }

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
        /// <param name="vertMatrix">The matrix to multiply the vertices by.</param>
        public void Add(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
           // Convert the color to an int.
            uint c = ((uint)color.A << 24) | ((uint)color.B << 16) | ((uint)color.G << 8) | color.R;

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
                    uvRect = (Rectangle)textureArea;

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
                    if (_textureList.Count >= 32)
                    {
                        throw new Exception("Texture limit of 32 reached.");
                    }
                    else
                    {
                        _textureList.Add(texture);
                        tid = _textureList.Count - 1;
                    }
            }

            // Determine the vertex matrix.
            Matrix4 vertexMatrix = vertMatrix ?? Matrix4.Identity;

            // Check if render limit reached.
            if (_indicesCount / 6 >= Size) throw new Exception("Render limit of " + Size + " reached.");

            Vector2 nn = texture == null ? Vector2.Zero : Vector2.TransformPosition(uvRect.Location, textureMatrix);
            Vector2 pn = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X + uvRect.Width, uvRect.Y), textureMatrix);
            Vector2 np = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X, uvRect.Y + uvRect.Height), textureMatrix);
            Vector2 pp = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X + uvRect.Width, uvRect.Y + uvRect.Height), textureMatrix);

            // Set four vertices.
            _dataPointer->Vertex = Vector3.TransformPosition(location, vertexMatrix);
            _dataPointer->UV = nn;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(location.X + size.X, location.Y, location.Z), vertexMatrix);
            _dataPointer->UV = pn;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(location.X + size.X, location.Y + size.Y, location.Z), vertexMatrix);
            _dataPointer->UV = pp;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(location.X, location.Y + size.Y, location.Z), vertexMatrix);
            _dataPointer->UV = np;
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            // Increment indices count.
            _indicesCount += 6;
        }

        #endregion

        /// <inheritdoc />
        public override void Draw(Matrix4? bufferMatrix = null, ShaderProgram shader = null)
        {
            if (!AnythingMapped)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Tried to draw buffer that wasn't mapped.");
                return;
            }

            ThreadManager.ForceGLThread();

            Helpers.CheckError("map buffer - before draw");

            // Sync shader.
            shader?.Bind();
            if (bufferMatrix != null)
                ShaderProgram.Current.SetUniformMatrix4("bufferMatrix", (Matrix4)bufferMatrix);
            else
                ShaderProgram.Current.SetUniformMatrix4("bufferMatrix", Matrix4.Identity);
            Helpers.CheckError("map buffer - shader preparation");

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