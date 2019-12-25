#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    /// <summary>
    /// Batch which handles batching sprites to be drawn together.
    /// Up to 16 textures and around 16k sprites can be batched at once.
    /// </summary>
    public class SpriteBatch : RecyclableCommand
    {
        /// <summary>
        /// Whether the batch is full.
        /// This happens when the maximum indices are met, or the maximum textures.
        /// </summary>
        public bool Full { get; protected set; }

        #region Texturing

        /// <summary>
        /// The textures to bind.
        /// </summary>
        protected uint[] _textureBinding = new uint[Texture.Bound.Length];

        /// <summary>
        /// How many texture slots are utilized.
        /// </summary>
        protected int _textureSlotUtilization;

        #endregion

        #region Memory Management

        /// <summary>
        /// Pointer to the unmanaged batch memory.
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

        #endregion

        #region Draw Params

        /// <summary>
        /// The index to start rendering from.
        /// </summary>
        protected uint _startIndex;

        /// <summary>
        /// The index to end rendering at.
        /// </summary>
        protected uint? _endIndex;

        #endregion

        #region Own Graphics Memory

        /// <summary>
        /// Whether to create gl objects if missing.
        /// </summary>
        private bool _createGl;

        /// <summary>
        /// Whether data needs to be uploaded.
        /// </summary>
        protected bool _upload = true;

        protected VertexBuffer _vbo;
        protected VertexArrayObject _vao;

        #endregion

        #region Vertex Type

        private int _structByteSize;
        private Type _structType;

        #endregion

        /// <summary>
        /// Default constructor for the recycler factory.
        /// </summary>
        public SpriteBatch() : this(false)
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
        public SpriteBatch(bool ownGraphicsMemory = false)
        {
            _structType = typeof(VertexData);
            _structByteSize = Marshal.SizeOf<VertexData>();

            _size = 400; // Initial size is 100 sprites.
            _batchedVertices = Marshal.AllocHGlobal(_size * _structByteSize);
            _createGl = ownGraphicsMemory;
        }

        /// <summary>
        /// Recycle the batch, clearing all batched sprites and textures.
        /// Graphics objects are reused if possible.
        /// </summary>
        public override void Recycle()
        {
            Full = false;
            _textureSlotUtilization = 0;
            _mappedTo = 0;
            _startIndex = 0;
            _endIndex = null;
            _upload = true;
        }

        /// <summary>
        /// Returns the data within the batch to map a sprite into, and adds the provided texture to the texture mapping.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        /// <returns>he data inside the batch to be filled.</returns>
        public virtual unsafe Span<VertexData> GetData(Texture texture)
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
                _batchedVertices = Marshal.ReAllocHGlobal(_batchedVertices, (IntPtr) (resizeAmount * _structByteSize));
                _size = resizeAmount;
            }

            // Get the data.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<VertexData>((void*) &((byte*) _batchedVertices)[_mappedTo * _structByteSize], 4);
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
                composer.VaoCache.TryGetValue(_structType, out vao);
                if (vao == null)
                {
                    vao = new VertexArrayObject<VertexData>(vbo);
                    composer.VaoCache.Add(_structType, vao);
                }
            }

            // Upload the data if needed.
            if (_upload)
            {
                vbo.Upload(_batchedVertices, (uint) (_mappedTo * _structByteSize));
                _upload = false;
            }

            // Bind graphics objects.
            VertexArrayObject.EnsureBound(vao);
            if (vao.IBO == null) IndexBuffer.EnsureBound(IndexBuffer.QuadIbo.Pointer);
            BindTextures();

            // Render specified range.
            var startIndex = (IntPtr) (_startIndex * sizeof(ushort));
            uint length = _endIndex - _startIndex ?? (uint) (_mappedTo / 4) * 6 - _startIndex;
            Debug.Assert((uint) _mappedTo <= Engine.Renderer.MaxIndices);
            Debug.Assert((uint) startIndex + length <= Engine.Renderer.MaxIndices * 6);
            Gl.DrawElements(PrimitiveType.Triangles, (int) length, DrawElementsType.UnsignedShort, startIndex);
        }

        #region Texturing API

        /// <summary>
        /// Add the specified pointer to the texture binding, if possible.
        /// </summary>
        /// <param name="pointer">The texture pointer to add.</param>
        /// <param name="bindingPointer">The pointer to the internal binding.</param>
        /// <returns>Whether the texture was added.</returns>
        public bool AddTextureBinding(uint pointer, out int bindingPointer)
        {
            bindingPointer = -1;

            if (_textureSlotUtilization == _textureBinding.Length) return false;

            if (TextureInBinding(pointer, out bindingPointer)) return true;

            // Add to binding.
            _textureBinding[_textureSlotUtilization] = pointer;
            bindingPointer = _textureSlotUtilization;
            _textureSlotUtilization++;

            // Verify if the texture binding maximum has been reached.
            if (_textureSlotUtilization == _textureBinding.Length) Full = true;

            return true;
        }

        /// <summary>
        /// Check whether the specified texture pointer exists in the batch binding.
        /// </summary>
        /// <param name="pointer">The texture point to check.</param>
        /// <param name="bindingPointer">The internal pointer bound at or -1 if not bound.</param>
        /// <returns>Whether this texture exists in the batch binding.</returns>
        public bool TextureInBinding(uint pointer, out int bindingPointer)
        {
            for (var i = 0; i < _textureSlotUtilization; i++)
            {
                if (_textureBinding[i] != pointer) continue;
                bindingPointer = i;
                return true;
            }

            bindingPointer = -1;
            return false;
        }

        /// <summary>
        /// Bind the textures in use.
        /// </summary>
        public void BindTextures()
        {
            Debug.Assert(GLThread.IsGLThread());

            for (uint i = 0; i < _textureSlotUtilization; i++)
            {
                Texture.EnsureBound(_textureBinding[i], i);
            }
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

        #endregion

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