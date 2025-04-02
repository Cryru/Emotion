#nullable enable

using Emotion.Common.Serialization;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shader;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.Rendering;
using OpenGL;

namespace Emotion.WIPUpdates.ThreeDee;

public class TerrainMeshGrid : ChunkedGrid<float, VersionedGridChunk<float>>, IGridWorldSpaceTiles
{
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

    public float GetHeightAt(Vector2 worldSpace)
    {
        worldSpace -= TileSize; // Stiching

        Vector2 tilePos = worldSpace / TileSize;
        Vector2 floorPos = tilePos.Floor();
        Vector2 ceilPos = tilePos.Ceiling();

        VersionedGridChunk<float>? chunk00 = GetChunkAt(floorPos, out Vector2 relCoord00);
        VersionedGridChunk<float>? chunk10 = GetChunkAt(new Vector2(ceilPos.X, floorPos.Y), out Vector2 relCoord10);
        VersionedGridChunk<float>? chunk01 = GetChunkAt(new Vector2(floorPos.X, ceilPos.Y), out Vector2 relCoord01);
        VersionedGridChunk<float>? chunk11 = GetChunkAt(ceilPos, out Vector2 relCoord11);

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

    // todo: 3d culling
    public void Render(RenderComposer c, Rectangle clipArea)
    {
        if (ChunkSize != _indexBufferChunkSize || _indexBuffer == null)
            PrepareIndexBuffer();
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

        ResetChunksMarkedToRender();
        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 chunkCoord = new Vector2(x, y);
                VersionedGridChunk<float>? chunk = GetChunk(chunkCoord);
                if (chunk == null) continue;
                MarkChunkForRender(chunk, chunkCoord);
            }
        }

        Vector2 brushWorldSpace = GetEditorBrush();
        if (_renderThisPass != null)
        {
            Engine.Renderer.FlushRenderStream();

            var oldState = c.CurrentState.Clone();

            PrepareChunkRendering(c);
            c.CurrentState.Shader.SetUniformVector2("brushWorldSpace", brushWorldSpace);
            c.CurrentState.Shader.SetUniformFloat("brushRadius", _editorBrushSize);
            Texture.EnsureBound(TerrainMeshMaterial.DiffuseTexture.Pointer);

            for (int i = 0; i < _renderThisPass.Count; i++)
            {
                TerrainGridRenderCacheChunk chunkToRender = _renderThisPass[i];
                GPUVertexMemory? gpuMem = chunkToRender.GPUMemory;
                if (gpuMem == null) continue;

                VertexArrayObject.EnsureBound(gpuMem.VAO);
                IndexBuffer.EnsureBound(_indexBuffer.Pointer);

                Gl.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
            }

            //c.SetShader(null);
            c.SetState(oldState);
        }
    }

    private void PrepareChunkRendering(RenderComposer c)
    {
        Engine.Renderer.SetFaceCulling(true, true);

        NewShaderAsset? asset = TerrainMeshMaterial.Shader?.Get();
        if (asset != null && asset.CompiledShader != null)
            c.SetShader(asset.CompiledShader);

        c.CurrentState.Shader.SetUniformInt("diffuseTexture", 0);
    }

    private List<TerrainGridRenderCacheChunk>? _renderThisPass;
    private Dictionary<Vector2, TerrainGridRenderCacheChunk> _cachedChunks = new();

    private Vector2 _indexBufferChunkSize;
    private ushort[] _indices;
    private IndexBuffer? _indexBuffer;

    private void PrepareIndexBuffer()
    {
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

    private void ResetChunksMarkedToRender()
    {
        _renderThisPass?.Clear();
    }

    private void MarkChunkForRender(VersionedGridChunk<float> chunk, Vector2 chunkCoord)
    {
        _renderThisPass ??= new List<TerrainGridRenderCacheChunk>(32);

        _cachedChunks.TryGetValue(chunkCoord, out TerrainGridRenderCacheChunk? cachedChunk);
        if (cachedChunk == null)
        {
            cachedChunk = new TerrainGridRenderCacheChunk();
            _cachedChunks.Add(chunkCoord, cachedChunk);
        }
        _renderThisPass.Add(cachedChunk);

        UpdateChunkRenderCache(cachedChunk, chunk, chunkCoord);

        if (cachedChunk.GPUMemory == null || cachedChunk.GPUDirty)
        {
            cachedChunk.GPUMemory ??= GPUMemoryAllocator.AllocateBuffer(cachedChunk.VertexMemory.Format);
            cachedChunk.GPUMemory.VBO.Upload(cachedChunk.VertexMemory);
            cachedChunk.GPUDirty = false;
        }
    }

    private void UpdateChunkRenderCache(TerrainGridRenderCacheChunk chunkCache, VersionedGridChunk<float> chunk, Vector2 chunkCoord)
    {
        // We already have the latest version of this
        if (chunkCache.CachedVersion == chunk.ChunkVersion && chunkCache.VertexMemory.Allocated) return;

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

        if (!chunkCache.VertexMemory.Allocated)
            chunkCache.VertexMemory = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal_Color.Format, vertexCount, $"TerrainChunk_{chunkCoord}");
        else if (chunkCache.VertexMemory.VertexCount < vertexCount)
            chunkCache.VertexMemory = VertexDataAllocation.Reallocate(ref chunkCache.VertexMemory, vertexCount);

        var vertices = chunkCache.VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal_Color>();

        // Get data for stiching vertices
        float[] dataTop = chunkTop?.GetRawData() ?? Array.Empty<float>();
        float[] dataLeft = chunkLeft?.GetRawData() ?? Array.Empty<float>();
        float[] dataMe = chunk.GetRawData() ?? Array.Empty<float>();

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
                vData.Position = worldPos.ToVec3(heightSample);

                Vector2 percent = (tileCoord + Vector2.One) / (ChunkSize + Vector2.One);
                vData.Color = Color.Lerp(Color.Black, Color.White, (heightSample * 20) / 40).ToUint();
                //vData.UV = new Vector2(1.0f - (x / ChunkSize.X), 1.0f - (y / ChunkSize.Y));

                Vector2 mapSize = new Vector2(533.3333f, 533.33105f);
                vData.UV = new Vector2(1.0f - (worldPos.X / mapSize.X), 1.0f - (worldPos.Y / mapSize.Y));

                vData.Normal = new Vector3(0, 0, -1);

                vIdx++;
            }
        }

        chunkCache.GPUDirty = true;
        chunkCache.CachedVersion = chunk.ChunkVersion;
    }

    #region World Space

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
        if (_renderThisPass != null && _editorBrush)
        {
            CameraBase camera = Engine.Renderer.Camera;
            Ray3D mouseRay = camera.GetCameraMouseRay();
            Vector3 mousePosWorld = mouseRay.IntersectWithPlane(RenderComposer.Up, Vector3.Zero);

            for (int i = 0; i < _renderThisPass.Count; i++)
            {
                TerrainGridRenderCacheChunk chunkToRender = _renderThisPass[i];
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

    private class TerrainGridRenderCacheChunk
    {
        public int CachedVersion = -1;
        public VertexDataAllocation VertexMemory;
        public GPUVertexMemory? GPUMemory;
        public bool GPUDirty = true;
    }
}
