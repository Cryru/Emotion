#nullable enable

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
	public unsafe class RenderStreamBatch
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
		protected TextureAtlas? _atlas;
		protected TextureAtlas? _smoothAtlas;

		#endregion

		#region Mapping Memory

		protected IntPtr _backingBuffer;
		protected uint _backingBufferSize;
		protected uint _backingBufferOffset;

		protected uint _backingSizeLeft
		{
			get => _backingBufferSize - _backingBufferOffset;
		}

		protected ushort _vertexIndex;

		protected IntPtr _backingIndexBuffer;
		protected uint _backingIndexBufferSize;
		protected uint _backingIndexOffset;

		protected uint _backingIndexSizeLeft
		{
			get => _backingIndexBufferSize - _backingIndexOffset;
		}

		#endregion

		#region GL Objects

		// todo: these could be shared by render batches that share vertex layout.
		// implying that there would be more than one batch lol
		protected class GLRenderObjects
		{
			public VertexBuffer VBO;
			public IndexBuffer IBO;
			public VertexArrayObject VAO;

			public GLRenderObjects(VertexBuffer vbo, IndexBuffer ibo, VertexArrayObject vao)
			{
				VBO = vbo;
				IBO = ibo;
				VAO = vao;
			}
		}

		protected Dictionary<Type, Stack<GLRenderObjects>> _renderObjects = new();
		protected Dictionary<Type, Stack<GLRenderObjects>> _renderObjectsUsed = new();

		#endregion

		public RenderStreamBatch(bool useAtlas = true)
		{
			_indexByteSize = sizeof(ushort);

			_structCapacity = ushort.MaxValue / 3;

			const int defaultStructSize = 32;
			uint bufferSizeBytes = defaultStructSize * ushort.MaxValue;

			_backingBufferSize = bufferSizeBytes;
			_backingBuffer = UnmanagedMemoryAllocator.MemAlloc((int) _backingBufferSize);
			_backingIndexBufferSize = _indexByteSize * _structCapacity * 6;
			_backingIndexBuffer = UnmanagedMemoryAllocator.MemAlloc((int) _backingIndexBufferSize);

			// Arbitrary chosen number.
			// The number of objects we'll need can vary greatly, and there is no
			// reliable way to determine how many the driver can create.
			// Godspeed.
			for (var i = 0; i < 16; i++)
			{
				CreateRenderObject(typeof(VertexData));
			}

			for (var i = 0; i < _batchableLengths.Length; i++)
			{
				_batchableLengths[i] = new int[1];
			}

			if (!useAtlas) return;
			_atlas = new TextureAtlas();
			_smoothAtlas = new TextureAtlas(true);
			_currentTexture = _atlas.AtlasPointer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StreamData<VertexData> GetStreamMemory(uint structCount, uint indexCount, BatchMode batchMode, Texture? texture = null)
		{
			return GetStreamMemory<VertexData>(structCount, indexCount, batchMode, texture);
		}

		public StreamData<T> GetStreamMemory<T>(uint structCount, uint indexCount, BatchMode batchMode, Texture? texture = null) where T : struct
		{
			Type type = typeof(T);
			uint vertexTypeByteSize = _currentVertexTypeByteSize;
			if (CurrentVertexType != type) vertexTypeByteSize = (uint) Marshal.SizeOf<T>();

			uint vBytesNeeded = structCount * vertexTypeByteSize;
			uint iBytesNeeded = indexCount * _indexByteSize;

			// This request can never be serviced, as it itself is larger that an empty buffer.
			if (vBytesNeeded > _backingBufferSize || iBytesNeeded > _backingIndexBufferSize) return default;

			if (CurrentVertexType != type)
			{
				if (AnythingMapped) FlushRender();
				EnsureRenderObjectsOfType(type);
				_currentVertexTypeByteSize = vertexTypeByteSize;
				CurrentVertexType = type;
			}

			// Check if the request can be served, if not - flush the buffers.
			bool gotStructs = _backingSizeLeft >= vBytesNeeded && _vertexIndex + structCount + indexCount <= ushort.MaxValue;
			bool gotIndices = _backingIndexSizeLeft >= iBytesNeeded;
			if (!gotStructs || !gotIndices)
			{
				if (AnythingMapped) FlushRender();

				// Should have enough size now.
				Assert(_backingSizeLeft >= vBytesNeeded);
				Assert(_backingIndexSizeLeft >= iBytesNeeded);
			}

			// Batch type logic.
			if (batchMode != BatchMode && AnythingMapped) FlushRender();
			BatchMode = batchMode;

			texture ??= Texture.EmptyWhiteTexture;
			uint texturePointer = texture.Pointer;

			// Texture atlas logic
			bool batchedTexture = _atlas != null;
			if (batchedTexture)
			{
				AssertNotNull(_atlas);
				AssertNotNull(_smoothAtlas);

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

			// Mark memory area as used.
			uint vOffset = _backingBufferOffset;
			uint iOffset = _backingIndexOffset;
			_backingBufferOffset += vBytesNeeded;
			_backingIndexOffset += iBytesNeeded;

			// ReSharper disable PossibleNullReferenceException
			Assert(_backingBuffer != IntPtr.Zero);
			Assert(_backingIndexBuffer != IntPtr.Zero);
			var verticesData = new Span<T>(&((byte*) _backingBuffer)[vOffset], (int) structCount);
			var indicesData = new Span<ushort>(&((byte*) _backingIndexBuffer)[iOffset], (int) indexCount);
			// ReSharper enable PossibleNullReferenceException

			ushort index = _vertexIndex;
			_vertexIndex = (ushort) (_vertexIndex + structCount);

			// Some batch modes cannot be used with draw elements.
			// For them record where the memory request started and its length.
			// 1 request = 1 mesh
			if (BatchMode == BatchMode.TriangleFan)
			{
				// Not enough size - extend.
				if (_batchableLengthUtilization + 1 >= _batchableLengths[0].Length)
					for (var i = 0; i < _batchableLengths.Length; i++)
					{
						Array.Resize(ref _batchableLengths[i], _batchableLengths[i].Length * 2);
					}

				_batchableLengths[0][_batchableLengthUtilization] = index;
				_batchableLengths[1][_batchableLengthUtilization] = (int) indexCount;
				_batchableLengthUtilization++;
			}

			// If using the texture atlas, record the mapping.
			// This needs to happen after any potential flushes.
			if (batchedTexture)
			{
				AssertNotNull(_atlas);
				AssertNotNull(_smoothAtlas);

				var structsMappedInCurrentMapping = (int) (vOffset / vertexTypeByteSize);
				var upToStructNow = (int) (structsMappedInCurrentMapping + structCount);
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
			Assert(AnythingMapped);

			uint mappedBytes = _backingBufferOffset;
			uint mappedBytesIndices = _backingIndexOffset;

			GLRenderObjects? renderObj = GetFirstFreeRenderObject(CurrentVertexType);
			if (renderObj == null) // Impossible!
			{
				Assert(false, "RenderStream had no render object to flush with.");
				return;
			}

			var structByteSize = (uint) renderObj.VAO.ByteSize;

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
				var count = (int) (mappedBytesIndices / _indexByteSize);
				Gl.DrawElements(primitiveType, count, DrawElementsType.UnsignedShort, IntPtr.Zero);
			}

			PerfProfiler.FrameEventEnd("Stream Render");

			_vertexIndex = 0;
			_backingBufferOffset = 0;
			_backingIndexOffset = 0;

			// Reset texture mapping, if using it.
			_atlas?.ResetMapping();
			_smoothAtlas?.ResetMapping();
		}

		public void DoTasks(RenderComposer c)
		{
			_atlas?.Update(c);
			_smoothAtlas?.Update(c);

			// Funnel used objects back into the usable pool.
			foreach (KeyValuePair<Type, Stack<GLRenderObjects>> usedStackPair in _renderObjectsUsed)
			{
				Stack<GLRenderObjects> usableStack = _renderObjects[usedStackPair.Key];
				Stack<GLRenderObjects> usedStack = usedStackPair.Value;
				while (usedStack.Count > 0) usableStack.Push(usedStack.Pop());
			}
		}

		#region GL Render Objects

		protected GLRenderObjects CreateRenderObject(Type vertexType)
		{
			var vbo = new VertexBuffer(0, BufferUsage.StreamDraw);
			var ibo = new IndexBuffer(0, BufferUsage.StreamDraw);
			var vao = new VertexArrayObjectTypeArg(vertexType, vbo, ibo);

			var objectsPair = new GLRenderObjects(vbo, ibo, vao);

			if (!_renderObjects.TryGetValue(vertexType, out Stack<GLRenderObjects>? renderObjsOfType))
			{
				renderObjsOfType = new Stack<GLRenderObjects>();
				_renderObjects.Add(vertexType, renderObjsOfType);

				var usedStack = new Stack<GLRenderObjects>();
				_renderObjectsUsed.Add(vertexType, usedStack);
			}

			renderObjsOfType.Push(objectsPair);
			return objectsPair;
		}

		protected void EnsureRenderObjectsOfType(Type type)
		{
			if (!_renderObjects.ContainsKey(type)) CreateRenderObject(type);
		}

		// According to the Khronos OpenGL reference, buffers cannot be uploaded to if any rendering
		// in the pipeline references the buffer object, regardless of whether it concerns the region
		// being updated. Nvidia and AMD gracefully handle this, but Intel GPUs will silently fail to
		// render some triangles.
		//
		// Because of this we cannot reuse buffers in the same frame, regardless of how small of a draw there is,
		// and fencing is a lot slower than just allocating a bunch.
		protected GLRenderObjects? GetFirstFreeRenderObject(Type vertexType)
		{
			if (!_renderObjects.TryGetValue(vertexType, out Stack<GLRenderObjects>? renderObjsOfType))
			{
				Assert(false, $"No render object of vertex type {vertexType.Name}");
				return null;
			}

			if (renderObjsOfType.Count == 0) CreateRenderObject(vertexType);

			if (renderObjsOfType.Count > 0)
			{
				GLRenderObjects obj = renderObjsOfType.Pop();
				Stack<GLRenderObjects> usedStack = _renderObjectsUsed[vertexType];
				usedStack.Push(obj);
				return obj;
			}

			Assert(false, "No free render object for stream!?");
			return null;
		}

		#endregion

		#region Helpers and Overloads

		/// <summary>
		/// Get stream memory and automatically map indices depending on the batch mode.
		/// </summary>
		public Span<T> GetStreamMemory<T>(uint structCount, BatchMode batchMode, Texture? texture = null) where T : struct
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
						indicesSpan[i] = (ushort) (offset + i);
					}

					break;
			}

			return streamData.VerticesData;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<VertexData> GetStreamMemory(uint structCount, BatchMode batchMode, Texture? texture = null)
		{
			return GetStreamMemory<VertexData>(structCount, batchMode, texture);
		}

		#endregion
	}
}