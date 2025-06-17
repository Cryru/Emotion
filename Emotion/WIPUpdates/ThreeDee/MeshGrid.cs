using Emotion.Common.Serialization;
using Emotion.Game.OctTree;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.Utility;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.Rendering;
using OpenGL;
using System;
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

    public virtual Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        float left = MathF.Round(location.X / TileSize.X);
        float top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public virtual Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        Vector2 worldPos = (tileCoord2d * TileSize);
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

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData)
    {
        foreach (KeyValuePair<Vector2, MeshGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            MeshGridChunkRuntimeCache chunkCache = item.Value;
            VertexDataAllocation vertices = chunkCache.VertexMemory;
            if (!vertices.Allocated) continue;
            if (chunkCache.Bounds.IsEmpty) continue;
            if (!chunkCache.Bounds.Intersects(cube)) continue;

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

    public Vector3 SweepCube(Cube moverCube, Vector3 movement)
    {
        // Sweep in all axes separetely for best results.
        Vector3 moveAmount = movement;
        for (int i = 0; i < 3; i++)
        {
            int axis = i;

            float moveInThisDir = moveAmount[axis];
            if (moveInThisDir == 0) continue;

            Vector3 movementVector = Vector3.Zero;
            movementVector[axis] = moveInThisDir;

            Vector3 asCollision = SweepCubeInternal(moverCube, movementVector);
            moveAmount[axis] = asCollision[axis];
            moverCube.Origin[axis] += asCollision[axis];
        }

        return moveAmount;
    }

    private Vector3 SweepCubeInternal(Cube cube, Vector3 movement)
    {
        Cube cubeMoved = cube;
        cubeMoved.Origin += movement;
        Cube sweepBound = cube.Union(cubeMoved);

        (Vector3 aMin, Vector3 aMax) = cube.GetMinMax();

        float earliestHit = 1f;

        foreach (KeyValuePair<Vector2, MeshGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            MeshGridChunkRuntimeCache chunkCache = item.Value;
            if (chunkCache.Bounds.IsEmpty) continue;
            if (!chunkCache.Bounds.Intersects(sweepBound)) continue;

            if (chunkCache.Colliders == null) continue; // todo: vertices based chunks? triangle collision

            foreach (Cube other in chunkCache.Colliders)
            {
                (Vector3 bMin, Vector3 bMax) = other.GetMinMax();

                float tEntry = 0f;
                float tExit = 1f;

                // For each axis, compute entry and exit times
                for (int i = 0; i < 3; i++)
                {
                    float aMinI = aMin[i], aMaxI = aMax[i];
                    float bMinI = bMin[i], bMaxI = bMax[i];
                    float moveForAxis = movement[i];

                    if (moveForAxis == 0f)
                    {
                        // If already separated, a hit is not possible
                        if (aMaxI <= bMinI || aMinI >= bMaxI)
                        {
                            tEntry = 1f;
                            tExit = 0f;
                            break;
                        }
                        // already overlapping despite no move :/
                    }
                    else
                    {
                        // Equal, but moving apart
                        if ((aMaxI == bMinI && moveForAxis < 0f) || (aMinI == bMaxI && moveForAxis > 0f))
                        {
                            tEntry = 1f;
                            tExit = 0f;
                            break;
                        }

                        // Compute entry and exit distances
                        float invEntry, invExit;
                        if (moveForAxis > 0f)
                        {
                            invEntry = bMinI - aMaxI;
                            invExit = bMaxI - aMinI;
                        }
                        else
                        {
                            invEntry = bMaxI - aMinI;
                            invExit = bMinI - aMaxI;
                        }

                        float entryI = invEntry / moveForAxis;
                        float exitI = invExit / moveForAxis;

                        // Entry must be before exit
                        if (entryI > exitI)
                            (entryI, exitI) = (exitI, entryI);

                        // Latest entry, earliest exit
                        tEntry = Math.Max(tEntry, entryI);
                        tExit = Math.Min(tExit, exitI);

                        // No overlap possible
                        if (tEntry > tExit) break;
                    }
                }

                // If entry is smaller (or equal) to exit and its within 0,1 then we have a collision
                if (tEntry <= tExit && tEntry >= 0f && tEntry < earliestHit)
                    earliestHit = tEntry;
            }
        }

        return movement * earliestHit;
    }

    public bool CollideRay(Ray3D ray, out Vector3 collisionPoint)
    {
        collisionPoint = Vector3.Zero;

        Vector3 closestIntersection = Vector3.Zero;
        float closestIntersectionDist = float.PositiveInfinity;

        foreach (KeyValuePair<Vector2, MeshGridChunkRuntimeCache> item in _chunkRuntimeData)
        {
            MeshGridChunkRuntimeCache chunkCache = item.Value;
            if (chunkCache.Bounds.IsEmpty) continue;
            if (!ray.IntersectWithCube(chunkCache.Bounds, out Vector3 _)) continue;

            if (chunkCache.Colliders == null) continue; // todo: vertices based chunks? triangle collision

            foreach (Cube other in chunkCache.Colliders)
            {
                if (ray.IntersectWithCube(other, out Vector3 colPoint))
                {
                    float dist = Vector3.Distance(colPoint, ray.Start);
                    if (dist < closestIntersectionDist)
                    {
                        closestIntersection = colPoint;
                        closestIntersectionDist = dist;
                    }
                }
            }
        }

        collisionPoint = closestIntersection;
        return closestIntersectionDist != float.PositiveInfinity;
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