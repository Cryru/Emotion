#nullable enable

using Emotion.Editor;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;
using Emotion.Graphics.Shading;
using Emotion.Primitives;
using Emotion.Primitives.Grids;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using OpenGL;

namespace Emotion.Game.World.Terrain;

public abstract partial class TerrainGridBase<T, ChunkT, IndexT> :
    ChunkedWorldSpaceGrid<T, ChunkT>,
    ITerrainGrid3D,
    ICustomReflectorMeta_ExtraMembers
    where ChunkT : MeshGridStreamableChunk<T, IndexT>, new()
    where T : unmanaged
    where IndexT : INumber<IndexT>
{
    public bool Initialized { get; private set; }

    public TerrainGridBase(Vector2 tileSize, float chunkSize) : base(tileSize, chunkSize)
    {
        // todo: do this in chunks rather than world units
        if (SimulationRange == 0 || RenderRange == 0)
        {
            int factor = (int)MathF.Max(tileSize.Y, tileSize.X);
            factor = Math.Max(factor, 1);

            SimulationRange = 512 * factor;
            RenderRange = 512 * factor;
        }
        if (SimulationRange < RenderRange)
            SimulationRange = RenderRange;
    }

    public virtual IEnumerator InitRoutine()
    {
        SetupDebugVisualizations();
        Initialized = true;
        yield break;
    }

    public virtual void Done()
    {
        EngineEditor.RemoveEditorVisualizations(this);
    }

    #region Serialization

    public static new ComplexTypeHandlerMemberBase[] GetExtraReflectorMembers()
    {
        return [
           new ComplexTypeHandlerMember<TerrainGridBase<T, ChunkT, IndexT>, Vector2>(
                "TileSize",
                static (ref TerrainGridBase<T, ChunkT, IndexT> p, Vector2 v) => p.TileSize = v,
                static (p) => p.TileSize
           )
       ];
    }

    #endregion

    #region API

    public void Update(float dt)
    {
        if (!Initialized) return;
        TickChunkStateUpdates(dt);
        TickChunkMeshUpdates();
        UpdateInternal(dt);
    }

    public abstract float GetHeightAt(Vector2 worldSpace);

    #endregion

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData)
    {
        Dictionary<Vector2, ChunkT> chunks = GetChunksInState(ChunkState.HasMesh);
        foreach (KeyValuePair<Vector2, ChunkT> item in chunks)
        {
            ChunkT chunk = item.Value;

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
                VertexDataAllocation? vertices = chunk.VertexMemory;
                if (vertices == null || !vertices.Allocated) continue;

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

            if (chunk.Colliders != null)
            {
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
            else
            {
                VertexDataAllocation? vertices = chunk.VertexMemory;
                if (vertices == null || !vertices.Allocated) continue;

                IndexT[]? indices = chunk.CPUIndexBuffer;
                if (indices == null) continue;

                if (ray.IntersectWithVertices(indices, chunk.IndicesUsed, vertices, out Vector3 colPoint, out Vector3 colSurfaceNormal, out _))
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

    public void Render(GameMap map, Renderer r, CameraCullingContext culling)
    {
        Frustum frustum = culling.Frustum;

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
                    _renderThisPass.Add(chunk);
            }
        }

        if (_renderThisPass.Count > 0)
        {
            Engine.Renderer.FlushRenderStream();

            bool transparentPass = false;

            RenderState oldState = r.CurrentState.Clone();

            MeshMaterial material = GetMeshMaterial();
            r.SetState(material.State);
            r.CurrentShader.SetUniformInt("diffuseTexture", 0);

            Texture diffuseTexture = material.GetDiffuseTexture();
            Texture.EnsureBound(diffuseTexture.Pointer);

            SetupShaderState(r.CurrentShader);

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

                if (chunkToRender.TransparentIndicesUsed != 0) transparentPass = true;
            }

            if (transparentPass)
            {
                foreach (ChunkT chunkToRender in _renderThisPass)
                {
                    IndexBuffer? indexBuffer = chunkToRender.IndexBuffer;
                    if (indexBuffer == null) continue;

                    GPUVertexMemory? gpuMem = chunkToRender.GPUVertexMemory;
                    if (gpuMem == null) continue;

                    VertexArrayObject.EnsureBound(gpuMem.VAO);
                    IndexBuffer.EnsureBound(indexBuffer.Pointer);

                    int indices = chunkToRender.TransparentIndicesUsed;
                    IntPtr indexOffset = (IntPtr)chunkToRender.IndicesUsed * indexBuffer.DataTypeSize;
                    Gl.DrawElements(PrimitiveType.Triangles, indices, indexBuffer.DataType, indexOffset);
                }
            }

            r.SetState(oldState);
        }
    }

    #endregion

    #region Protected API

    // These events should only trigger in the editor or if some game is dynamically editing the terrain.

    protected override void OnChunkRemoved(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkRemoved(chunkCoord, newChunk);
        if (!Initialized) return;

        // todo: demote chunk to data only before freeing
        RemoveChunkFromLoadedList(chunkCoord);
        VertexDataAllocation.FreeAllocated(newChunk.VertexMemory);
        GPUMemoryAllocator.FreeBuffer(newChunk.GPUVertexMemory);
    }

    protected abstract void UpdateChunkVertices(Vector2 chunkCoord, ChunkT chunk);

    protected abstract MeshMaterial GetMeshMaterial();

    protected virtual void SetupShaderState(ShaderProgram shader)
    {
        // nop
    }

    protected virtual void UpdateInternal(float dt)
    {

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
            Cube cameraPosCube = new Cube(cameraPos.ToVec3(0), new Vector3(100f, 100f, 1000f));

            foreach (ChunkT chunkToRender in _renderThisPass)
            {
                if (!chunkToRender.Bounds.Intersects(cameraPosCube)) continue;

                VertexDataAllocation vertices = chunkToRender.VertexMemory;
                VertexDataFormat format = vertices.Format;

                int normalIdx = 0;
                foreach (Vector3 normal in vertices.ForEachNormal())
                {
                    Vector3 pos = vertices.GetVertexPositionAtIndex(normalIdx);
                    c.RenderLine(pos, pos + normal * 2f, normal == Graphics.Renderer.Up ? Color.Green : Color.Red, 0.1f);

                    normalIdx++;
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