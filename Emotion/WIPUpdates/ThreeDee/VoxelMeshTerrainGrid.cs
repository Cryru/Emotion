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
        VoxelMeshGridChunk? chunk = GetChunkAt(worldSpace, out Vector2 relativeCoord);
        if (chunk == null) return 0;
        for (int z = (int)ChunkSize3D.Z - 1; z >= 0; z--)
        {
            uint val = GetAtForChunk(chunk, relativeCoord.ToVec3(z));
            if (val != 0)
                return z;
        }

        return 0;
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

    public override void Render(RenderComposer c, Rectangle clipArea)
    {
        if (ChunkSize != _indexBufferChunkSize || _indexBuffer == null)
            PrepareIndexBuffer();

        base.Render(c, clipArea);
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

        Vector2 chunkWorldOffset = (chunkCoord * chunkWorldSize);// + tileSize;

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
                    float sample = dataMe[dataCoord];
                    if (sample == 0) continue;

                    Vector3 worldPos = chunkWorldOffset.ToVec3() + (tileCoord * tileSize3D);
                    Vector3 halfSize = tileSize3D / 2f;

                    // Top (Z+)
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

                    // Bottom (Z-)
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

                    // Back (X-)
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

                    // Front (X+)
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

                    // Left (Y-)
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

                    // Right (Y+)
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

        // Update colliders
        renderCache.Colliders ??= new List<Cube>();
        renderCache.Colliders.Clear();
        for (int y = 0; y < ChunkSize.Y; y++)
        {
            for (int x = 0; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                Vector2 tileOrigin = chunkWorldOffset + (tileCoord * tileSize);
                float height = GetHeightAt(tileOrigin);
                float heightWorldSpace = (height + 1) * TileSize3D.Z;

                renderCache.Colliders.Add(
                    Cube.FromCenterAndSize(
                        tileOrigin.ToVec3(heightWorldSpace / 2f - TileSize3D.Z / 2f),
                        new Vector3(TileSize3D.X, TileSize3D.Y, heightWorldSpace)
                    )
                );
            }
        }

        // The indices used are the same for all chunks, just the length is different
        AssertNotNull(_indices);
        AssertNotNull(_indexBuffer);
        renderCache.SetIndices(_indices, _indexBuffer, (int)(vIdx / 4f * 6f));

        renderCache.GPUDirty = true;
        renderCache.VerticesGeneratedForVersion = chunk.ChunkVersion;

        Vector3 chunkSize3 = (ChunkSize3D * TileSize3D);
        renderCache.Bounds = Cube.FromCenterAndSize(chunkWorldOffset.ToVec3() + chunkSize3 / 2f + TileSize3D / 2f, chunkSize3 + TileSize3D);
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