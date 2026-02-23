#nullable enable

using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Primitives.Grids;
using System.Runtime.InteropServices;

namespace Emotion.Game.World.Terrain;

[StructLayout(LayoutKind.Sequential)]
public struct TerrainData
{
    public float Height;
    public Vector3 DBG_Pos;
}

public class TerrainChunk : MeshGridStreamableChunk<TerrainData, ushort>
{

}

public partial class TerrainMeshGridNew : MeshGrid<TerrainData, TerrainChunk, ushort>, IMapGrid
{
    public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");

    public TerrainMeshGridNew(Vector2 tileSize, float chunkSize) : base(tileSize, chunkSize)
    {
    }

    public IEnumerator InitRoutine(GameMap.GridFriendAdapter adapter)
    {
        GetMeshMaterial().EnsureAssetsLoaded();
        yield return GLThread.ExecuteOnGLThreadAsync(PrepareIndexBuffer);
        yield return base.InitRoutine();
    }

    #region Rendering

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

    protected override void SetupShaderState(ShaderProgram shader)
    {
        Vector2 brushWorldSpace = GetEditorBrush();
        shader.SetUniformVector2("brushWorldSpace", brushWorldSpace);
        shader.SetUniformFloat("brushRadius", _editorBrushSize);
    }

    #endregion

    #region Rendering - Mesh Generation

    protected override TerrainChunk InitializeNewChunk()
    {
        TerrainChunk newChunk = new();
        int vertices = (int)((ChunkSize.X * ChunkSize.Y) + ((ChunkSize.X + 1) * (ChunkSize.Y + 1)));

        TerrainData[] newChunkData = new TerrainData[vertices];
        newChunk.SetRawData(newChunkData);

        return newChunk;
    }

    protected override void UpdateChunkVertices(Vector2 chunkCoord, TerrainChunk chunk)
    {
#if DEBUG
        Interlocked.Increment(ref chunk.DEBUG_UpdateVerticesThreadCount);
        Assert(chunk.DEBUG_UpdateVerticesThreadCount == 1);
#endif

        int chunkVersionUpdateTo = chunk.ChunkVersion;
        bool propagate = chunkVersionUpdateTo != chunk.VerticesGeneratedForVersion;

        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = TileSize / 2f;
        Vector2 chunkWorldSize = ChunkSize * tileSize;
        Vector2 chunkWorldOffset = chunkCoord * chunkWorldSize;
        int evenRowWidth = (int)(ChunkSize.X + 1);
        int oddRowWidth = (int)ChunkSize.X;

        //Engine.Renderer.DbgAddCube(Cube.FromMinAndMax(chunkWorldOffset.ToVec3(0), (ChunkSize * TileSize).ToVec3(10) ));

        TerrainData[] data = chunk.GetRawData() ?? Array.Empty<TerrainData>();
        Span<VertexData_Pos_UV_Normal_Color> vertices = ResizeVertexMemoryAndGetSpan(ref chunk.VertexMemory, chunkCoord, data.Length);

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;

        Vector2 pen = Vector2.Zero;
        int verticesUsed = 0;
        for (int y = 0; y < ChunkSize.Y + ChunkSize.Y + 1; y++)
        {
            bool oddRow = y % 2 != 0;
            pen.X = oddRow ? halfTileSize.X : 0;

            for (int x = 0; x < (oddRow ? oddRowWidth : evenRowWidth); x++)
            {
                ref TerrainData terrainData = ref data[verticesUsed];
                ref VertexData_Pos_UV_Normal_Color vert = ref vertices[verticesUsed];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = new Vector3(0, 0, 1);
                vert.Color = oddRow ? Color.Blue.ToUint() : Color.WhiteUint;
                vert.UV = Vector2.Zero;

                terrainData.DBG_Pos = vert.Position;

                //Engine.Renderer.DbgAddPoint(vert.Position.ToVec2().ToVec3(0));
                //Engine.Renderer.DbgAddText(vert.Position.ToVec2().ToVec3(0), $"{verticesUsed}");

                min = verticesUsed == 0 ? vert.Position : Vector3.Min(min, vert.Position);
                max = verticesUsed == 0 ? vert.Position : Vector3.Max(max, vert.Position);

                pen.X += tileSize.X;

                verticesUsed++;
            }

            pen.Y += halfTileSize.Y;
        }
       
        GLThread.ExecuteOnGLThreadAsync(PrepareIndexBuffer);

        // The indices used are the same for all chunks
        AssertNotNull(_indices);
        AssertNotNull(_indexBuffer);
        chunk.SetIndices(_indices, _indexBuffer, _indicesLength);
        chunk.VerticesUsed = (uint)verticesUsed;
        chunk.Bounds = Cube.FromMinAndMax(min, max);
        chunk.VerticesGeneratedForVersion = chunkVersionUpdateTo;

#if DEBUG
        Interlocked.Decrement(ref chunk.DEBUG_UpdateVerticesThreadCount);
        Assert(chunk.DEBUG_UpdateVerticesThreadCount == 0);
#endif
    }

    #endregion

    #region Rendering - Index Buffer

    // One index buffer is used for all chunks, and they are all the same length.

    protected ushort[]? _indices; // Collision only;
    protected int _indicesLength;
    protected IndexBuffer? _indexBuffer;

    protected void PrepareIndexBuffer()
    {
        Assert(GLThread.IsGLThread());

        int quads = (int)(ChunkSize.X * ChunkSize.Y);
        int indexCount = quads * 3 * 4;
        int evenRowWidth = (int)(ChunkSize.X + 1);
        int oddRowWidth = (int) ChunkSize.X;
        int height = (int) (ChunkSize.Y + ChunkSize.Y + 1);

        _indexBuffer ??= new IndexBuffer();

        ushort[] indices = new ushort[indexCount];

        // Go through each odd row, and create triangles from the vertices there
        // which are all in the middle of quads.
        int indexOffset = 0;
        for (int y = 1; y < height; y += 2)
        {
            int middleVertexStart = (y * evenRowWidth) - (y / 2);
            for (int x = 0; x < oddRowWidth; x++)
            {
                int middleVertex = middleVertexStart + x;

                int topLeft = middleVertex - evenRowWidth;
                int bottomRight = middleVertex + evenRowWidth;

                indices[indexOffset + 0] = (ushort)middleVertex;
                indices[indexOffset + 1] = (ushort)bottomRight;
                indices[indexOffset + 2] = (ushort)(topLeft + 1);

                indices[indexOffset + 3] = (ushort)middleVertex;
                indices[indexOffset + 4] = (ushort)(topLeft + 1);
                indices[indexOffset + 5] = (ushort)topLeft;

                indices[indexOffset + 6] = (ushort)middleVertex;
                indices[indexOffset + 7] = (ushort)topLeft;
                indices[indexOffset + 8] = (ushort)(bottomRight - 1);

                indices[indexOffset + 9] = (ushort)middleVertex;
                indices[indexOffset + 10] = (ushort)(bottomRight - 1);
                indices[indexOffset + 11] = (ushort)bottomRight;

                indexOffset += (4 * 3);
            }
        }

        _indexBuffer.Upload(indices, indexCount);
        _indicesLength = indexCount;

        // Used for collision
        _indices = indices;
    }

    #endregion

    #region World Space Helpers

    public override Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        location -= TileSize; // Stiching vertex
        return base.GetTilePosOfWorldPos(location);
    }

    public override Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        return base.GetWorldPosOfTile(tileCoord2d) + TileSize;
    }

    public override float GetHeightAt(Vector2 worldSpace)
    {
        worldSpace -= TileSize; // Stiching

        Vector2 tilePos = worldSpace / TileSize;
        Vector2 floorPos = tilePos.Floor();
        Vector2 ceilPos = tilePos.Ceiling();

        //TerrainMeshGridChunk? chunk00 = GetChunkAt(floorPos, out Vector2 relCoord00);
        //TerrainMeshGridChunk? chunk10 = GetChunkAt(new Vector2(ceilPos.X, floorPos.Y), out Vector2 relCoord10);
        //TerrainMeshGridChunk? chunk01 = GetChunkAt(new Vector2(floorPos.X, ceilPos.Y), out Vector2 relCoord01);
        //TerrainMeshGridChunk? chunk11 = GetChunkAt(ceilPos, out Vector2 relCoord11);

        //float h00 = chunk00 != null ? GetAtForChunk(chunk00, relCoord00) : 0;
        //float h10 = chunk10 != null ? GetAtForChunk(chunk10, relCoord10) : 0;
        //float h01 = chunk01 != null ? GetAtForChunk(chunk01, relCoord01) : 0;
        //float h11 = chunk11 != null ? GetAtForChunk(chunk11, relCoord11) : 0;

        //float fracX = tilePos.X - floorPos.X;
        //float fracY = tilePos.Y - floorPos.Y;

        //float h0 = Maths.Lerp(h00, h10, fracX); // bottom
        //float h1 = Maths.Lerp(h01, h11, fracX); // top
        //float height = Maths.Lerp(h0, h1, fracY); // both

        return 0;
    }

    #endregion

    #region Brush

    private bool _editorBrush;
    private float _editorBrushSize;
    private float _editorBrushStr;
    private Vector2 _editorBrushPosition;

    public void SetEditorBrush(bool enabled, float brushSize, float brushStr)
    {
        _editorBrush = enabled;
        _editorBrushSize = brushSize;
        _editorBrushStr = brushStr;
    }

    public Vector2 GetEditorBrushWorldPosition()
    {
        return _editorBrushPosition;
    }

    private Vector2 GetEditorBrush()
    {
        Vector2 brushPoint = Vector2.NaN;
        if (_editorBrush)
        {
            CameraBase camera = Engine.Renderer.Camera;
            Ray3D mouseRay = camera.GetCameraMouseRay();
            Vector3 mousePosWorld = mouseRay.IntersectWithPlane(Graphics.Renderer.Up, Vector3.Zero);

            foreach (TerrainChunk chunkToRender in _renderThisPass)
            {
                VertexDataAllocation vertices = chunkToRender.VertexMemory;
                if (!vertices.Allocated) continue;

                ushort[]? indices = chunkToRender.CPUIndexBuffer;
                if (indices == null) continue;

                if (mouseRay.IntersectWithVertices(indices, vertices, out Vector3 collisionPoint, out _, out _))
                    brushPoint = collisionPoint.ToVec2();
            }

            // No chunk there - just intersect with plane
            if (float.IsNaN(brushPoint.X))
                brushPoint = mousePosWorld.ToVec2();
        }

        _editorBrushPosition = brushPoint;
        return _editorBrushPosition;
    }

    public enum BrushOperation
    {
        Rise,
        Lower
    }

    public void ApplyBrushHeight(BrushOperation op)
    {
        Vector2 brushPos = GetEditorBrush();
        Vector2 chunkWorldSize = ChunkSize * TileSize;

        Engine.Renderer.DbgClear();
        Engine.Renderer.DbgAddPoint(brushPos.ToVec3(), 0.5f, Color.Red);

        ApplyBrushToTerrain? brushFunc = null;
        switch(op)
        {
            case BrushOperation.Rise:
                brushFunc = static (ref data, str) => data.Height += str;
                break;
            case BrushOperation.Lower:
                brushFunc = static (ref data, str) => data.Height -= str;
                break;
        }
        if (brushFunc == null) return;

        float brushRadius = _editorBrushSize;
        int minChunkX = (int) MathF.Floor((brushPos.X - brushRadius) / chunkWorldSize.X);
        int maxChunkX = (int) MathF.Floor((brushPos.X + brushRadius) / chunkWorldSize.X);
        int minChunkY = (int) MathF.Floor((brushPos.Y - brushRadius) / chunkWorldSize.Y);
        int maxChunkY = (int) MathF.Floor((brushPos.Y + brushRadius) / chunkWorldSize.Y);
        for (int cy = minChunkY; cy <= maxChunkY; cy++)
        {
            for (int cx = minChunkX; cx <= maxChunkX; cx++)
            {
                Vector2 chunkCoord = new Vector2(cx, cy);
                TerrainChunk? chunk = GetChunk(chunkCoord);
                if (chunk == null)
                {
                    TerrainChunk newChunk = InitializeNewChunk();
                    _chunks.Add(chunkCoord, newChunk);
                    OnChunkCreated(chunkCoord, newChunk);
                    _chunkBoundsCacheValid = false;

                    chunk = newChunk;
                }
                ApplyBrushToSingleChunk(brushPos, chunkCoord, chunk, brushFunc);
            }
        }
    }

    public delegate void ApplyBrushToTerrain(ref TerrainData data, float strength);

    private void ApplyBrushToSingleChunk(Vector2 brushPos, Vector2 chunkCoord, TerrainChunk chunk, ApplyBrushToTerrain func)
    {
        float radius = _editorBrushSize;
        float radiusSq = MathF.Pow(radius, 2);
        float strength = _editorBrushStr;
       
        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = TileSize / 2f;
        Vector2 chunkWorldOffset = chunkCoord * (ChunkSize * tileSize);

        Vector2 brushChunkSpace = brushPos - chunkWorldOffset;

        int minY = (int) MathF.Floor((brushChunkSpace.Y - radius) / halfTileSize.Y);
        int maxY = (int) MathF.Ceiling((brushChunkSpace.Y + radius) / halfTileSize.Y);
        minY = Math.Max(0, minY);
        maxY = (int) MathF.Min(ChunkSize.Y * 2, maxY);

        int evenRowWidth = (int) (ChunkSize.X + 1); // 0...2...4
        int oddRowWidth = (int) ChunkSize.X; // 1...3...5
        int verticesPerPair = evenRowWidth + oddRowWidth;

        TerrainData[] data = chunk.GetRawData();
        for (int y = minY; y <= maxY; y++)
        {
            bool oddRow = y % 2 != 0;
            float xOffset = oddRow ? halfTileSize.X : 0;
            int rowWidth = oddRow ? oddRowWidth : evenRowWidth;

            int minX = (int) MathF.Floor((brushChunkSpace.X - xOffset - radius) / tileSize.X);
            int maxX = (int) MathF.Ceiling((brushChunkSpace.X - xOffset + radius) / tileSize.X);

            minX = Math.Max(0, minX);
            maxX = Math.Min(rowWidth - 1, maxX);

            int rowStartIndex = (y / 2) * verticesPerPair;
            if (oddRow) rowStartIndex += evenRowWidth;

            for (int x = minX; x <= maxX; x++)
            {
                Vector2 vertPos = new Vector2(x * tileSize.X + xOffset, y * halfTileSize.Y) + chunkWorldOffset;

                // Circular brush
                float distSq = Vector2.DistanceSquared(brushPos, vertPos);
                if (distSq < radiusSq)
                {
                    float d = MathF.Sqrt(distSq) / radius;
                    float falloff = MathF.Pow(1f - d * d, 2);

                    int idx = rowStartIndex + x;
                    ref TerrainData terrain = ref data[idx];

                    var tPos = terrain.DBG_Pos.ToVec2();
                    if (tPos != Vector2.Zero && tPos != vertPos)
                    {
                        bool a = true;
                    }

                    //Engine.Renderer.DbgAddPoint(vertPos.ToVec3());
                    //Engine.Renderer.DbgAddText(vertPos.ToVec3(), falloff.ToString("0.00"));
                    func(ref terrain, strength * falloff);
                }
            }
        }

        OnChunkChanged(chunkCoord, chunk);
    }

    #endregion
}