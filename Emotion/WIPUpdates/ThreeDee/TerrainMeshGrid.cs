#nullable enable

using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Game.OctTree;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Shader;
using Emotion.Graphics.ThreeDee;
using Emotion.Utility;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.Rendering;
using OpenGL;
using TerrainMeshGridChunk = Emotion.WIPUpdates.Grids.VersionedGridChunk<float>;

namespace Emotion.WIPUpdates.ThreeDee;

public class TerrainMeshGrid : ChunkedGrid<float, TerrainMeshGridChunk>, IGridWorldSpaceTiles
{
    public bool Initialized { get; private set; }

    public Vector2 TileSize { get; private set; }

    [DontSerialize]
    public MeshMaterial TerrainMeshMaterial = new MeshMaterial()
    {
        Name = "TerrainChunkMaterial",
        Shader = Engine.AssetLoader.ONE_Get<NewShaderAsset>("Shaders3D/TerrainShader.glsl"),
        State =
        {
            FaceCulling = true,
            FaceCullingBackFace = true,
            ShaderName = "Shaders3D/TerrainShader.glsl"
        }
    };

    public TerrainMeshGrid(Vector2 tileSize, float chunkSize) : base(chunkSize)
    {
        TileSize = tileSize;
    }

    // serialization
    protected TerrainMeshGrid()
    {

    }

    public void Update()
    {
        // Chunk bounds are considered "eventually consistent" with the data.
        // This is to decrease the performance hit of large edits.
        foreach (KeyValuePair<Vector2, TerrainGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            UpdateChunkBounds(item.Key, item.Value);
        }
    }

    #region Rendering

    private List<TerrainGridChunkRuntimeCache> _renderThisPass = new(32);

    private void MarkChunkForRender(Vector2 chunkCoord, TerrainMeshGridChunk chunk)
    {
        if (!_chunkRuntimeData.TryGetValue(chunkCoord, out TerrainGridChunkRuntimeCache? renderCache)) return;

        UpdateChunkVertices(chunkCoord, renderCache);
        Assert(renderCache.VertexMemory.Allocated);

        // todo: deallocate at some point?
        if (renderCache.GPUMemory == null || renderCache.GPUDirty)
        {
            renderCache.GPUMemory ??= GPUMemoryAllocator.AllocateBuffer(renderCache.VertexMemory.Format);
            renderCache.GPUMemory.VBO.Upload(renderCache.VertexMemory);
            renderCache.GPUDirty = false;
        }

        _renderThisPass.Add(renderCache);
    }

    // todo: 3d culling
    public void Render(RenderComposer c, Rectangle clipArea)
    {
        if (ChunkSize != _indexBufferChunkSize || _indexBuffer == null)
            PrepareIndexBuffer();

        // These are static-ish.
        // todo: make them static?
        AssertNotNull(_indexBuffer);
        AssertNotNull(TerrainMeshMaterial);

        Vector2 tileSize = TileSize;
        Vector2 chunkWorldSize = ChunkSize * tileSize;

        Rectangle cacheAreaChunkSpace = clipArea;
        cacheAreaChunkSpace.SnapToGrid(chunkWorldSize);

        cacheAreaChunkSpace.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        //min -= tileSize / 2f;
        //max += tileSize / 2f;

        min /= chunkWorldSize;
        max /= chunkWorldSize;

        min = min.Floor();
        max = max.Ceiling();

        // Pick chunks to render this pass.
        _renderThisPass.Clear();
        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 chunkCoord = new Vector2(x, y);
                TerrainMeshGridChunk? chunk = GetChunk(chunkCoord);
                if (chunk == null) continue;
                MarkChunkForRender(chunkCoord, chunk);
            }
        }

        Vector2 brushWorldSpace = GetEditorBrush();
        if (_renderThisPass.Count > 0)
        {
            Engine.Renderer.FlushRenderStream();

            RenderState oldState = c.CurrentState.Clone();

            Engine.Renderer.SetFaceCulling(true, true);
            NewShaderAsset? asset = TerrainMeshMaterial.Shader?.Get();
            if (asset != null && asset.CompiledShader != null)
                c.SetShader(asset.CompiledShader);

            c.CurrentState.Shader.SetUniformInt("diffuseTexture", 0);
            c.CurrentState.Shader.SetUniformVector2("brushWorldSpace", brushWorldSpace);
            c.CurrentState.Shader.SetUniformFloat("brushRadius", _editorBrushSize);
            Texture.EnsureBound(TerrainMeshMaterial.DiffuseTexture.Pointer);

            foreach (TerrainGridChunkRuntimeCache chunkToRender in _renderThisPass)
            {
                GPUVertexMemory? gpuMem = chunkToRender.GPUMemory;
                if (gpuMem == null) continue;

                VertexArrayObject.EnsureBound(gpuMem.VAO);
                IndexBuffer.EnsureBound(_indexBuffer.Pointer);

                Gl.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
            }

            c.SetState(oldState);
        }

        _octTree.RenderDebug(c);
    }

    #endregion

    #region Chunk Vertices

    private OctTree<TerrainGridChunkRuntimeCache> _octTree = new();
    private Dictionary<Vector2, TerrainGridChunkRuntimeCache> _chunkRuntimeData = new();

    public IEnumerator InitRuntimeDataRoutine()
    {
        Initialized = true;
        foreach (KeyValuePair<Vector2, TerrainMeshGridChunk> item in _chunks)
        {
            OnChunkCreated(item.Key, item.Value);
        }
        
        // Create bounds.
        Update();

        yield break;
    }

    // These events should only really trigger in the editor or if some game is dynamically editing the terrain.

    protected override void OnChunkCreated(Vector2 chunkCoord, TerrainMeshGridChunk newChunk)
    {
        base.OnChunkCreated(chunkCoord, newChunk);
        if (!Initialized) return;

        var renderCache = new TerrainGridChunkRuntimeCache(newChunk);
        _chunkRuntimeData.Add(chunkCoord, renderCache);
    }

    protected override void OnChunkRemoved(Vector2 chunkCoord, TerrainMeshGridChunk newChunk)
    {
        base.OnChunkRemoved(chunkCoord, newChunk);
        if (!Initialized) return;

        // Free resources
        if (_chunkRuntimeData.Remove(chunkCoord, out TerrainGridChunkRuntimeCache? renderCache))
        {
            _octTree.Remove(renderCache);
            VertexDataAllocation.FreeAllocated(ref renderCache.VertexMemory);
            GPUMemoryAllocator.FreeBuffer(renderCache.GPUMemory);
        }
    }

    private void UpdateChunkBounds(Vector2 chunkCoord, TerrainGridChunkRuntimeCache renderCache)
    {
        TerrainMeshGridChunk chunk = renderCache.Chunk;

        if (renderCache.BoundsVersion != chunk.ChunkVersion) return;

        _octTree.Update(renderCache);
        renderCache.BoundsVersion = chunk.ChunkVersion;
    }

    private void UpdateChunkVertices(Vector2 chunkCoord, TerrainGridChunkRuntimeCache renderCache)
    private void UpdateChunkVertices(Vector2 chunkCoord, TerrainGridChunkRuntimeCache renderCache, bool propagate = true)
    {
        TerrainMeshGridChunk chunk = renderCache.Chunk;

        // We already have the latest version of this
        if (renderCache.VerticesGeneratedForVersion == chunk.ChunkVersion && renderCache.VertexMemory.Allocated) return;

        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = TileSize / 2f;
        Vector2 chunkWorldSize = ChunkSize * tileSize;

        VersionedGridChunk<float>? chunkLeft = GetChunk(chunkCoord + new Vector2(-1, 0));
        VersionedGridChunk<float>? chunkTop = GetChunk(chunkCoord + new Vector2(0, -1));
        VersionedGridChunk<float>? chunkDiag = GetChunk(chunkCoord + new Vector2(-1, -1));

        Vector2 chunkWorldOffset = (chunkCoord * chunkWorldSize) + tileSize;

        int vertexCount = (int)(ChunkSize.X * ChunkSize.Y);
        int stichingVertices = (int)(ChunkSize.X + ChunkSize.Y + 1);
        vertexCount += stichingVertices;

        if (!renderCache.VertexMemory.Allocated)
            renderCache.VertexMemory = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal_Color.Format, vertexCount, $"TerrainChunk_{chunkCoord}");
        else if (renderCache.VertexMemory.VertexCount < vertexCount)
            renderCache.VertexMemory = VertexDataAllocation.Reallocate(ref renderCache.VertexMemory, vertexCount);

        var vertices = renderCache.VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal_Color>();

        // Get data for stiching vertices
        float[] dataTop = chunkTop?.GetRawData() ?? Array.Empty<float>();
        float[] dataLeft = chunkLeft?.GetRawData() ?? Array.Empty<float>();
        float[] dataMe = chunk.GetRawData() ?? Array.Empty<float>();

        // Get data around for stitching vertices
        Vector2 leftChunkCoord = chunkCoord + new Vector2(-1, 0);
        VersionedGridChunk<float>? chunkLeft = GetChunk(leftChunkCoord);
        float[] dataLeft = chunkLeft?.GetRawData() ?? Array.Empty<float>();

        Vector2 topChunkCoord = chunkCoord + new Vector2(0, -1);
        VersionedGridChunk<float>? chunkTop = GetChunk(topChunkCoord);
        float[] dataTop = chunkTop?.GetRawData() ?? Array.Empty<float>();

        Vector2 topLeftChunkCoord = chunkCoord + new Vector2(-1, -1);
        VersionedGridChunk<float>? chunkTopLeft = GetChunk(topLeftChunkCoord);
        float[] dataTopLeft = chunkTopLeft?.GetRawData() ?? Array.Empty<float>();

        // Propagate changes to stitching chunks
        if (propagate)
        {
            TerrainGridChunkRuntimeCache? chunkCache;

            Vector2 rightChunkCoord = chunkCoord + new Vector2(1, 0);
            if (_chunkRuntimeData.TryGetValue(rightChunkCoord, out chunkCache))
            {
                chunkCache.VerticesGeneratedForVersion++;
                UpdateChunkVertices(rightChunkCoord, chunkCache, false);
            }

            Vector2 bottomChunkCoord = chunkCoord + new Vector2(0, 1);
            if (_chunkRuntimeData.TryGetValue(bottomChunkCoord, out chunkCache))
            {
                chunkCache.VerticesGeneratedForVersion++;
                UpdateChunkVertices(bottomChunkCoord, chunkCache, false);
            }

            Vector2 bottomRightChunkCoord = chunkCoord + new Vector2(1, 1);
            if (_chunkRuntimeData.TryGetValue(bottomRightChunkCoord, out chunkCache))
            {
                chunkCache.VerticesGeneratedForVersion++;
                UpdateChunkVertices(bottomRightChunkCoord, chunkCache, false);
            }

            // For normals
            if (_chunkRuntimeData.TryGetValue(leftChunkCoord, out chunkCache))
            {
                chunkCache.VerticesGeneratedForVersion++;
                UpdateChunkVertices(leftChunkCoord, chunkCache, false);
            }

            Vector2 bottomLeftCoord = chunkCoord + new Vector2(-1, 1);
            if (_chunkRuntimeData.TryGetValue(bottomLeftCoord, out chunkCache))
            {
                chunkCache.VerticesGeneratedForVersion++;
                UpdateChunkVertices(bottomLeftCoord, chunkCache, false);
            }

            Vector2 topLeftCoord = chunkCoord + new Vector2(-1, -1);
            if (_chunkRuntimeData.TryGetValue(topLeftCoord, out chunkCache))
            {
                chunkCache.VerticesGeneratedForVersion++;
                UpdateChunkVertices(topLeftCoord, chunkCache, false);
            }
        }

        int vIdx = 0;
        for (int y = -1; y < ChunkSize.Y; y++)
        {
            for (int x = -1; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);

                float heightSample = 0;
                if (x == -1 && y == -1)
                {
                    if (chunkDiag != null)
                    {
                        float[] data = chunkDiag.GetRawData();
                        heightSample = data[^1];
                    }
                }
                else if (y == -1)
                {
                    int dataOffset = (int)((ChunkSize.Y - 1) * ChunkSize.X) + (x);
                    heightSample = dataTop.Length == 0 ? 0 : dataTop[dataOffset];
                }
                else if (x == -1)
                {
                    int dataOffset = (int)((y * ChunkSize.X) + ChunkSize.X - 1);
                    heightSample = dataLeft.Length == 0 ? 0 : dataLeft[dataOffset];
                }
                else
                {
                    int dataOffset = GridHelpers.GetCoordinate1DFrom2D(tileCoord, ChunkSize);
                    heightSample = dataMe[dataOffset];
                }

                Vector2 worldPos = chunkWorldOffset + (tileCoord * tileSize);

                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                vData.Normal = Vector3.Zero;
                vData.Position = worldPos.ToVec3(heightSample);

                Vector2 percent = (tileCoord + Vector2.One) / (ChunkSize + Vector2.One);
                vData.Color = Color.WhiteUint;// Color.Lerp(Color.Black, Color.White, (heightSample * 20) / 280).ToUint();
                //vData.UV = new Vector2(1.0f - (x / ChunkSize.X), 1.0f - (y / ChunkSize.Y));

                Vector2 mapSize = new Vector2(533.3333f, 533.33105f);
                vData.UV = new Vector2(1.0f - (worldPos.X / mapSize.X), 1.0f - (worldPos.Y / mapSize.Y));

                

                vIdx++;
            }
        }

        // Calculate vertex normals
        for (int i = 0; i < _indices.Length; i += 3)
        {
            int index0 = _indices[i];
            int index1 = _indices[i + 1];
            int index2 = _indices[i + 2];

            ref VertexData_Pos_UV_Normal_Color v0 = ref vertices[index0];
            ref VertexData_Pos_UV_Normal_Color v1 = ref vertices[index1];
            ref VertexData_Pos_UV_Normal_Color v2 = ref vertices[index2];

            var tri = new Triangle(v0.Position, v1.Position, v2.Position);
            Vector3 normal = tri.Normal;

            v0.Normal += normal;
            v1.Normal += normal;
            v2.Normal += normal;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[i];
            vData.Normal = Vector3.Normalize(vData.Normal);
        }

        renderCache.GPUDirty = true;
        renderCache.VerticesGeneratedForVersion = chunk.ChunkVersion;
    }

    #endregion

    #region Rendering - Index Buffer

    private Vector2 _indexBufferChunkSize;
    private ushort[] _indices;
    private IndexBuffer? _indexBuffer;

    private void PrepareIndexBuffer()
    {
        Assert(GLThread.IsGLThread());

        int vertexCount = (int)(ChunkSize.X * ChunkSize.Y);
        int stichingVertices = (int)(ChunkSize.X + ChunkSize.Y + 1);
        vertexCount += stichingVertices;

        // ChunkSize - 1 + stiching 1
        int quads = (int)(ChunkSize.X * ChunkSize.Y);
        int stride = (int)ChunkSize.X + 1;
        int indexCount = quads * 6;

        _indexBuffer ??= new IndexBuffer();

        ushort[] indices = new ushort[indexCount];
        int indexOffset = 0;
        for (int i = 0; i < vertexCount - stride; i++)
        {
            if ((i + 1) % stride == 0) continue;

            indices[indexOffset + 0] = (ushort)(i + stride);
            indices[indexOffset + 1] = (ushort)(i + stride + 1);
            indices[indexOffset + 2] = (ushort)(i + 1);

            indices[indexOffset + 3] = (ushort)(i + 1);
            indices[indexOffset + 4] = (ushort)(i);
            indices[indexOffset + 5] = (ushort)(i + stride);

            indexOffset += 6;
        }

        _indexBuffer.Upload(indices);
        _indexBufferChunkSize = ChunkSize;
        _indices = indices;
    }

    #endregion

    #region World Space Helpers

    public float GetHeightAt(Vector2 worldSpace)
    {
        worldSpace -= TileSize; // Stiching

        Vector2 tilePos = worldSpace / TileSize;
        Vector2 floorPos = tilePos.Floor();
        Vector2 ceilPos = tilePos.Ceiling();

        TerrainMeshGridChunk? chunk00 = GetChunkAt(floorPos, out Vector2 relCoord00);
        TerrainMeshGridChunk? chunk10 = GetChunkAt(new Vector2(ceilPos.X, floorPos.Y), out Vector2 relCoord10);
        TerrainMeshGridChunk? chunk01 = GetChunkAt(new Vector2(floorPos.X, ceilPos.Y), out Vector2 relCoord01);
        TerrainMeshGridChunk? chunk11 = GetChunkAt(ceilPos, out Vector2 relCoord11);

        float h00 = chunk00 != null ? GetAtForChunk(chunk00, relCoord00) : 0;
        float h10 = chunk10 != null ? GetAtForChunk(chunk10, relCoord10) : 0;
        float h01 = chunk01 != null ? GetAtForChunk(chunk01, relCoord01) : 0;
        float h11 = chunk11 != null ? GetAtForChunk(chunk11, relCoord11) : 0;

        float fracX = tilePos.X - floorPos.X;
        float fracY = tilePos.Y - floorPos.Y;

        float h0 = Maths.Lerp(h00, h10, fracX); // bottom
        float h1 = Maths.Lerp(h01, h11, fracX); // top
        float height = Maths.Lerp(h0, h1, fracY); // both

        return height;
    }

    public Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        location -= TileSize;

        float left = MathF.Round(location.X / TileSize.X);
        float top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        Vector2 worldPos = (tileCoord2d * TileSize) + TileSize;
        return worldPos;
    }

    #endregion

    #region Brush

    private bool _editorBrush;
    private float _editorBrushSize;
    private Vector2 _editorBrushPosition;

    public void SetEditorBrush(bool enabled, float brushSize)
    {
        _editorBrush = enabled;
        _editorBrushSize = brushSize;
    }

    public Vector2 GetEditorBrushWorldPosition()
    {
        return _editorBrushPosition;
    }

    private Vector2 GetEditorBrush()
    {
        Vector2 brushPoint = Vector2.NaN;
        if (_editorBrush)
        {
            CameraBase camera = Engine.Renderer.Camera;
            Ray3D mouseRay = camera.GetCameraMouseRay();
            Vector3 mousePosWorld = mouseRay.IntersectWithPlane(RenderComposer.Up, Vector3.Zero);

            foreach (TerrainGridChunkRuntimeCache chunkToRender in _renderThisPass)
            {
                VertexDataAllocation vertices = chunkToRender.VertexMemory;
                if (!vertices.Allocated) continue;

                if (mouseRay.IntersectWithVertices(_indices, vertices, out Vector3 collisionPoint, out _, out _))
                    brushPoint = collisionPoint.ToVec2();
            }
        }

        _editorBrushPosition = brushPoint;
        return _editorBrushPosition;
    }

    #endregion

    private class TerrainGridChunkRuntimeCache : IOctTreeStorable
    {
        public TerrainMeshGridChunk Chunk;

        public int VerticesGeneratedForVersion = -1;
        public VertexDataAllocation VertexMemory;

        public GPUVertexMemory? GPUMemory;
        public bool GPUDirty = true;

        public int BoundsVersion = -1;
        public Cube Bounds;

        public TerrainGridChunkRuntimeCache(TerrainMeshGridChunk chunk)
        {
            Chunk = chunk;
        }

        public Cube GetOctTreeBound()
        {
            return Bounds;
        }
    }
}