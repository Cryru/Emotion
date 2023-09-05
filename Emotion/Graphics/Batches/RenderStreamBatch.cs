#region Using

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    public unsafe partial class RenderStreamBatch
	{
		protected uint _structCapacity;
		protected uint _indexByteSize;

		/// <summary>
		/// The current batch mode of the stream. This determines the primitives drawing mode.
		/// </summary>
		public BatchMode BatchMode { get; protected set; } = BatchMode.Quad;
		
		/// <summary>
		/// The data type of the vertices currently being mapped.
		/// </summary>
		public Type CurrentVertexType { get; protected set; } = typeof(VertexData);

        // Performance cache for GetStreamMemory
        private uint _currentVertexTypeByteSize = (uint) VertexData.SizeInBytes;

		/// <summary>
		/// Whether anything is mapped to the stream.
		/// </summary>
		public bool AnythingMapped
		{
			get => _backingBufferOffset != 0;
		}

		#region MultiDraw

		protected int[][] _batchableLengths = new int[2][];
		protected int _batchableLengthUtilization;

		#endregion

		#region Texturing

		protected uint _currentTexture;
		protected TextureAtlas _atlas;
		protected TextureAtlas _smoothAtlas;

		#endregion

		#region Mapping Memory

		protected IntPtr _backingBuffer;
		protected uint _backingBufferSize;
		protected uint _backingBufferOffset;

        protected ushort _vertexIndex;

        protected uint BackingSizeLeft
        {
            get => _backingBufferSize - _backingBufferOffset;
        }

        protected IntPtr _backingIndexBuffer;
		protected uint _backingIndexBufferSize;
		protected uint _backingIndexOffset;
        protected uint BackingIndexSizeLeft
        {
            get => _backingIndexBufferSize - _backingIndexOffset;
        }

		#endregion

		#region GL Objects

		// todo: these could be shared by render batches that share vertex layout.
		protected class GLRenderObjects
		{
			public VertexBuffer VBO;
			public IndexBuffer IBO;

			public VertexArrayObject VAO;

			public uint FrameUsed;
        }

		protected Dictionary<Type, List<GLRenderObjects>> _renderObjects = new Dictionary<Type, List<GLRenderObjects>>();

        #endregion

        public RenderStreamBatch(bool useAtlas = true)
		{
			_indexByteSize = sizeof(ushort);

			_structCapacity = ushort.MaxValue / 3;

			const int defaultStructSize = 32;
            uint bufferSizeBytes = defaultStructSize * _structCapacity;

			_backingBufferSize = bufferSizeBytes;
			_backingBuffer = UnmanagedMemoryAllocator.MemAlloc((int)_backingBufferSize);
            _backingIndexBufferSize = _indexByteSize * _structCapacity * 3;
            _backingIndexBuffer = UnmanagedMemoryAllocator.MemAlloc((int)_backingIndexBufferSize);

			// Arbitrary chosen number.
			// The number of objects we'll need can vary greatly, and there is no
			// reliable way to determine how many the driver can create.
			// Godspeed.
			for (int i = 0; i < 16; i++)
			{
				CreateRenderObject(typeof(VertexData));
			}

			if (!useAtlas) return;
			_atlas = new TextureAtlas();
			_smoothAtlas = new TextureAtlas(true);
			_currentTexture = _atlas.AtlasPointer;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StreamData<VertexData> GetStreamMemory(uint structCount, uint indexCount, BatchMode batchMode, Texture texture = null)
        {
			return GetStreamMemory<VertexData>(structCount, indexCount, batchMode, texture);
		}

        public StreamData<T> GetStreamMemory<T>(uint structCount, uint indexCount, BatchMode batchMode, Texture texture = null) where T : struct
        {
			Type type = typeof(T);
			if (CurrentVertexType != type)
			{
				if (AnythingMapped) FlushRender();
				EnsureRenderObjectsOfType(type);
                _currentVertexTypeByteSize = (uint)Marshal.SizeOf<T>();
            }
			CurrentVertexType = type;
			uint typeByteSize = _currentVertexTypeByteSize;

            if (batchMode != BatchMode && AnythingMapped) FlushRender();
			BatchMode = batchMode;

			texture ??= Texture.EmptyWhiteTexture;
			uint texturePointer = texture.Pointer;

			// Texture atlas logic
			bool batchedTexture = _atlas != null;
			if (batchedTexture)
			{
				if (texture.Smooth && _smoothAtlas.TryBatchTexture(texture))
					texturePointer = _smoothAtlas.AtlasPointer;
				else if (!texture.Smooth && _atlas.TryBatchTexture(texture))
					texturePointer = _atlas.AtlasPointer;
			}

			// If the texture is changing, flush old data.
			if (texturePointer != _currentTexture)
			{
				if (AnythingMapped) FlushRender();
				_currentTexture = texturePointer;
			}

			uint vBytesNeeded = structCount * typeByteSize;
			uint iBytesNeeded = indexCount * _indexByteSize;

			// This request can never be serviced, as it itself is larger that an empty buffer.
			if (vBytesNeeded > _backingBufferSize || iBytesNeeded > _backingIndexBufferSize) return default;

			// Check if the request can be served, if not - flush the buffers.
			bool gotStructs = BackingSizeLeft >= vBytesNeeded && _vertexIndex + structCount + indexCount <= ushort.MaxValue;
			bool gotIndices = BackingIndexSizeLeft >= iBytesNeeded;
			if (!gotStructs || !gotIndices)
			{
				if (AnythingMapped) FlushRender();

				// Should have enough size now.
				Debug.Assert(BackingSizeLeft >= vBytesNeeded);
				Debug.Assert(BackingIndexSizeLeft >= iBytesNeeded);
			}

			// Mark memory area as used.
			uint vOffset = _backingBufferOffset;
			uint iOffset = _backingIndexOffset;
			_backingBufferOffset += vBytesNeeded;
			_backingIndexOffset += iBytesNeeded;

            // ReSharper disable PossibleNullReferenceException
            Debug.Assert(_backingBuffer != IntPtr.Zero);
            Debug.Assert(_backingIndexBuffer != IntPtr.Zero);
            var verticesData = new Span<T>(&((byte*)_backingBuffer)[vOffset], (int)structCount);
			var indicesData = new Span<ushort>(&((byte*)_backingIndexBuffer)[iOffset], (int)indexCount);
			// ReSharper enable PossibleNullReferenceException

			ushort index = _vertexIndex;
			_vertexIndex = (ushort)(_vertexIndex + structCount);

			// Some batch modes cannot be used with draw elements.
			// For them record where the memory request started and its length.
			// 1 request = 1 mesh
			if (BatchMode == BatchMode.TriangleFan)
			{
				// Not enough size.
				if (_batchableLengthUtilization + 1 >= (_batchableLengths[0]?.Length ?? 0))
					for (var i = 0; i < _batchableLengths.Length; i++)
					{
						_batchableLengths[i] ??= new int[1];
						Array.Resize(ref _batchableLengths[i], _batchableLengths[i].Length * 2);
					}

				_batchableLengths[0][_batchableLengthUtilization] = index;
				_batchableLengths[1][_batchableLengthUtilization] = (int)indexCount;
				_batchableLengthUtilization++;
			}

			// If using the texture atlas, record the mapping.
			// This needs to happen after any potential flushes.
			if (batchedTexture)
			{
				var structsMappedInCurrentMapping = (int)(vOffset / typeByteSize);
				var upToStructNow = (int)(structsMappedInCurrentMapping + structCount);
				if (_currentTexture == _atlas.AtlasPointer) _atlas.RecordTextureMapping(texture, upToStructNow);
				else if (_currentTexture == _smoothAtlas.AtlasPointer) _smoothAtlas.RecordTextureMapping(texture, upToStructNow);
			}

			return new StreamData<T>
			{
				VerticesData = verticesData,
				IndicesData = indicesData,
				StructIndex = index
			};
		}

		public void FlushRender()
		{
			Debug.Assert(AnythingMapped);

			uint mappedBytes = _backingBufferOffset;
			uint mappedBytesIndices = _backingIndexOffset;

            var renderObj = GetFirstFreeRenderObject(CurrentVertexType);
			uint structByteSize = (uint) renderObj.VAO.ByteSize;

            PerfProfiler.FrameEventStart("Stream Render", $"{mappedBytes / structByteSize} Vertices with {mappedBytesIndices / _indexByteSize} Indices");

			// Remap UVs to be within the atlas, if using the atlas.
			if (_currentTexture == _atlas?.AtlasPointer)
				_atlas.RemapBatchUVs(_backingBuffer, mappedBytes, structByteSize, renderObj.VAO.UVByteOffset);
			else if (_currentTexture == _smoothAtlas?.AtlasPointer)
				_smoothAtlas.RemapBatchUVs(_backingBuffer, mappedBytes, structByteSize, renderObj.VAO.UVByteOffset);

			// Avoid allocations
			if (renderObj.VBO.Size < mappedBytes)
				renderObj.VBO.Upload(_backingBuffer, mappedBytes);
			else
				renderObj.VBO.UploadPartial(_backingBuffer, mappedBytes);

            if (renderObj.IBO.Size < mappedBytesIndices)
                renderObj.IBO.Upload(_backingIndexBuffer, mappedBytesIndices);
            else
                renderObj.IBO.UploadPartial(_backingIndexBuffer, mappedBytesIndices);

            // Bind GL state.
            Texture.EnsureBound(_currentTexture);
			VertexArrayObject.EnsureBound(renderObj.VAO);
			IndexBuffer.EnsureBound(renderObj.IBO.Pointer);

			// Draw with the appropriate state.
			PrimitiveType primitiveType = BatchMode switch
			{
				BatchMode.Quad => PrimitiveType.Triangles,
				BatchMode.SequentialTriangles => PrimitiveType.Triangles,
				BatchMode.TriangleFan => PrimitiveType.TriangleFan,
				_ => PrimitiveType.Triangles
			};
			if (BatchMode == BatchMode.TriangleFan)
			{
				if (Gl.CurrentVersion.GLES)
					for (var i = 0; i < _batchableLengthUtilization; i++)
					{
						Gl.DrawArrays(primitiveType, _batchableLengths[0][i], _batchableLengths[1][i]);
					}
				else
					Gl.MultiDrawArrays(primitiveType, _batchableLengths[0], _batchableLengths[1], _batchableLengthUtilization);

				_batchableLengthUtilization = 0;
			}
			else
			{
				var count = (int)(mappedBytesIndices / _indexByteSize);
				Gl.DrawElements(primitiveType, count, DrawElementsType.UnsignedShort, IntPtr.Zero);
			}

			PerfProfiler.FrameEventEnd("Stream Render");

            _vertexIndex = 0;
            _backingBufferOffset = 0;
            _backingIndexOffset = 0;

			// This render obj will be reused only in the next frame,
			// to ensure synchronization and not having to fence.
			//
			// todo: figure out how this interfaces with double buffering,
			// is it possible for the object to still be in use?
			renderObj.FrameUsed = Engine.FrameCount;

            // Reset texture mapping, if using it.
            if (_atlas == null) return;
			_atlas.ResetMapping();
			_smoothAtlas.ResetMapping();

			// According to the Khronos OpenGL reference, buffers cannot be uploaded to if any rendering
			// in the pipeline references the buffer object, regardless of whether it concerns the region
			// being updated. Nvidia and AMD gracefully handle this, but Intel GPUs will silently fail to
			// render some triangles.
			//
			// Because of this we swap buffers after every render, regardless of how small it is.
			// This sucks and makes the FencedBufferSource more or less useless and a more general solution
			// should be implemented. For now this is the bandaid.
        }

		public void DoTasks(RenderComposer c)
		{
			_atlas?.Update(c);
			_smoothAtlas?.Update(c);
		}

        #region GL Render Objects

		protected GLRenderObjects CreateRenderObject(Type vertexType)
		{
            var vbo = new VertexBuffer(0, BufferUsage.StreamDraw);
            var ibo = new IndexBuffer(0, BufferUsage.StreamDraw);
            var vao = new VertexArrayObjectTypeArg(vertexType, vbo, ibo);

            GLRenderObjects objectsPair = new GLRenderObjects()
			{
				VBO = vbo,
				VAO = vao,
				IBO = ibo,
			};

            List<GLRenderObjects> renderObjsOfType;
            if (!_renderObjects.TryGetValue(vertexType, out renderObjsOfType))
			{
				renderObjsOfType = new List<GLRenderObjects>();
				_renderObjects.Add(vertexType, renderObjsOfType);
			}
            renderObjsOfType.Add(objectsPair);
			return objectsPair;
        }

		protected void EnsureRenderObjectsOfType(Type type)
		{
            if (!_renderObjects.ContainsKey(type))
            {
				CreateRenderObject(type);
            }
        }

        protected GLRenderObjects GetFirstFreeRenderObject(Type vertexType)
        {
            List<GLRenderObjects> renderObjsOfType;
            if (!_renderObjects.TryGetValue(vertexType, out renderObjsOfType))
            {
				Assert(false, $"No render object of vertex type {vertexType.Name}");
				return null;
            }

            for (int i = 0; i < renderObjsOfType.Count; i++)
			{
				var obj = renderObjsOfType[i];
				if (obj.FrameUsed == Engine.FrameCount) continue;
				return obj;
            }

			return CreateRenderObject(vertexType);
		}

		#endregion

		#region Helpers and Overloads

        /// <summary>
        /// Get stream memory and automatically map indices depending on the batch mode.
        /// </summary>
        public Span<T> GetStreamMemory<T>(uint structCount, BatchMode batchMode, Texture texture = null) where T : struct
		{
			uint indexCount = 0;
			switch (batchMode)
			{
				case BatchMode.Quad:
					indexCount = structCount / 4 * 6;
					break;
				case BatchMode.TriangleFan:
				case BatchMode.SequentialTriangles:
					indexCount = structCount;
					break;
			}

			StreamData<T> streamData = GetStreamMemory<T>(structCount, indexCount, batchMode, texture);
			Span<ushort> indicesSpan = streamData.IndicesData;
			ushort offset = streamData.StructIndex;
			switch (batchMode)
			{
				case BatchMode.Quad:
					IndexBuffer.FillQuadIndices(indicesSpan, offset);
					break;
				case BatchMode.TriangleFan:
				case BatchMode.SequentialTriangles:
					for (ushort i = 0; i < indicesSpan.Length; i++)
					{
						indicesSpan[i] = (ushort)(offset + i);
					}

					break;
			}

			return streamData.VerticesData;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<VertexData> GetStreamMemory(uint structCount, BatchMode batchMode, Texture texture = null)
        {
            return GetStreamMemory<VertexData>(structCount, batchMode, texture);
        }

        #endregion
    }
}