// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emotion.Debug;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.GLES.Buffer;

#endregion

namespace Emotion.Graphics
{
    public sealed unsafe class MapBuffer
    {
        #region Properties

        /// <summary>
        /// The VBO holding the buffer data.
        /// </summary>
        public Buffer VBO { get; private set; }

        /// <summary>
        /// The IBO holding the buffer indices.
        /// </summary>
        public IndexBuffer IBO { get; private set; }

        /// <summary>
        /// The VAO holding the buffer vertex attribute bindings.
        /// </summary>
        public VertexArray VAO { get; private set; }

        /// <summary>
        /// Whether the buffer is currently mapping. Call Draw() to finish mapping.
        /// </summary>
        public bool Mapping
        {
            get => _dataPointer != null;
        }

        /// <summary>
        /// Whether anything is currently mapped into the buffer.
        /// </summary>
        public bool AnythingMapped
        {
            get => _indicesCount != 0;
        }

        /// <summary>
        /// The size of the buffer in vertices.
        /// </summary>
        public int Size { get; private set; }

        #endregion

        #region Draw State

        private VertexData* _dataPointer;
        private ushort _indicesCount;
        private List<Texture> _textureList;

        #endregion

        #region Objects

        private Renderer _renderer;

        #endregion

        /// <summary>
        /// Create a new map buffer of the specified size.
        /// </summary>
        /// <param name="size">The size of the map buffer in vertices.</param>
        /// <param name="renderer">The renderer hosting this map buffer.</param>
        public MapBuffer(int size, Renderer renderer)
        {
            Size = size;
            _renderer = renderer;
            _textureList = new List<Texture>();

            // Calculate the size of the buffer.
            int quadSize = VertexData.SizeInBytes * 4;
            int bufferSize = size * quadSize;

            VBO = new Buffer(bufferSize, 3, BufferUsageHint.DynamicDraw);
            VAO = new VertexArray();

            VAO.Bind();
            VBO.Bind();

            GL.EnableVertexAttribArray(ShaderProgram.VertexLocation);
            GL.VertexAttribPointer(ShaderProgram.VertexLocation, 3, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "Vertex"));

            GL.EnableVertexAttribArray(ShaderProgram.UvLocation);
            GL.VertexAttribPointer(ShaderProgram.UvLocation, 2, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "UV"));

            GL.EnableVertexAttribArray(ShaderProgram.TidLocation);
            GL.VertexAttribPointer(ShaderProgram.TidLocation, 1, VertexAttribPointerType.Float, true, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "Tid"));

            GL.EnableVertexAttribArray(ShaderProgram.ColorLocation);
            GL.VertexAttribPointer(ShaderProgram.ColorLocation, 4, VertexAttribPointerType.UnsignedByte, true, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "Color"));

            VBO.Unbind();
            VAO.Unbind();

            Helpers.CheckError("map buffer - loading vbo into vao");

            ushort[] indices = new ushort[bufferSize * 6];
            ushort offset = 0;
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

            IBO = new IndexBuffer(indices);

            Helpers.CheckError("map buffer - creating ibo");
        }

        /// <summary>
        /// Start mapping the buffer.
        /// </summary>
        public VertexData* Start()
        {
            VBO.Bind();
            _dataPointer = (VertexData*)GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, VertexData.SizeInBytes, BufferAccessMask.MapWriteBit);
            Helpers.CheckError("map buffer - start");

            return _dataPointer;
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
            Helpers.CheckError("before add");

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
                {
                    uvRect = new Rectangle(0, 0, 1, 1);
                    textureMatrix = Matrix4.Identity;
                }
                else
                {
                    uvRect = (Rectangle)textureArea;
                }

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
                        Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Texture limit reached.");

                        // Restart the mapping.
                        Draw();
                        Start();

                        // Clear the texture list.
                        _textureList.Clear();
                        _textureList.Add(texture);
                        tid = 0;
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
            if (_indicesCount >= Size * 6)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Render limit reached.");

                // Restart the mapping.
                Draw();
                Start();
            }

            // Set four vertices.
            _dataPointer->Vertex = Vector3.TransformPosition(location, vertexMatrix);
            _dataPointer->UV = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X, uvRect.Y + uvRect.Height), textureMatrix);
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(location.X + size.X, location.Y, location.Z), vertexMatrix);
            _dataPointer->UV = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X + uvRect.Width, uvRect.Y + uvRect.Height), textureMatrix);
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(location.X + size.X, location.Y + size.Y, location.Z), vertexMatrix);
            _dataPointer->UV = texture == null ? Vector2.Zero : Vector2.TransformPosition(new Vector2(uvRect.X + uvRect.Width, uvRect.Y), textureMatrix);
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            _dataPointer->Vertex = Vector3.TransformPosition(new Vector3(location.X, location.Y + size.Y, location.Z),vertexMatrix);
            _dataPointer->UV = texture == null ? Vector2.Zero : Vector2.TransformPosition(uvRect.Location, textureMatrix);
            _dataPointer->Tid = tid;
            _dataPointer->Color = c;
            _dataPointer++;

            // Increment indices count.
            _indicesCount += 6;

            Helpers.CheckError("after add");
        }

        /// <summary>
        /// Finish mapping the buffer, and draw it.
        /// </summary>
        /// <param name="primitiveType">The primitive type to draw the buffer in.</param>
        /// <param name="bufferMatrix">The matrix4 to upload as an uniform for "bufferMatrix". If null nothing will be uploaded.</param>
        /// <param name="shader">The shader to use. If null the current one will be used.</param>
        public void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles, Matrix4? bufferMatrix = null, ShaderProgram shader = null)
        {
            Helpers.CheckError("before flush");

            // Sync shader.
            shader?.Bind();
            if (bufferMatrix != null) ShaderProgram.Current.SetUniformMatrix4("bufferMatrix", (Matrix4)bufferMatrix);
            ShaderProgram.Current.SetUniformFloat("time", _renderer.Context.Time);
            Helpers.CheckError("map buffer - shader preparation");

            // Bind textures.
            for (int i = 0; i < _textureList.Count; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                _textureList[i].Bind();
            }
            Helpers.CheckError("map buffer - texture binding");

            // Draw indices.
            VBO.Bind();
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            Helpers.CheckError("map buffer - map end");

            VAO.Bind();
            IBO.Bind();
            Helpers.CheckError("map buffer - bind");

            GL.DrawElements(primitiveType, _indicesCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
            Helpers.CheckError("map buffer - draw");

            IBO.Unbind();
            VAO.Unbind();
            Helpers.CheckError("map buffer - unbind");

            // Reset count.
            _indicesCount = 0;
            _dataPointer = null;

            Helpers.CheckError("flush");
        }

        /// <summary>
        /// Destroy the map buffer freeing resources.
        /// </summary>
        public void Delete()
        {
            VBO?.Delete();
            IBO?.Delete();
            VAO?.Delete();
        }
    }
}