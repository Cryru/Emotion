#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Threading;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Primitives.Grids;
using Emotion.World.Terrain;
using OpenGL;
using VoxelMeshGridChunk = Emotion.Game.World.Terrain.MeshGridStreaming.MeshGridStreamableChunk<uint, uint>;

namespace Emotion.Game.World.Terrain;

public class VoxelMeshTerrainGrid : MeshGrid<uint, VoxelMeshGridChunk, uint>
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

    protected override VoxelMeshGridChunk InitializeNewChunk()
    {
        VoxelMeshGridChunk newChunk = new VoxelMeshGridChunk();
        uint[] newChunkData = new uint[(int)(ChunkSize3D.X * ChunkSize3D.Y * ChunkSize3D.Z)];
        newChunk.SetRawData(newChunkData);

        return newChunk;
    }

    #region 3D Grid Helpers

    protected void SetAtForChunk(VoxelMeshGridChunk chunk, Vector3 position, uint value)
    {
        uint[] data = chunk.GetRawData();
        int idx = GridHelpers.GetCoordinate1DFrom3D(position, ChunkSize3D);
        data[idx] = value;
    }

    protected uint GetAtForChunk(VoxelMeshGridChunk chunk, Vector3 position)
    {
        uint[] data = chunk.GetRawData();
        int idx = GridHelpers.GetCoordinate1DFrom3D(position, ChunkSize3D);
        return data[idx];
    }

    public void SetAt(Vector3 position, uint value)
    {
        VoxelMeshGridChunk? chunk = GetChunkAt(position.ToVec2(), out Vector2 chunkCoord, out Vector2 relativeCoord);
        if (chunk == null) return;
        SetAtForChunk(chunk, relativeCoord.ToVec3(position.Z), value);
        OnChunkChanged(chunkCoord, chunk);
    }

    public uint GetAt(Vector3 position)
    {
        VoxelMeshGridChunk? chunk = GetChunkAt(position.ToVec2(), out Vector2 relativeCoord);
        if (chunk == null) return default;
        return GetAtForChunk(chunk, relativeCoord.ToVec3(position.Z));
    }

    public bool ExpandingSetAt(Vector3 position, uint value)
    {
        Assert(position == position.Floor());
        if (position.Z >= ChunkSize3D.Z)
            return false;

        Vector2 pos2D = position.ToVec2();
        Vector2 chunkCoord = GetChunkCoordinateOfValueCoordinate(pos2D);

        uint defVal = default;
        bool isDelete = defVal.Equals(value);

        VoxelMeshGridChunk? chunk = GetChunkAt(pos2D, out Vector2 relativeLocation);
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
        VoxelMeshGridChunk newChunk = InitializeNewChunk();
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
        VoxelMeshGridChunk? chunk = GetChunkAt(tilePos, out Vector2 relativeCoord);
        if (chunk == null) return 0;
        for (int z = (int)ChunkSize3D.Z - 1; z >= 0; z--)
        {
            uint val = GetAtForChunk(chunk, relativeCoord.ToVec3(z));
            if (val != 0)
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

    public Cube GetCubeOfTilePos(Vector3 tilePos)
    {
        return new Cube(tilePos * TileSize3D, TileSize3D / 2f);
    }

    #endregion

    #region Rendering - Index

    // One index buffer is used for all chunks, and they are all the same length.

    protected uint[]? _indices; // Collision only
    protected int _indicesLength;
    protected IndexBuffer? _indexBuffer;

    protected void PrepareIndexBuffer()
    {
        Assert(GLThread.IsGLThread());

        int quads = (int)(ChunkSize3D.X * ChunkSize3D.Y * ChunkSize3D.Z) * 6;
        int stride = (int)ChunkSize.X + 1;
        int indexCount = quads * 6;

        _indexBuffer ??= new IndexBuffer(DrawElementsType.UnsignedInt);

        uint[] indices = new uint[indexCount];
        IndexBuffer.FillQuadIndices<uint>(indices, 0);
        _indexBuffer.Upload(indices);
        _indices = indices;
        _indicesLength = indexCount;
    }

    #endregion

    #region Rendering

    protected override void SetupShaderState(ShaderProgram shader)
    {
        shader.SetUniformVector2("brushWorldSpace", Vector2.Zero);
        shader.SetUniformFloat("brushRadius", 0);
    }

    protected virtual bool IsEmpty(uint tileData)
    {
        return tileData == 0;
    }

    protected virtual void SetVoxelFaceUV(
        CubeFace face, Span<VertexData_Pos_UV_Normal_Color> vertices,
        int coord1D, uint voxelId,
        uint topSample, uint leftSample, uint rightsample,
        uint frontSample, uint backSample, uint bottomSample
    )
    {

    }

    private int RunChunkMeshGeneration(Vector2 chunkWorldOffset, uint[] dataMe, Span<VertexData_Pos_UV_Normal_Color> vertices, Vector3 tileSize3D)
    {
        bool justCount = vertices.IsEmpty;
        Vector3 halfSize = tileSize3D / 2f;

        int vIdx = 0;
        for (int z = 0; z < ChunkSize3D.Z; z++)
        {
            for (int y = 0; y < ChunkSize.Y; y++)
            {
                for (int x = 0; x < ChunkSize.X; x++)
                {
                    Vector3 tileCoord = new Vector3(x, y, z);

                    int dataCoord = GridHelpers.GetCoordinate1DFrom3D(tileCoord, ChunkSize3D);
                    uint voxelId = dataMe[dataCoord];
                    if (IsEmpty(voxelId)) continue;

                    Vector3 worldPos = chunkWorldOffset.ToVec3() + tileCoord * tileSize3D;

                    uint topSample = 0;
                    if (z != ChunkSize3D.Z - 1)
                    {
                        Vector3 topTileCoordinates = new Vector3(x, y, z + 1);
                        int topCoord = GridHelpers.GetCoordinate1DFrom3D(topTileCoordinates, ChunkSize3D);
                        topSample = dataMe[topCoord];
                    }

                    // Top (Z+)
                    if (IsEmpty(topSample))
                    {
                        if (justCount)
                        {
                            vIdx += 4;
                        }
                        else
                        {
                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, 1);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, 1);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, 1);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, 1);
                                vData.Normal = Graphics.Renderer.Up;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            SetVoxelFaceUV(
                                CubeFace.PositiveZ, vertices.Slice(vIdx - 4), dataCoord, voxelId,
                                topSample, 0, 0, 0, 0, 0
                            );
                        }
                    }

                    // Bottom (Z-)
                    bool makeBottomFace = z == 0;
                    if (!makeBottomFace)
                    {
                        Vector3 bottomTileCoordinates = new Vector3(x, y, z - 1);
                        int bottomCoord = GridHelpers.GetCoordinate1DFrom3D(bottomTileCoordinates, ChunkSize3D);
                        uint bottomSample = dataMe[bottomCoord];
                        makeBottomFace = IsEmpty(bottomSample);
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
                                CubeFace.NegativeZ, vertices.Slice(vIdx - 4), dataCoord, voxelId,
                                topSample, 0, 0, 0, 0, 0
                            );
                        }
                    }

                    // Back (X-)
                    bool makeBackface = x == 0;
                    if (!makeBackface)
                    {
                        Vector3 backTileCoordinates = new Vector3(x - 1, y, z);
                        int backCoord = GridHelpers.GetCoordinate1DFrom3D(backTileCoordinates, ChunkSize3D);
                        uint backSample = dataMe[backCoord];
                        makeBackface = IsEmpty(backSample);
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
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, 1);
                                vData.Normal = -Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, 1);
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
                                CubeFace.NegativeX, vertices.Slice(vIdx - 4), dataCoord, voxelId,
                                topSample, 0, 0, 0, 0, 0
                            );
                        }
                    }

                    // Front (X+)
                    bool makeFrontFace = x == ChunkSize.X - 1;
                    if (!makeFrontFace)
                    {
                        Vector3 frontTileCoordinates = new Vector3(x + 1, y, z);
                        int frontCoord = GridHelpers.GetCoordinate1DFrom3D(frontTileCoordinates, ChunkSize3D);
                        uint frontSample = dataMe[frontCoord];
                        makeFrontFace = IsEmpty(frontSample);
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
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, 1);
                                vData.Normal = Graphics.Renderer.Forward;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, 1);
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
                                CubeFace.PositiveX, vertices.Slice(vIdx - 4), dataCoord, voxelId,
                                topSample, 0, 0, 0, 0, 0
                            );
                        }
                    }

                    // Left (Y-)
                    bool makeLeftFace = y == 0;
                    if (!makeLeftFace)
                    {
                        Vector3 leftTileCoordinates = new Vector3(x, y - 1, z);
                        int leftCoord = GridHelpers.GetCoordinate1DFrom3D(leftTileCoordinates, ChunkSize3D);
                        uint leftSample = dataMe[leftCoord];
                        makeLeftFace = IsEmpty(leftSample);
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
                                vData.Position = worldPos + halfSize * new Vector3(-1, -1, 1);
                                vData.Normal = -Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(1, -1, 1);
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
                                CubeFace.NegativeY, vertices.Slice(vIdx - 4), dataCoord, voxelId,
                                topSample, 0, 0, 0, 0, 0
                            );
                        }
                    }

                    // Right (Y+)
                    bool makeRightFace = y == ChunkSize.Y - 1;
                    if (!makeRightFace)
                    {
                        Vector3 rightTileCoordinates = new Vector3(x, y + 1, z);
                        int rightCoord = GridHelpers.GetCoordinate1DFrom3D(rightTileCoordinates, ChunkSize3D);
                        uint rightSample = dataMe[rightCoord];
                        makeRightFace = IsEmpty(rightSample);
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
                                vData.Position = worldPos + halfSize * new Vector3(1, 1, 1);
                                vData.Normal = Graphics.Renderer.Right;
                                vData.Color = Color.WhiteUint;
                                vData.UV = new Vector2(0, 0);
                                vIdx++;
                            }

                            {
                                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                                vData.Position = worldPos + halfSize * new Vector3(-1, 1, 1);
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
                                CubeFace.PositiveY, vertices.Slice(vIdx - 4), dataCoord, voxelId,
                                topSample, 0, 0, 0, 0, 0
                            );
                        }
                    }
                }
            }
        }

        return vIdx;
    }

    protected override void UpdateChunkVertices(Vector2 chunkCoord, VoxelMeshGridChunk chunk, bool propagate = true)
    {
        // We already have the latest version of this
        if (chunk.VerticesGeneratedForVersion == chunk.ChunkVersion && chunk.VertexMemory.Allocated) return;

        Vector2 tileSize = TileSize;
        Vector3 tileSize3D = TileSize3D;
        Vector2 halfTileSize = tileSize / 2f;
        Vector3 halfTileSize3D = tileSize3D / 2f;

        Vector2 chunkWorldSize = ChunkSize * tileSize;
        Vector2 chunkWorldOffset = chunkCoord * chunkWorldSize;

        // Get my data
        uint[] dataMe = chunk.GetRawData() ?? Array.Empty<uint>();

        int verticesToAllocate = RunChunkMeshGeneration(chunkWorldOffset, dataMe, Span<VertexData_Pos_UV_Normal_Color>.Empty, tileSize3D);
        verticesToAllocate = (int)Math.Ceiling(verticesToAllocate / 1000.0f) * 1000; // Round to thousandth
        Span<VertexData_Pos_UV_Normal_Color> vertices = chunk.ResizeVertexMemoryAndGetSpan(chunkCoord, verticesToAllocate);

        int verticesUsed = RunChunkMeshGeneration(chunkWorldOffset, dataMe, vertices, tileSize3D);

        // Update colliders
        // todo: if our voxels are just cubes we can simulate collisions by just using
        // the chunk data instead of building actual cube meshes, though this way we encode
        // where the checks are needed as opposed to some structure to allow ray skipping
        chunk.Colliders ??= new List<Cube>();
        chunk.Colliders.Clear();
        for (int y = 0; y < ChunkSize.Y; y++)
        {
            for (int x = 0; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                Vector2 tileOrigin = chunkWorldOffset + tileCoord * tileSize;

                int columnStartZ = -1;
                for (int z = (int)ChunkSize3D.Z - 1; z >= -1; z--)
                {
                    bool isEmpty;
                    if (z == -1)
                    {
                        isEmpty = true;
                    }
                    else
                    {
                        uint val = GetAtForChunk(chunk, tileCoord.ToVec3(z));
                        isEmpty = IsEmpty(val);
                    }

                    bool columnStarted = columnStartZ != -1;
                    if (isEmpty && columnStarted)
                    {
                        int columnSize = columnStartZ - z;
                        float columnStartWorldSpace = columnStartZ * tileSize3D.Z;
                        float columnSizeWorldSpace = columnSize * tileSize3D.Z;

                        chunk.Colliders.Add(
                            new Cube(tileOrigin.ToVec3(columnStartWorldSpace - columnSizeWorldSpace / 2f + halfTileSize3D.Z), halfTileSize.ToVec3(columnSizeWorldSpace / 2f))
                        );

                        columnStartZ = -1;
                    }
                    else if (!isEmpty && !columnStarted)
                    {
                        columnStartZ = z;
                    }
                }
            }
        }

        // The indices used are the same for all chunks, just the length is different
        AssertNotNull(_indices);
        AssertNotNull(_indexBuffer);
        chunk.SetIndices(_indices, _indexBuffer, (int)(verticesUsed / 4f * 6f));

        chunk.GPUDirty = true;
        chunk.VerticesGeneratedForVersion = chunk.ChunkVersion;
        chunk.VerticesUsed = (uint)verticesUsed;

        Vector3 chunkSizeWorld3 = ChunkSize3D * tileSize3D;
        chunk.Bounds = Cube.FromCenterAndSize(
            chunkWorldOffset.ToVec3() + chunkSizeWorld3 / 2f - halfTileSize3D,
            chunkSizeWorld3
        );
    }

    [DontSerialize]
    public MeshMaterial TerrainMeshMaterial = new MeshMaterial()
    {
        Name = "TerrainChunkMaterial",
        State =
        {
            FaceCulling = true,
            FaceCullingBackFace = true,
            ShaderName = "Shaders3D/TerrainShader.glsl"
        }
    };

    protected override MeshMaterial GetMeshMaterial()
    {
        return TerrainMeshMaterial;
    }

    #endregion
}