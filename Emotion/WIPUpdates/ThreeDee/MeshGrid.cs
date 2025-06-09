using Emotion.Common.Serialization;
using Emotion.Game.OctTree;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.Rendering;
using OpenGL;

namespace Emotion.WIPUpdates.ThreeDee;

#nullable enable

public abstract class MeshGrid<T, ChunkT> : ChunkedGrid<T, ChunkT>, IGridWorldSpaceTiles, ITerrainGrid3D
    where ChunkT : VersionedGridChunk<T>, new()
    where T : struct, IEquatable<T>
{
    public bool Initialized { get; private set; }

    //[SerializeNonPublicGetSet]
    public Vector2 TileSize { get; protected set; }

    public MeshGrid(Vector2 tileSize, float chunkSize) : base(chunkSize)
    {
        TileSize = tileSize;
    }

    // serialization
    protected MeshGrid()
    {

    }

    #region API

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

    public void Update(float dt)
    {
        // Chunk bounds are considered "eventually consistent" with the data.
        // This is to decrease the performance hit of large edits.
        foreach (KeyValuePair<Vector2, MeshGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            UpdateChunkBounds(item.Key, item.Value);
        }
    }

    public abstract float GetHeightAt(Vector2 worldSpace);

    #endregion

    #region Rendering

    protected List<MeshGridChunkRuntimeCache> _renderThisPass = new(32);

    private void MarkChunkForRender(Vector2 chunkCoord, ChunkT chunk)
    {
        if (!_chunkRuntimeData.TryGetValue(chunkCoord, out MeshGridChunkRuntimeCache? renderCache)) return;

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
    public virtual void Render(RenderComposer c, Rectangle clipArea)
    {
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
                ChunkT? chunk = GetChunk(chunkCoord);
                if (chunk == null) continue;
                MarkChunkForRender(chunkCoord, chunk);
            }
        }
       
        if (_renderThisPass.Count > 0)
        {
            Engine.Renderer.FlushRenderStream();

            RenderState oldState = c.CurrentState.Clone();

            Engine.Renderer.SetFaceCulling(true, true);

            MeshMaterial material = GetMeshMaterial();
            NewShaderAsset? asset = material.Shader?.Get();
            if (asset != null && asset.CompiledShader != null)
                c.SetShader(asset.CompiledShader);

            c.CurrentState.Shader.SetUniformInt("diffuseTexture", 0);
            Texture.EnsureBound(material.DiffuseTexture.Pointer);

            SetupShaderState(c.CurrentState.Shader);

            foreach (MeshGridChunkRuntimeCache chunkToRender in _renderThisPass)
            {
                IndexBuffer? indexBuffer = chunkToRender.IndexBuffer;
                if (indexBuffer == null) continue;

                GPUVertexMemory? gpuMem = chunkToRender.GPUMemory;
                if (gpuMem == null) continue;

                VertexArrayObject.EnsureBound(gpuMem.VAO);
                IndexBuffer.EnsureBound(indexBuffer.Pointer);

                int indices = chunkToRender.IndicesUsed;
                Gl.DrawElements(PrimitiveType.Triangles, indices, indexBuffer.DataType, IntPtr.Zero);
            }

            c.SetState(oldState);

            // Draw arrows for normals
            //foreach (TerrainGridChunkRuntimeCache chunkToRender in _renderThisPass)
            //{
            //    VertexDataAllocation vertices = chunkToRender.VertexMemory;
            //    Span<VertexData_Pos_UV_Normal_Color> verticesSpan = vertices.GetAsSpan<VertexData_Pos_UV_Normal_Color>();
            //    for (int i = 0; i < verticesSpan.Length; i++)
            //    {
            //        ref VertexData_Pos_UV_Normal_Color vert = ref verticesSpan[i];
            //        if (vert.Normal == RenderComposer.Up) continue;

            //        c.RenderLine(vert.Position, vert.Position + vert.Normal * 0.5f, Color.Red, 0.05f);
            //    }
            //}
        }

        _octTree.RenderDebug(c);
    }

    #endregion

    #region Chunk Mesh Management

    private OctTree<MeshGridChunkRuntimeCache> _octTree = new();
    protected Dictionary<Vector2, MeshGridChunkRuntimeCache> _chunkRuntimeData = new();

    public IEnumerator InitRuntimeDataRoutine()
    {
        Initialized = true;
        foreach (KeyValuePair<Vector2, ChunkT> item in _chunks)
        {
            OnChunkCreated(item.Key, item.Value);
        }

        // Create bounds.
        Update(0);

        yield break;
    }

    // These events should only really trigger in the editor or if some game is dynamically editing the terrain.

    protected override void OnChunkCreated(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkCreated(chunkCoord, newChunk);
        if (!Initialized) return;

        var renderCache = new MeshGridChunkRuntimeCache(newChunk);
        _chunkRuntimeData.Add(chunkCoord, renderCache);
    }

    protected override void OnChunkRemoved(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkRemoved(chunkCoord, newChunk);
        if (!Initialized) return;

        // Free resources
        if (_chunkRuntimeData.Remove(chunkCoord, out MeshGridChunkRuntimeCache? renderCache))
        {
            _octTree.Remove(renderCache);
            VertexDataAllocation.FreeAllocated(ref renderCache.VertexMemory);
            GPUMemoryAllocator.FreeBuffer(renderCache.GPUMemory);
        }
    }

    private void UpdateChunkBounds(Vector2 chunkCoord, MeshGridChunkRuntimeCache renderCache)
    {
        VersionedGridChunk<T> chunk = renderCache.Chunk;

        if (renderCache.BoundsVersion != chunk.ChunkVersion) return;

        _octTree.Update(renderCache);
        renderCache.BoundsVersion = chunk.ChunkVersion;
    }

    protected abstract void UpdateChunkVertices(Vector2 chunkCoord, MeshGridChunkRuntimeCache renderCache, bool propagate = true);

    protected abstract MeshMaterial GetMeshMaterial();

    protected virtual void SetupShaderState(ShaderProgram shader)
    {
        // nop
    }

    #endregion

    protected class MeshGridChunkRuntimeCache : IOctTreeStorable
    {
        public VersionedGridChunk<T> Chunk;

        public int VerticesGeneratedForVersion = -1;
        public VertexDataAllocation VertexMemory;
        public int IndicesUsed = -1;

        public GPUVertexMemory? GPUMemory;
        public bool GPUDirty = true;

        public int BoundsVersion = -1;
        public Cube Bounds;

        public IndexBuffer? IndexBuffer;
        public int IndexCount;

        public MeshGridChunkRuntimeCache(VersionedGridChunk<T> chunk)
        {
            Chunk = chunk;
        }

        public Span<VertexData_Pos_UV_Normal_Color> EnsureVertexMemoryAndGetSpan(Vector2 chunkCoord, int vertexCount)
        {
            if (!VertexMemory.Allocated)
                VertexMemory = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal_Color.Format, vertexCount, $"TerrainChunk_{chunkCoord}");
            else if (VertexMemory.VertexCount < vertexCount)
                VertexMemory = VertexDataAllocation.Reallocate(ref VertexMemory, vertexCount);

            return VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal_Color>();
        }

        public Cube GetOctTreeBound()
        {
            return Bounds;
        }
    }
}