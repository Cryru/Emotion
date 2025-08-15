#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using Emotion.Core.Utility.Time;
using Emotion.Editor;
using Emotion.Game.World.Terrain;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;
using Emotion.Graphics.Shading;
using Emotion.Primitives.Grids;
using OpenGL;
using System.Collections.Concurrent;

namespace Emotion.Game.World.Terrain;

[DontSerialize]
public abstract partial class MeshGrid<T, ChunkT, IndexT> : ChunkedGrid<T, ChunkT>, IGridWorldSpaceTiles, ITerrainGrid3D, IStreamableGrid
    where ChunkT : MeshGridStreamableChunk<T, IndexT>, new()
    where T : struct, IEquatable<T>
    where IndexT : INumber<IndexT>
{
    public bool Initialized { get; private set; }

    //[SerializeNonPublicGetSet]
    public Vector2 TileSize { get; protected set; }

    public MeshGrid(Vector2 tileSize, float chunkSize) : base(chunkSize)
    {
        TileSize = tileSize;

        if (ChunkStreamManager == null)
        {
            int factor = (int)MathF.Max(tileSize.Y, tileSize.X);
            ChunkStreamManager = new ChunkStreamManager(512 * factor, 512 * factor);
        }
    }

    public virtual IEnumerator InitRuntimeDataRoutine()
    {
        SetupDebugVisualizations();
        Initialized = true;
        yield break;
    }

    public virtual void UnloadRuntimeData()
    {
        EngineEditor.RemoveEditorVisualizations(this);
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
        if (!Initialized) return;
        ChunkStreamManager.Update(this);
        ProcessChunkUpdateMeshQueue();
    }

    public abstract float GetHeightAt(Vector2 worldSpace);

    #endregion

    #region Chunk Update API

    private HashSet<Vector2> _updateChunks = new HashSet<Vector2>(64);
    private Queue<Vector2> _queuedUpdateChunks = new Queue<Vector2>(64);
    private Queue<Vector2> _queuedUpdateChunksBB = new Queue<Vector2>(64);
    private Lock _chunkUpdateLock = new();

    protected override void OnChunkChanged(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkChanged(chunkCoord, newChunk);
        RequestChunkMeshUpdate(chunkCoord, newChunk);
    }

    protected void UpdateDependentChunk(Vector2 chunkCoord, ChunkT newChunk)
    {
        RequestChunkMeshUpdate(chunkCoord, newChunk);
    }

    public void RequestChunkMeshUpdate(Vector2 chunkCoord, ChunkT newChunk)
    {
        lock (_chunkUpdateLock)
        {
            if (_updateChunks.Add(chunkCoord))
                _queuedUpdateChunks.Enqueue(chunkCoord);
        }
    }

    private IEnumerator UpdateChunkMeshRoutine(Vector2 chunkCoord, ChunkT chunk)
    {
        Assert(chunk.Busy);

        yield return CheckStreamingStateChange(chunkCoord, chunk);

        if (chunk.State >= ChunkState.HasMesh)
        {
            UpdateChunkVertices(chunkCoord, chunk);
        }
        
        if (chunk.State >= ChunkState.HasGPUData)
        {
            yield return GLThread.ExecuteOnGLThreadAsync(static (chunk) =>
            {
                // Assert that the chunk is really in this state.
                Assert(chunk.GPUVertexMemory != null);
                if (chunk.GPUVertexMemory == null) return;

                Assert(chunk.VertexMemory.Allocated);
                if (!chunk.VertexMemory.Allocated) return;

                chunk.GPUVertexMemory.VBO.Upload(chunk.VertexMemory, chunk.VerticesUsed);
                chunk.GPUUploadedVersion = chunk.VerticesGeneratedForVersion;
            }, chunk);
        }
        chunk.Busy = false;
    }

    private void ProcessChunkUpdateMeshQueue()
    {
        lock (_chunkUpdateLock)
        {
            Assert(_queuedUpdateChunksBB.Count == 0);

            while (_queuedUpdateChunks.TryDequeue(out Vector2 chunkCoord))
            {
                ChunkT? chunk = GetChunk(chunkCoord);
                if (chunk == null)
                {
                    _updateChunks.Remove(chunkCoord);
                    continue;
                }

                if (chunk.Busy)
                {
                    // Queue back
                    _queuedUpdateChunksBB.Enqueue(chunkCoord);
                    continue;
                }

                chunk.Busy = true;
                Engine.Jobs.Add(UpdateChunkMeshRoutine(chunkCoord, chunk));
                _updateChunks.Remove(chunkCoord);
            }

            (_queuedUpdateChunks, _queuedUpdateChunksBB) = (_queuedUpdateChunksBB, _queuedUpdateChunks);
            Assert(_queuedUpdateChunksBB.Count == 0);
            _queuedUpdateChunksBB.Clear();
        }
    }

    #endregion

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData)
    {
        Dictionary<Vector2, ChunkT> chunks = GetChunksInState(ChunkState.HasMesh);
        foreach (KeyValuePair<Vector2, ChunkT> item in chunks)
        {
            ChunkT chunk = item.Value;
            VertexDataAllocation vertices = chunk.VertexMemory;
            if (!vertices.Allocated) continue;
            if (chunk.Bounds.IsEmpty) continue;
            if (!chunk.Bounds.Intersects(cube)) continue;

            if (chunk.Colliders != null)
            {
                for (int i = 0; i < chunk.Colliders.Count; i++)
                {
                    Cube collider = chunk.Colliders[i];
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
                IndexT[]? indices = chunk.CPUIndexBuffer;
                if (indices == null) continue;

                if (cube.IntersectWithVertices(indices, chunk.IndicesUsed, vertices, out Vector3 collisionPoint, out Vector3 normal, out _))
                    return true;
            }
        }

        return false;
    }

    public Vector3 SweepCube(Cube moverCube, Vector3 movement)
    {
        // We sort the movement by magnitude
        VectorExtensions.SortComponents(movement, out Vector3 _, out Vector3 moveRemap);

        // Sweep in all axes separately for best results.
        Vector3 moveAmount = movement;
        for (int i = 0; i < 3; i++)
        {
            int axis = (int)moveRemap[i];
            float moveInThisDir = movement[axis];
            if (Maths.Approximately(moveInThisDir, 0)) continue;

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

        Dictionary<Vector2, ChunkT> chunks = GetChunksInState(ChunkState.HasMesh);
        foreach (KeyValuePair<Vector2, ChunkT> item in chunks)
        {
            ChunkT chunk = item.Value;
            if (chunk.Bounds.IsEmpty) continue;
            if (!chunk.Bounds.Intersects(sweepBound)) continue;

            if (chunk.Colliders == null) continue; // todo: vertices based chunks? triangle collision

            foreach (Cube other in chunk.Colliders)
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
                        if (aMaxI < bMinI || aMinI > bMaxI || Maths.Approximately(aMaxI, bMinI) || Maths.Approximately(aMinI, bMaxI))
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
                        if ((Maths.Approximately(aMaxI, bMinI) && moveForAxis < 0f) || (Maths.Approximately(aMinI, bMaxI) && moveForAxis > 0f))
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

    public bool CollideRay(Ray3D ray, out Vector3 collisionPoint, out Vector3 surfaceNormal)
    {
        collisionPoint = Vector3.Zero;
        surfaceNormal = Vector3.Zero;

        Vector3 closestIntersection = Vector3.Zero;
        float closestIntersectionDist = float.PositiveInfinity;

        Dictionary<Vector2, ChunkT> chunks = GetChunksInState(ChunkState.HasMesh);
        foreach (KeyValuePair<Vector2, ChunkT> item in chunks)
        {
            ChunkT chunk = item.Value;
            if (chunk.Bounds.IsEmpty) continue;
            if (!ray.IntersectWithCube(chunk.Bounds, out Vector3 _, out Vector3 __)) continue;

            if (chunk.Colliders == null) continue; // todo: vertices based chunks? triangle collision

            foreach (Cube other in chunk.Colliders)
            {
                if (ray.IntersectWithCube(other, out Vector3 colPoint, out Vector3 colSurfaceNormal))
                {
                    float dist = Vector3.Distance(colPoint, ray.Start);
                    if (dist < closestIntersectionDist)
                    {
                        closestIntersection = colPoint;
                        closestIntersectionDist = dist;
                        surfaceNormal = colSurfaceNormal;
                    }
                }
            }
        }

        collisionPoint = closestIntersection;
        return closestIntersectionDist != float.PositiveInfinity;
    }

    #endregion

    #region Rendering

    protected List<ChunkT> _renderThisPass = new(32);

    public virtual void Render(Renderer c, Frustum frustum)
    {
        // Gather chunks to render this pass.
        _renderThisPass.Clear();
        Dictionary<Vector2, ChunkT> chunks = GetChunksInState(ChunkState.HasGPUData);
        foreach (KeyValuePair<Vector2, ChunkT> item in chunks)
        {
            ChunkT chunk = item.Value;
            if (chunk.GPUUploadedVersion != -1)
            {
                Cube bounds = chunk.Bounds;
                Assert(!bounds.IsEmpty);
                if (frustum.IntersectsOrContainsCube(bounds)) // Frustum cull
                {
                    _renderThisPass.Add(chunk);
                }
            }
        }

        if (_renderThisPass.Count > 0)
        {
            Engine.Renderer.FlushRenderStream();

            RenderState oldState = c.CurrentState.Clone();

            MeshMaterial material = GetMeshMaterial();
            c.SetState(material.State);
            c.CurrentShader.SetUniformInt("diffuseTexture", 0);

            Texture diffuseTexture = material.GetDiffuseTexture();
            Texture.EnsureBound(diffuseTexture.Pointer);

            SetupShaderState(c.CurrentShader);

            foreach (ChunkT chunkToRender in _renderThisPass)
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
        }
    }

    #endregion

    #region Protected API

    [DontSerialize]
    public ChunkStreamManager ChunkStreamManager { get; set; }

    // These events should only really trigger in the editor or if some game is dynamically editing the terrain.

    protected override void OnChunkRemoved(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkRemoved(chunkCoord, newChunk);
        if (!Initialized) return;

        // todo: demote chunk to data only before freeing
        RemoveChunkFromLoadedList(chunkCoord);
        VertexDataAllocation.FreeAllocated(ref newChunk.VertexMemory);
        GPUMemoryAllocator.FreeBuffer(newChunk.GPUVertexMemory);
    }

    protected abstract void UpdateChunkVertices(Vector2 chunkCoord, ChunkT chunk);

    protected abstract MeshMaterial GetMeshMaterial();

    protected virtual void SetupShaderState(ShaderProgram shader)
    {
        // nop
    }

    #endregion

    #region Debug

    protected virtual void SetupDebugVisualizations()
    {
        EngineEditor.AddEditorVisualization(this, "View Chunk Boundaries", (c) =>
        {
            foreach (ChunkT chunkToRender in _renderThisPass)
            {
                chunkToRender.Bounds.RenderOutline(c);
            }
        });

        EngineEditor.AddEditorVisualization(this, "View Chunk Colliders", (c) =>
        {
            Vector3 cameraPos = c.Camera.Position;
            foreach (ChunkT chunkToRender in _renderThisPass)
            {
                if (chunkToRender.Colliders == null) continue;
                foreach (var collider in chunkToRender.Colliders)
                {
                    if (Vector2.Distance(cameraPos.ToVec2(), collider.Origin.ToVec2()) > 10f) continue;

                    collider.RenderOutline(c, Color.Blue);
                }
            }
        });

        EngineEditor.AddEditorVisualization(this, "View Tile Vertex Normals", (c) =>
        {
            Vector2 cameraPos = c.Camera.Position.ToVec2();

            foreach (ChunkT chunkToRender in _renderThisPass)
            {
                if (Vector2.Distance(chunkToRender.Bounds.Origin.ToVec2(), cameraPos) > 15f) continue;

                VertexDataAllocation vertices = chunkToRender.VertexMemory;
                Span<VertexData_Pos_UV_Normal_Color> verticesSpan = vertices.GetAsSpan<VertexData_Pos_UV_Normal_Color>();
                for (int i = 0; i < verticesSpan.Length; i++)
                {
                    ref VertexData_Pos_UV_Normal_Color vert = ref verticesSpan[i];
                    if (vert.Normal == Graphics.Renderer.Up) continue;

                    c.RenderLine(vert.Position, vert.Position + vert.Normal * 0.5f, Color.Red, 0.05f);
                }
            }
        });
    }

    #endregion

    public bool IsTileInBounds(Vector2 tile)
    {
        ChunkT? chunk = GetChunkAt(tile, out Vector2 _);
        return chunk != null && chunk.CanBeSimulated;
    }
}