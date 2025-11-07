#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Primitives.Grids;
using OpenGL;

namespace Emotion.Game.World.Terrain;

public class VoxelMeshTerrainGrid : VoxelMeshTerrainGrid<uint, MeshGridStreamableChunk<uint, uint>, uint>, IMapGrid
{
    public VoxelMeshTerrainGrid(Vector3 tileSize, float chunkHeight, float chunkSize) : base(tileSize, chunkHeight, chunkSize)
    {
    }
}

public class VoxelMeshTerrainGrid<TData, TChunk, TIndex> : MeshGrid<TData, TChunk, TIndex>
    where TData : struct, IEquatable<TData>
    where TChunk : MeshGridStreamableChunk<TData, TIndex>, new()
    where TIndex : unmanaged, INumber<TIndex>
{
    public Vector3 TileSize3D { get; init; }

    public Vector3 ChunkSize3D { get; init; }

    public VoxelMeshTerrainGrid(Vector3 tileSize, float chunkHeight, float chunkSize) : base(tileSize.ToVec2(), chunkSize)
    {
        TileSize3D = tileSize;
        ChunkSize3D = new Vector3(chunkSize, chunkSize, chunkHeight);
    }

    public override IEnumerator InitRuntimeDataRoutine()
    {
        List<Asset> assets = new List<Asset>();
        if (TerrainMeshMaterial.DiffuseTextureName != null)
            assets.Add(Engine.AssetLoader.ONE_Get<TextureAsset>(TerrainMeshMaterial.DiffuseTextureName));

        ThreadExecutionWaitToken indexBufferTask = GLThread.ExecuteOnGLThreadAsync(PrepareIndexBuffer);
        yield return base.InitRuntimeDataRoutine();

        yield return indexBufferTask;

        for (int i = 0; i < assets.Count; i++)
        {
            yield return assets[i];
        }
    }

    protected override TChunk InitializeNewChunk()
    {
        var newChunk = new TChunk();
        var newChunkData = new TData[(int)(ChunkSize3D.X * ChunkSize3D.Y * ChunkSize3D.Z)];
        newChunk.SetRawData(newChunkData);

        return newChunk;
    }

    #region 3D Grid Helpers

    protected void SetAtForChunk(TChunk chunk, Vector3 position, TData value)
    {
        TData[] data = chunk.GetRawData();
        int idx = GridHelpers.GetCoordinate1DFrom3D(position, ChunkSize3D);
        data[idx] = value;
    }

    protected TData GetAtForChunk(TChunk chunk, Vector3 position)
    {
        TData[] data = chunk.GetRawData();
        int idx = GridHelpers.GetCoordinate1DFrom3D(position, ChunkSize3D);
        return data[idx];
    }

    public void SetAt(Vector3 position, TData value)
    {
        TChunk? chunk = GetChunkAt(position.ToVec2(), out Vector2 chunkCoord, out Vector2 relativeCoord);
        if (chunk == null) return;
        SetAtForChunk(chunk, relativeCoord.ToVec3(position.Z), value);
        OnChunkChanged(chunkCoord, chunk);
    }

    public TData GetAt(Vector3 position)
    {
        TChunk? chunk = GetChunkAt(position.ToVec2(), out Vector2 relativeCoord);
        if (chunk == null) return default;
        return GetAtForChunk(chunk, relativeCoord.ToVec3(position.Z));
    }

    public bool ExpandingSetAt(Vector3 position, TData value)
    {
        Assert(position == position.Floor());
        if (position.Z >= ChunkSize3D.Z)
            return false;

        Vector2 pos2D = position.ToVec2();
        Vector2 chunkCoord = GetChunkCoordinateOfValueCoordinate(pos2D);

        uint defVal = default;
        bool isDelete = defVal.Equals(value);

        TChunk? chunk = GetChunkAt(pos2D, out Vector2 relativeLocation);
        if (chunk != null)
        {
            // Setting position in a chunk - easy peasy.
            SetAtForChunk(chunk, relativeLocation.ToVec3(position.Z), value);
            OnChunkChanged(chunkCoord, chunk);

            if (!isDelete) return false;

            // Compact the grid if the chunk is now empty.
            if (chunk.IsEmpty())
            {
                _chunks.Remove(chunkCoord);
                OnChunkRemoved(chunkCoord, chunk);
                return true;
            }

            return false;
        }

        // Trying to delete nothing
        if (isDelete) return false;

        // Initialize new chunk
        TChunk newChunk = InitializeNewChunk();
        _chunks.Add(chunkCoord, newChunk);
        OnChunkCreated(chunkCoord, newChunk);
        _chunkBoundsCacheValid = false;

        SetAtForChunk(newChunk, relativeLocation.ToVec3(position.Z), value);
        OnChunkChanged(chunkCoord, newChunk);
        return true;
    }

    #endregion

    #region Public API

    public override float GetHeightAt(Vector2 worldSpace)
    {
        Vector2 tilePos = GetTilePosOfWorldPos(worldSpace);
        TChunk? chunk = GetChunkAt(tilePos, out Vector2 relativeCoord);
        if (chunk == null) return 0;
        return GetHeightAtChunk(chunk, relativeCoord);
    }

    public float GetHeightAtChunk(TChunk? chunk, Vector2 tilePos)
    {
        for (int z = (int)ChunkSize3D.Z - 1; z >= 0; z--)
        {
            TData val = GetAtForChunk(chunk, tilePos.ToVec3(z));
            if (!IsEmpty(val))
                return z;
        }

        return -1;
    }

    public Vector3 GetTilePos3DOfWorldPos(Vector3 location)
    {
        float left = MathF.Round(location.X / TileSize.X);
        float top = MathF.Round(location.Y / TileSize.Y);
        float depth = MathF.Round(location.Z / TileSize3D.Z);

        return new Vector3(left, top, depth);
    }

    public Cube GetVoxelBoundsOfTile(Vector3 tilePos)
    {
        return new Cube(tilePos * TileSize3D, TileSize3D / 2f);
    }

    #endregion

    #region Rendering - Index

    // One index buffer is used for all chunks, and they are all the same length.

    protected TIndex[]? _indices; // Collision only
    protected int _indicesLength;
    protected IndexBuffer? _indexBuffer;

    protected void PrepareIndexBuffer()
    {
        Assert(GLThread.IsGLThread());

        int quads = (int)(ChunkSize3D.X * ChunkSize3D.Y * ChunkSize3D.Z) * 6;
        int stride = (int)ChunkSize.X + 1;
        int indexCount = quads * 6;

        _indexBuffer ??= new IndexBuffer(DrawElementsType.UnsignedInt);

        TIndex[] indices = new TIndex[indexCount];
        IndexBuffer.FillQuadIndices<TIndex>(indices, 0);
        _indexBuffer.Upload(indices);
        _indices = indices;
        _indicesLength = indexCount;
    }

    #endregion

    #region Gameplay Customization

    protected virtual float GetVoxelHeight(Vector2 chunkCoord, TChunk chunk, Vector3 inChunkTilePos, int oneDTilePos, TData tileData)
    {
        return 1f;
    }

    protected virtual bool IsEmpty(TData tileData)
    {
        return tileData is 0;
    }

    public virtual bool IsInteractive(TData tileData)
    {
        return !IsEmpty(tileData);
    }

    protected virtual bool IsVoxelTransparent(TData tileData)
    {
        return false;
    }

    #endregion

    #region Rendering

    protected override void SetupShaderState(ShaderProgram shader)
    {
        shader.SetUniformVector2("brushWorldSpace", Vector2.Zero);
        shader.SetUniformFloat("brushRadius", 0);
    }

    protected virtual void SetVoxelFaceUV(
        Vector2 chunkCoord,
        CubeFace face, Span<VertexData_Pos_UV_Normal_Color> vertices,
        int coord1D, TData voxelData,
        TData topSample, TData leftSample, TData rightsample,
        TData frontSample, TData backSample, TData bottomSample
    )
    {

    }

    private int RunChunkMeshGeneration(
        Vector2 chunkCoord,
        TChunk chunk,
        Vector2 chunkWorldOffset,
        TData[] dataMe,
        Span<VertexData_Pos_UV_Normal_Color> vertices,
        Vector3 tileSize3D,

        bool isTransparentPass,
        out bool hasTransparent
    )
    {
        hasTransparent = false;

        bool justCount = vertices.IsEmpty;
        Vector3 halfSize = tileSize3D / 2f;

        int oneDCoord = 0;
        int vIdx = 0;
        for (int z = 0; z < ChunkSize3D.Z; z++)
        {
            for (int y = 0; y < ChunkSize.Y; y++)
            {
                for (int x = 0; x < ChunkSize.X; x++)
                {
                    Vector3 tileCoord = new Vector3(x, y, z);

                    int dataCoord = oneDCoord; // optimized - GridHelpers.GetCoordinate1DFrom3D(tileCoord, ChunkSize3D);
                    oneDCoord++;

                    TData voxelData = dataMe[dataCoord];
                    if (IsEmpty(voxelData)) continue;

                    bool transparentVoxel = IsVoxelTransparent(voxelData);
                    if (transparentVoxel)
                    {
                        hasTransparent = true;
                        if (!isTransparentPass && !justCount)
                            continue;
                    }
                    else
                    {
                        if (isTransparentPass)
                            continue;
                    }

                    Vector3 worldPos = chunkWorldOffset.ToVec3() + tileCoord * tileSize3D;

                    float voxelHeight = GetVoxelHeight(chunkCoord, chunk, tileCoord, dataCoord, voxelData);
                    if (voxelHeight != -1)
                    {
                        voxelHeight = Maths.Map(voxelHeight, 0f, 1f, -1f, 1f);
                        voxelHeight = MathF.Max(voxelHeight, -0.99f);
                    }

                    // Top (Z+)
                    TData topSample = default;
                    if (z != ChunkSize3D.Z - 1)
                    {
                        Vector3 topTileCoordinates = new Vector3(x, y, z + 1);
                        int topCoord = GridHelpers.GetCoordinate1DFrom3D(topTileCoordinates, ChunkSize3D);
                        topSample = dataMe[topCoord];
                    }
                    if (IsEmpty(topSample) || IsVoxelTransparent(topSample))
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                chunkCoord,
                                CubeFace.PositiveZ, vertices.Slice(vIdx - 4), dataCoord, voxelData,
                                topSample, default, default, default, default, default
                            );
                        }
                    }

                    // Bottom (Z-)
                    bool makeBottomFace = z == 0;
                    if (!makeBottomFace)
                    {
                        Vector3 bottomTileCoordinates = new Vector3(x, y, z - 1);
                        int bottomCoord = GridHelpers.GetCoordinate1DFrom3D(bottomTileCoordinates, ChunkSize3D);
                        TData bottomSample = dataMe[bottomCoord];
                        makeBottomFace = IsEmpty(bottomSample) || IsVoxelTransparent(bottomSample);
                    }
                    if (makeBottomFace)
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, -1);
                                vData.Normal = -Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, -1);
                                vData.Normal = -Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, -1);
                                vData.Normal = -Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, -1);
                                vData.Normal = -Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                chunkCoord,
                                CubeFace.NegativeZ, vertices.Slice(vIdx - 4), dataCoord, voxelData,
                                topSample, default, default, default, default, default
                            );
                        }
                    }

                    // Back (X-)
                    bool makeBackface = x == 0;
                    if (!makeBackface)
                    {
                        Vector3 backTileCoordinates = new Vector3(x - 1, y, z);
                        int backCoord = GridHelpers.GetCoordinate1DFrom3D(backTileCoordinates, ChunkSize3D);
                        TData backSample = dataMe[backCoord];
                        makeBackface = IsEmpty(backSample) || IsVoxelTransparent(backSample);
                    }
                    if (makeBackface)
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, voxelHeight);
                                vData.Normal = -Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, voxelHeight);
                                vData.Normal = -Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, -1);
                                vData.Normal = -Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, -1);
                                vData.Normal = -Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                chunkCoord,
                                CubeFace.NegativeX, vertices.Slice(vIdx - 4), dataCoord, voxelData,
                                topSample, default, default, default, default, default
                            );
                        }
                    }

                    // Front (X+)
                    bool makeFrontFace = x == ChunkSize.X - 1;
                    if (!makeFrontFace)
                    {
                        Vector3 frontTileCoordinates = new Vector3(x + 1, y, z);
                        int frontCoord = GridHelpers.GetCoordinate1DFrom3D(frontTileCoordinates, ChunkSize3D);
                        TData frontSample = dataMe[frontCoord];
                        makeFrontFace = IsEmpty(frontSample) || IsVoxelTransparent(frontSample);
                    }
                    if (makeFrontFace)
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, -1);
                                vData.Normal = Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, -1);
                                vData.Normal = Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                chunkCoord,
                                CubeFace.PositiveX, vertices.Slice(vIdx - 4), dataCoord, voxelData,
                                topSample, default, default, default, default, default
                            );
                        }
                    }

                    // Left (Y-)
                    bool makeLeftFace = y == 0;
                    if (!makeLeftFace)
                    {
                        Vector3 leftTileCoordinates = new Vector3(x, y - 1, z);
                        int leftCoord = GridHelpers.GetCoordinate1DFrom3D(leftTileCoordinates, ChunkSize3D);
                        TData leftSample = dataMe[leftCoord];
                        makeLeftFace = IsEmpty(leftSample) || IsVoxelTransparent(leftSample);
                    }
                    if (makeLeftFace)
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, voxelHeight);
                                vData.Normal = -Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, voxelHeight);
                                vData.Normal = -Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, -1);
                                vData.Normal = -Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, -1);
                                vData.Normal = -Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                chunkCoord,
                                CubeFace.NegativeY, vertices.Slice(vIdx - 4), dataCoord, voxelData,
                                topSample, default, default, default, default, default
                            );
                        }
                    }

                    // Right (Y+)
                    bool makeRightFace = y == ChunkSize.Y - 1;
                    if (!makeRightFace)
                    {
                        Vector3 rightTileCoordinates = new Vector3(x, y + 1, z);
                        int rightCoord = GridHelpers.GetCoordinate1DFrom3D(rightTileCoordinates, ChunkSize3D);
                        TData rightSample = dataMe[rightCoord];
                        makeRightFace = IsEmpty(rightSample) || IsVoxelTransparent(rightSample);
                    }
                    if (makeRightFace)
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, voxelHeight);
                                vData.Normal = Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, -1);
                                vData.Normal = Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, -1);
                                vData.Normal = Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                chunkCoord,
                                CubeFace.PositiveY, vertices.Slice(vIdx - 4), dataCoord, voxelData,
                                topSample, default, default, default, default, default
                            );
                        }
                    }
                }
            }
        }

        return vIdx;
    }

    private ObjectPool<List<Cube>> _colliderPool = new ObjectPool<List<Cube>>();

    protected override void UpdateChunkVertices(Vector2 chunkCoord, TChunk chunk)
    {
        Vector2 tileSize = TileSize;
        Vector3 tileSize3D = TileSize3D;
        Vector2 halfTileSize = tileSize / 2f;
        Vector3 halfTileSize3D = tileSize3D / 2f;

        Vector2 chunkWorldSize = ChunkSize * tileSize;
        Vector2 chunkWorldOffset = chunkCoord * chunkWorldSize;

        // Get my data
        TData[] dataMe = chunk.GetRawData() ?? Array.Empty<TData>();

        // Count vertices and re-allocate memory if needed
        int verticesToAllocate = RunChunkMeshGeneration(
            chunkCoord,
            chunk,
            chunkWorldOffset,
            dataMe,
            Span<VertexData_Pos_UV_Normal_Color>.Empty,
            tileSize3D,

            false,
            out bool hasTransparent
        );

        verticesToAllocate = (int)Math.Ceiling(verticesToAllocate / 1000.0f) * 1000; // Round to thousandth so that reallocation is not too often
        Span<VertexData_Pos_UV_Normal_Color> vertices = ResizeVertexMemoryAndGetSpan(ref chunk.VertexMemory, chunkCoord, verticesToAllocate);
        int verticesUsed = RunChunkMeshGeneration(chunkCoord, chunk, chunkWorldOffset, dataMe, vertices, tileSize3D, false, out bool _);

        // Append transparent vertices
        int nonTransparentVertices = verticesUsed;
        int transparentVertices = 0;
        int transparentVerticesStart = verticesUsed;
        if (hasTransparent)
        {
            transparentVertices = RunChunkMeshGeneration(
               chunkCoord,
               chunk,
               chunkWorldOffset,
               dataMe,
               vertices.Slice(verticesUsed),
               tileSize3D,

               true,
               out bool _
            );
            verticesUsed += transparentVertices;
        }

        // Update colliders
        // todo: if our voxels are just cubes we can simulate collisions by just using
        // the chunk data instead of building actual cube meshes, though this way we encode
        // where the checks are needed as opposed to some structure to allow ray skipping
        List<Cube> colliders = _colliderPool.Get();
        colliders.Clear();
        for (int y = 0; y < ChunkSize.Y; y++)
        {
            for (int x = 0; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                Vector2 tileOrigin = chunkWorldOffset + tileCoord * tileSize;

                int columnStartZ = -1;
                for (int z = (int)ChunkSize3D.Z - 1; z >= -1; z--)
                {
                    bool walkThrough;
                    if (z == -1)
                    {
                        walkThrough = true;
                    }
                    else
                    {
                        TData val = GetAtForChunk(chunk, tileCoord.ToVec3(z));
                        walkThrough = !IsInteractive(val);
                    }

                    bool columnStarted = columnStartZ != -1;
                    if (walkThrough && columnStarted)
                    {
                        int columnSize = columnStartZ - z;
                        float columnStartWorldSpace = columnStartZ * tileSize3D.Z;
                        float columnSizeWorldSpace = columnSize * tileSize3D.Z;

                        colliders.Add(
                            new Cube(tileOrigin.ToVec3(columnStartWorldSpace - columnSizeWorldSpace / 2f + halfTileSize3D.Z), halfTileSize.ToVec3(columnSizeWorldSpace / 2f))
                        );

                        columnStartZ = -1;
                    }
                    else if (!walkThrough && !columnStarted)
                    {
                        columnStartZ = z;
                    }
                }
            }
        }

        List<Cube>? oldColliders = chunk.Colliders;
        chunk.Colliders = colliders;
        if (oldColliders != null)
            _colliderPool.Return(oldColliders);

        // The indices used are the same for all chunks, just the length is different
        AssertNotNull(_indices);
        AssertNotNull(_indexBuffer);

        int indicesUsed = (int)(nonTransparentVertices / 4f * 6f);
        chunk.SetIndices(_indices, _indexBuffer, indicesUsed);

        if (hasTransparent)
            chunk.TransparentIndicesUsed = (int)(transparentVertices / 4f * 6f);
        else
            chunk.TransparentIndicesUsed = 0;

        Vector3 chunkSizeWorld3 = ChunkSize3D * tileSize3D;
        chunk.Bounds = Cube.FromCenterAndSize(
            chunkWorldOffset.ToVec3() + chunkSizeWorld3 / 2f - halfTileSize3D,
            chunkSizeWorld3
        );

        chunk.VerticesUsed = (uint)verticesUsed;
        chunk.VerticesGeneratedForVersion = chunk.ChunkVersion;
    }

    [DontSerialize]
    public MeshMaterial TerrainMeshMaterial = new MeshMaterial()
    {
        Name = "TerrainChunkMaterial",
        State =
        {
            FaceCulling = true,
            FaceCullingBackFace = true,
            Shader = "Shaders3D/TerrainShader.glsl"
        }
    };

    protected override MeshMaterial GetMeshMaterial()
    {
        return TerrainMeshMaterial;
    }

    #endregion
}