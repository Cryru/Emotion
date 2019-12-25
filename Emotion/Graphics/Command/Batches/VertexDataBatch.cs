#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    /// <summary>
    /// An immediate mode like batch. Uses the VAO and VBO of the RenderComposer it is under, and
    /// works with the VertexData struct.
    /// </summary>
    public class VertexDataBatch : QuadBatch
    {
        /// <summary>
        /// Pointer to the batch memory.
        /// </summary>
        protected IntPtr _batchedVertices;

        /// <summary>
        /// The size of the memory in vertex data.
        /// </summary>
        protected int _size;

        /// <summary>
        /// The number of vertices in use from _batchedVertices.
        /// </summary>
        protected int _mappedTo;

        /// <summary>
        /// The index to start rendering from.
        /// </summary>
        protected uint _startIndex;

        /// <summary>
        /// The index to end rendering at.
        /// </summary>
        protected uint? _endIndex;

        /// <summary>
        /// Whether data needs to be uploaded.
        /// </summary>
        protected bool _upload = true;

        #region Own Memory

        /// <summary>
        /// Whether to create gl objects if missing.
        /// </summary>
        private bool _createGl;

        private VertexBuffer _vbo;
        private VertexArrayObject _vao;

        #endregion

        /// <summary>
        /// Default constructor for the recycler factory.
        /// </summary>
        public VertexDataBatch() : this(false)
        {
        }

        /// <summary>
        /// Create a new batch for sprites.
        /// </summary>
        /// <param name="ownGraphicsMemory">
        /// Whether to use own graphics memory.
        /// If you are caching a render it is better to set this to true as it will prevent the data from being reuploaded.
        /// If set to false the VBO of the RenderComposer is used instead.
        /// </param>
        public VertexDataBatch(bool ownGraphicsMemory = false)
        {
            _size = 400; // Initial size is 100 sprites.
            _batchedVertices = Marshal.AllocHGlobal(_size * VertexData.SizeInBytes);
            _createGl = ownGraphicsMemory;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _mappedTo = 0;
            _startIndex = 0;
            _endIndex = null;
            _upload = true;
        }

        /// <inheritdoc />
        public override unsafe Span<VertexData> GetData(Texture texture)
        {
            // Check if already full.
            if (Full) return null;

            // Check if the texture exists in the binding for this batch.
            // This will also add the texture to this batch.
            // If there is no space the fullness check above will be true - fullness checks are always in advance.
            int texturePointer = -1;
            if (texture != null) AddTextureBinding(texture.Pointer, out texturePointer);

            // Check if enough data.
            if (_mappedTo == _size || _mappedTo + 4 > _size)
            {
                // When resizing, go for a buffer twice as big as what's needed.
                // The "fullness" check in PushSprite will ensure that the needed data isn't larger than what the default IBOs can batch at once.
                var resizeAmount = (int) Math.Min(_size * 2, Engine.Renderer.MaxIndices);
                _batchedVertices = Marshal.ReAllocHGlobal(_batchedVertices, (IntPtr) (resizeAmount * VertexData.SizeInBytes));
                _size = resizeAmount;
            }

            // Get the data.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<VertexData>((void*) &((VertexData*) _batchedVertices)[_mappedTo], 4);
            _mappedTo += 4;

            // Set the Tid.
            for (var i = 0; i < data.Length; i++)
            {
                data[i].Tid = texturePointer;
            }

            // Check if one more sprite can fit. Each sprite is 4 vertices.
            if (_mappedTo + 4 > Engine.Renderer.MaxIndices) Full = true;

            // New data will need to be uploaded.
            _upload = true;

            return data;
        }

        /// <inheritdoc />
        public override void Execute(RenderComposer composer)
        {
            if (_mappedTo == 0) return;

            // Get the right graphics objects to use for the drawing.
            VertexBuffer vbo;
            VertexArrayObject vao;
            if (_createGl)
            {
                if (_vbo == null)
                {
                    _vbo = new VertexBuffer();
                    _vao = new VertexArrayObject<VertexData>(_vbo, IndexBuffer.QuadIbo);
                }

                vbo = _vbo;
                vao = _vao;
            }
            else
            {
                vbo = composer.VertexBuffer;
                vao = composer.CommonVao;
            }

            // Upload the data if needed.
            if (_upload)
            {
                vbo.Upload(_batchedVertices, (uint) (_mappedTo * VertexData.SizeInBytes));
                _upload = false;
            }

            // Bind graphics objects.
            VertexArrayObject.EnsureBound(vao);
            if(vao.IBO == null) IndexBuffer.EnsureBound(IndexBuffer.QuadIbo.Pointer);
            BindTextures();

            // Render specified range.
            var startIndex = (IntPtr) (_startIndex * sizeof(ushort));
            uint length = _endIndex - _startIndex ?? (uint) (_mappedTo / 4) * 6 - _startIndex;
            Debug.Assert((uint) _mappedTo <= Engine.Renderer.MaxIndices);
            Debug.Assert((uint) startIndex + length <= Engine.Renderer.MaxIndices * 6);
            Gl.DrawElements(PrimitiveType.Triangles, (int) length, DrawElementsType.UnsignedShort, startIndex);
        }

        #region API for Outside Composer

        /// <summary>
        /// Get the batched sprite at the specific index.
        /// Changes to the returned object will modify the sprite itself.
        /// To change the texture request a binding from AddTextureBinding()
        /// To clear unused texture bindings call RemapTextures().
        /// </summary>
        /// <param name="index">The index of the batched sprite.</param>
        /// <returns>The sprite at that index or null if invalid.</returns>
        public unsafe Span<VertexData> GetSpriteAt(int index)
        {
            if (index < 0 || index * 4 >= _mappedTo) return null;

            // Mark state as dirty.
            _upload = true;

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            return new Span<VertexData>((void*) &((VertexData*) _batchedVertices)[index * 4], 4);
        }

        /// <summary>
        /// Remaps all textures in the current binding, removing unused bindings.
        /// </summary>
        public unsafe void RemapTextures()
        {
            var binding = new uint[_textureBinding.Length];
            Array.Copy(_textureBinding, 0, binding, 0, _textureBinding.Length);

            _textureSlotUtilization = 0;
            var data = new Span<VertexData>((void*) _batchedVertices, _mappedTo);
            for (var i = 0; i < data.Length; i++)
            {
                ref VertexData cur = ref data[i];

                // Check if any texture.
                if (cur.Tid == -1) continue;

                Debug.Assert(cur.Tid >= 0 && cur.Tid < binding.Length);

                // Get texture pointer.
                uint oldBindingPtr = binding[(int) cur.Tid];

                // Find within new binding.
                int newIdx = -1;
                for (var b = 0; b < _textureSlotUtilization; b++)
                {
                    if (_textureBinding[b] == oldBindingPtr) newIdx = b;
                }

                // If not found - add it.
                if (newIdx == -1)
                {
                    _textureBinding[_textureSlotUtilization] = oldBindingPtr;
                    newIdx = _textureSlotUtilization;
                    _textureSlotUtilization++;

                    Debug.Assert(_textureSlotUtilization < _textureBinding.Length);
                }

                // Amend vertex.
                cur.Tid = newIdx;
            }
        }

        /// <summary>
        /// Set the render range for the batch. By default the whole batch is rendered.
        /// </summary>
        /// <param name="start">The sprite index to start rendering from. Inclusive.</param>
        /// <param name="end">The sprite index to stop rendering at. If null will draw to the end. Exclusive.</param>
        public void SetRenderRange(uint start, uint? end = null)
        {
            // Convert to indices.
            _startIndex = start * 6;
            _endIndex = end * 6;
        }

        #endregion

        public override void Dispose()
        {
            Marshal.FreeHGlobal(_batchedVertices);
        }
    }
}