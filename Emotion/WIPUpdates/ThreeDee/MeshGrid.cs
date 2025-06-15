using Emotion.Common.Serialization;
using Emotion.Game.OctTree;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.Rendering;
using OpenGL;
using System.Diagnostics.Metrics;

namespace Emotion.WIPUpdates.ThreeDee;

#nullable enable

public abstract class MeshGrid<T, ChunkT, IndexT> : ChunkedGrid<T, ChunkT>, IGridWorldSpaceTiles, ITerrainGrid3D
    where ChunkT : VersionedGridChunk<T>, new()
    where T : struct, IEquatable<T>
    where IndexT : INumber<IndexT>
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

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData)
    {
        foreach (KeyValuePair<Vector2, MeshGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            MeshGridChunkRuntimeCache chunkCache = item.Value;
            VertexDataAllocation vertices = chunkCache.VertexMemory;
            if (!vertices.Allocated) continue;
            if (chunkCache.Bounds.IsEmpty) continue;
            //if (!chunkCache.Bounds.Intersects(cube)) continue;

            //Engine.Renderer.DbgAddCube(chunkCache.Bounds);

            if (chunkCache.Colliders != null)
            {
                for (int i = 0; i < chunkCache.Colliders.Count; i++)
                {
                    Cube collider = chunkCache.Colliders[i];
                    if (cube.Intersects(collider))
                    {
                        bool stopChecks = onIntersect(collider, userData);
                        if (stopChecks)
                            return true;
                    }
                }
            }
            else
            {
                IndexT[]? indices = chunkCache.CPUIndexBuffer;
                if (indices == null) continue;

                if (cube.IntersectWithVertices(indices, chunkCache.IndicesUsed, vertices, out Vector3 collisionPoint, out Vector3 normal, out _))
                    return true;
            }
        }

        return false;
    }

    public Vector3 SweepCube(Cube cube, Vector3 movement)
    {
        Engine.Renderer.DbgClear();
        float tFirst = 1.0f;
        (Vector3 aMin, Vector3 aMax) = cube.GetMinMax();
        Vector3 safeMovement = movement;
        foreach (KeyValuePair<Vector2, MeshGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            MeshGridChunkRuntimeCache chunkCache = item.Value;
            VertexDataAllocation vertices = chunkCache.VertexMemory;
            if (!vertices.Allocated) continue;
            if (chunkCache.Bounds.IsEmpty) continue;
            if (chunkCache.Colliders == null) continue; // todo: vertices based chunks? triangle collision

            foreach (Cube other in chunkCache.Colliders)
            {
                (Vector3 bMin, Vector3 bMax) = other.GetMinMax();




                for (int axis = 0; axis < 3; ++axis)
                {
                    float aMinA = aMin[axis];
                    float aMaxA = aMax[axis];
                    float bMinA = bMin[axis];
                    float bMaxA = bMax[axis];
                    float v = movement[axis];

                    if (v != 0.0f)
                    {
                        float invV = 1.0f / v;
                        float tEnter = (bMinA - aMaxA) * invV;
                        float tExit = (bMaxA - aMinA) * invV;

                        if (tEnter > tExit) (tEnter, tExit) = (tExit, tEnter);

                        if (tEnter >= 0.0f && tEnter <= 1.0f)
                        {
                            Engine.Renderer.DbgAddCube(other);
                            // Limit the movement along this axis
                            safeMovement[axis] = v * tEnter;
                        }
                    }

                    //if (v == 0.0f)
                    //{
                    //    // If no movement on this axis, but gaps exist, skip
                    //    if (aMaxA <= bMinA || aMinA >= bMaxA)
                    //    {
                    //        tEnter = 1.0f; tExit = 0.0f;
                    //        break;
                    //    }
                    //}
                    //else
                    //{
                    //    // Compute entry and exit times on this axis
                    //    float invV = 1.0f / v;
                    //    float t1 = (bMinA - aMaxA) * invV;
                    //    float t2 = (bMaxA - aMinA) * invV;
                    //    float tAxisEnter = MathF.Min(t1, t2);
                    //    float tAxisExit = MathF.Max(t1, t2);

                    //    tEnter = MathF.Max(tEnter, tAxisEnter);
                    //    tExit = MathF.Min(tExit, tAxisExit);

                    //    // No overlap on this axis during movement
                    //    if (tEnter > tExit) break;
                    //}
                }

                // If collision occurs within [0,1]
                //if (tEnter <= tExit && tEnter < tFirst && tEnter >= 0.0f)
                //{
                //    tFirst = tEnter;
                //}
            }
        }

        // Clamp to [0,1]
        if (tFirst < 0.0f) tFirst = 0.0f;
        if (tFirst > 1.0f) tFirst = 1.0f;

        return safeMovement;
    }

    #endregion

    #region Rendering

    protected List<MeshGridChunkRuntimeCache> _renderThisPass = new(32);

    private void MarkChunkForRender(Vector2 chunkCoord, ChunkT chunk)
    {
        if (!_chunkRuntimeData.TryGetValue(chunkCoord, out MeshGridChunkRuntimeCache? renderCache)) return;

        UpdateChunkVertices(chunkCoord, renderCache);
        Assert(renderCache.VertexMemory.Allocated);

        // todo: deallocate at some point?
        if (renderCache.GPUVertexMemory == null || renderCache.GPUDirty)
        {
            renderCache.GPUVertexMemory ??= GPUMemoryAllocator.AllocateBuffer(renderCache.VertexMemory.Format);
            renderCache.GPUVertexMemory.VBO.Upload(renderCache.VertexMemory);
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

                GPUVertexMemory? gpuMem = chunkToRender.GPUVertexMemory;
                if (gpuMem == null) continue;

                VertexArrayObject.EnsureBound(gpuMem.VAO);
                IndexBuffer.EnsureBound(indexBuffer.Pointer);

                int indices = chunkToRender.IndicesUsed;
                Gl.DrawElements(PrimitiveType.Triangles, indices, indexBuffer.DataType, IntPtr.Zero);
            }

            c.SetState(oldState);

            // Draw colliders
            //foreach (MeshGridChunkRuntimeCache chunkToRender in _renderThisPass)
            //{
            //    if (chunkToRender.Colliders == null) continue;
            //    foreach (var collider in chunkToRender.Colliders)
            //    {
            //        collider.RenderOutline(c, Color.Blue);
            //    }
            //}

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
            GPUMemoryAllocator.FreeBuffer(renderCache.GPUVertexMemory);
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
        public bool GPUDirty = true;

        #region Vertices

        public int VerticesGeneratedForVersion = -1;
        public VertexDataAllocation VertexMemory;
        public GPUVertexMemory? GPUVertexMemory;

        #endregion

        #region Bounds

        public int BoundsVersion = -1;
        public Cube Bounds;

        #endregion

        #region Indices

        public IndexT[]? CPUIndexBuffer { get; private set; } // todo: index allocation type
        public IndexBuffer? IndexBuffer { get; private set; }
        public int IndicesUsed { get; private set; } = -1;

        #endregion

        #region Collision

        public List<Cube>? Colliders; // todo: collider type

        #endregion

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

        public void SetIndices(IndexT[] cpuIndices, IndexBuffer gpuIndices, int indexCount)
        {
            CPUIndexBuffer = cpuIndices;
            IndexBuffer = gpuIndices;
            IndicesUsed = indexCount;
        }

        public Cube GetOctTreeBound()
        {
            return Bounds;
        }
    }
}