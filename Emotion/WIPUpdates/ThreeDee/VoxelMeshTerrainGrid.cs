using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Game.World2D;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.Rendering;
using System;
using VoxelMeshGridChunk = Emotion.WIPUpdates.Grids.VersionedGridChunk<uint>;

#nullable enable

namespace Emotion.WIPUpdates.ThreeDee;

public class VoxelMeshTerrainGrid : MeshGrid<uint, VoxelMeshGridChunk, uint>
{
    public Vector3 TileSize3D { get; init; }

    public Vector3 ChunkSize3D { get; init; }

    public VoxelMeshTerrainGrid(Vector3 tileSize, float chunkHeight, float chunkSize) : base(tileSize.ToVec2(), chunkSize)
    {
        TileSize3D = tileSize;
        ChunkSize3D = new Vector3(chunkSize, chunkSize, chunkHeight);
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
    protected Vector2 _indexBufferChunkSize;
    protected int _indicesLength;
    protected IndexBuffer? _indexBuffer;

    protected void PrepareIndexBuffer()
    {
        Assert(GLThread.IsGLThread());

        int quads = (int)(ChunkSize3D.X * ChunkSize3D.Y * ChunkSize3D.Z) * 6;
        int stride = (int)ChunkSize.X + 1;
        int indexCount = quads * 6;

        _indexBuffer ??= new IndexBuffer(OpenGL.DrawElementsType.UnsignedInt);

        uint[] indices = new uint[indexCount];
        IndexBuffer.FillQuadIndices<uint>(indices, 0);
        _indexBuffer.Upload(indices);
        _indices = indices;
        _indexBufferChunkSize = ChunkSize;
        _indicesLength = indexCount;
    }

    #endregion

    #region Rendering

    protected override void SetupShaderState(ShaderProgram shader)
    {
        shader.SetUniformVector2("brushWorldSpace", Vector2.Zero);
        shader.SetUniformFloat("brushRadius", 0);
    }

    public override void Render(RenderComposer c, Frustum frustum)
    {
        if (ChunkSize != _indexBufferChunkSize || _indexBuffer == null)
            PrepareIndexBuffer();

        base.Render(c, frustum);
    }

    protected virtual bool IsEmpty(uint tileData)
    {
        return tileData == 0;
    }

    protected override void UpdateChunkVertices(Vector2 chunkCoord, MeshGridChunkRuntimeCache renderCache, bool propagate = true)
    {
        VoxelMeshGridChunk chunk = renderCache.Chunk;

        // We already have the latest version of this
        if (renderCache.VerticesGeneratedForVersion == chunk.ChunkVersion && renderCache.VertexMemory.Allocated) return;

        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = TileSize / 2f;
        Vector3 tileSize3D = tileSize.ToVec3(tileSize.X);
        Vector2 chunkWorldSize = ChunkSize * tileSize;

        Vector2 chunkWorldOffset = chunkCoord * chunkWorldSize;

        // Get my data
        uint[] dataMe = chunk.GetRawData() ?? Array.Empty<uint>();

        int vertexCount = (int)(dataMe.Length * 24);
        var vertices = renderCache.EnsureVertexMemoryAndGetSpan(chunkCoord, vertexCount);

        int vIdx = 0;
        for (int z = 0; z < ChunkSize3D.Z; z++)
        {
            for (int y = 0; y < ChunkSize.Y; y++)
            {
                for (int x = 0; x < ChunkSize.X; x++)
                {
                    Vector3 tileCoord = new Vector3(x, y, z);

                    int dataCoord = GridHelpers.GetCoordinate1DFrom3D(tileCoord, ChunkSize3D);
                    uint sample = dataMe[dataCoord];
                    if (IsEmpty(sample)) continue;

                    Vector3 worldPos = chunkWorldOffset.ToVec3() + (tileCoord * tileSize3D);
                    Vector3 halfSize = tileSize3D / 2f;

                    // Top (Z+)
                    bool shouldDrawTopFace = z == ChunkSize3D.Z - 1;
                    if (!shouldDrawTopFace)
                    {
                        Vector3 topTileCoordinates = new Vector3(x, y, z + 1);
                        int topCoord = GridHelpers.GetCoordinate1DFrom3D(topTileCoordinates, ChunkSize3D);
                        uint topSample = dataMe[topCoord];
                        shouldDrawTopFace = IsEmpty(topSample);

                    }
                    if (shouldDrawTopFace)
                    {
                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, 1, 1));
                            vData.Normal = RenderComposer.Up;
                            vData.Color = Color.PrettyGreen.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, 1, 1));
                            vData.Normal = RenderComposer.Up;
                            vData.Color = Color.PrettyGreen.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, -1, 1));
                            vData.Normal = RenderComposer.Up;
                            vData.Color = Color.PrettyGreen.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, -1, 1));
                            vData.Normal = RenderComposer.Up;
                            vData.Color = Color.PrettyGreen.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }
                    }

                    // Bottom (Z-)
                    bool shouldDrawBottonFace = z == 0;
                    if (!shouldDrawBottonFace)
                    {
                        Vector3 bottomTileCoordinates = new Vector3(x, y, z - 1);
                        int bottomCoord = GridHelpers.GetCoordinate1DFrom3D(bottomTileCoordinates, ChunkSize3D);
                        uint bottomSample = dataMe[bottomCoord];
                        shouldDrawBottonFace = IsEmpty(bottomSample);
                    }
                    if (shouldDrawBottonFace)
                    {
                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, -1, -1));
                            vData.Normal = -RenderComposer.Up;
                            vData.Color = Color.WhiteUint;
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, -1, -1));
                            vData.Normal = -RenderComposer.Up;
                            vData.Color = Color.WhiteUint;
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, 1, -1));
                            vData.Normal = -RenderComposer.Up;
                            vData.Color = Color.WhiteUint;
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, 1, -1));
                            vData.Normal = -RenderComposer.Up;
                            vData.Color = Color.WhiteUint;
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }
                    }

                    // Back (X-)
                    bool shouldDrawBackFace = x == 0;
                    if (!shouldDrawBackFace)
                    {
                        Vector3 backTileCoordinates = new Vector3(x - 1, y, z);
                        int backCoord = GridHelpers.GetCoordinate1DFrom3D(backTileCoordinates, ChunkSize3D);
                        uint backSample = dataMe[backCoord];
                        shouldDrawBackFace = IsEmpty(backSample);
                    }
                    if (shouldDrawBackFace)
                    {
                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, 1, 1));
                            vData.Normal = -RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, -1, 1));
                            vData.Normal = -RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, -1, -1));
                            vData.Normal = -RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, 1, -1));
                            vData.Normal = -RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }
                    }

                    // Front (X+)
                    bool shouldDrawFrontFace = x == ChunkSize.X - 1;
                    if (!shouldDrawFrontFace)
                    {
                        Vector3 frontTileCoordinates = new Vector3(x + 1, y, z);
                        int frontCoord = GridHelpers.GetCoordinate1DFrom3D(frontTileCoordinates, ChunkSize3D);
                        uint frontSample = dataMe[frontCoord];
                        shouldDrawFrontFace = IsEmpty(frontSample);
                    }
                    if (shouldDrawFrontFace)
                    {
                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, -1, 1));
                            vData.Normal = RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, 1, 1));
                            vData.Normal = RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, 1, -1));
                            vData.Normal = RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, -1, -1));
                            vData.Normal = RenderComposer.Forward;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }
                    }

                    // Left (Y-)
                    bool shouldDrawLeftFace = y == 0;
                    if (!shouldDrawLeftFace)
                    {
                        Vector3 leftTileCoordinates = new Vector3(x, y - 1, z);
                        int leftCoord = GridHelpers.GetCoordinate1DFrom3D(leftTileCoordinates, ChunkSize3D);
                        uint leftSample = dataMe[leftCoord];
                        shouldDrawLeftFace = IsEmpty(leftSample);
                    }
                    if (shouldDrawLeftFace)
                    {
                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, -1, 1));
                            vData.Normal = -RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, -1, 1));
                            vData.Normal = -RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, -1, -1));
                            vData.Normal = -RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, -1, -1));
                            vData.Normal = -RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }
                    }

                    // Right (Y+)
                    bool shouldDrawRightFace = y == ChunkSize.Y - 1;
                    if (!shouldDrawRightFace)
                    {
                        Vector3 rightTileCoordinates = new Vector3(x, y + 1, z);
                        int rightCoord = GridHelpers.GetCoordinate1DFrom3D(rightTileCoordinates, ChunkSize3D);
                        uint rightSample = dataMe[rightCoord];
                        shouldDrawRightFace = IsEmpty(rightSample);
                    }
                    if (shouldDrawRightFace)
                    {
                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, 1, 1));
                            vData.Normal = RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, 1, 1));
                            vData.Normal = RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(-1, 1, -1));
                            vData.Normal = RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }

                        {
                            ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                            vData.Position = worldPos + (halfSize * new Vector3(1, 1, -1));
                            vData.Normal = RenderComposer.Right;
                            vData.Color = Color.PrettyBrown.ToUint();
                            vData.UV = new Vector2(0, 0);
                            vIdx++;
                        }
                    }
                }
            }
        }

        // Update colliders
        renderCache.Colliders ??= new List<Cube>();
        renderCache.Colliders.Clear();
        for (int y = 0; y < ChunkSize.Y; y++)
        {
            for (int x = 0; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                Vector2 tileOrigin = chunkWorldOffset + (tileCoord * tileSize);
                float z = GetHeightAt(tileOrigin);
                if (z == -1) continue;

                renderCache.Colliders.Add(new Cube(tileOrigin.ToVec3(z), TileSize3D / 2f));
            }
        }

        // The indices used are the same for all chunks, just the length is different
        AssertNotNull(_indices);
        AssertNotNull(_indexBuffer);
        renderCache.SetIndices(_indices, _indexBuffer, (int)(vIdx / 4f * 6f));

        renderCache.GPUDirty = true;
        renderCache.VerticesGeneratedForVersion = chunk.ChunkVersion;

        Vector3 chunkSizeWorld3 = ChunkSize3D * TileSize3D;
        renderCache.Bounds = Cube.FromCenterAndSize(
            (chunkWorldOffset.ToVec3() + chunkSizeWorld3 / 2f) - TileSize3D / 2f,
            chunkSizeWorld3
        );
    }

    [DontSerialize]
    public MeshMaterial TerrainMeshMaterial = new MeshMaterial()
    {
        Name = "TerrainChunkMaterial",
        Shader = "Shaders3D/TerrainShader.glsl",
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