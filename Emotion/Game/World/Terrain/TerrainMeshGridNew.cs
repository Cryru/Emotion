#nullable enable

using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using System.Runtime.InteropServices;

namespace Emotion.Game.World.Terrain;

[StructLayout(LayoutKind.Sequential)]
public struct TerrainData
{
    public float Height;
    public Color Color;
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

        float evenRows = ChunkSize.X * ChunkSize.Y;
        float oddRows = ChunkSize.X * ChunkSize.Y; // Middle vertices
        int vertices = (int)(evenRows + oddRows);

        TerrainData[] newChunkData = new TerrainData[vertices];
        for (int i = 0; i < newChunkData.Length; i++)
        {
            newChunkData[i].Color = Color.White;
        }
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
        int rowWidth = (int)ChunkSize.X;
        int rows = (int)(ChunkSize.Y + ChunkSize.Y);

        TerrainData[] data = chunk.GetRawData();

        int vertexCount = data.Length;
        int stichingVertices = 0;
        stichingVertices += rowWidth; // Extra row
        stichingVertices += (int)ChunkSize.Y; // Extra column on even rows
        stichingVertices++; // Bottom right corner
        vertexCount += stichingVertices;

        //Engine.Renderer.DbgAddCube(Cube.FromMinAndMax(chunkWorldOffset.ToVec3(0), (ChunkSize * TileSize).ToVec3(10)));

        Span<VertexData_Pos_UV_Normal_Color> vertices = ResizeVertexMemoryAndGetSpan(ref chunk.VertexMemory, chunkCoord, vertexCount);

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;

        Vector2 pen = Vector2.Zero;
        int verticesUsed = 0;
        int dataRead = 0;
        for (int y = 0; y < rows; y++)
        {
            bool oddRow = y % 2 != 0;
            pen.X = oddRow ? halfTileSize.X : 0;

            for (int x = 0; x < rowWidth; x++)
            {
                ref TerrainData terrainData = ref data[dataRead];
                ref VertexData_Pos_UV_Normal_Color vert = ref vertices[verticesUsed];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = GetNormalForVert(x, y, chunkCoord, tileSize, halfTileSize);
                vert.Color = terrainData.Color.ToUint();
                vert.UV = Vector2.Zero;

                //Engine.Renderer.DbgAddPoint(vert.Position.ToVec2().ToVec3(0));
                //Engine.Renderer.DbgAddText(vert.Position.ToVec2().ToVec3(0), $"{verticesUsed}");

                min = verticesUsed == 0 ? vert.Position : Vector3.Min(min, vert.Position);
                max = verticesUsed == 0 ? vert.Position : Vector3.Max(max, vert.Position);

                pen.X += tileSize.X;

                verticesUsed++;
                dataRead++;
            }

            if (!oddRow) verticesUsed++;

            pen.Y += halfTileSize.Y;
        }

        // Add stitching vertices
        Vector2 rightChunkCoord = chunkCoord + new Vector2(1, 0);
        TerrainChunk? chunkRight = GetChunk(rightChunkCoord);
        TerrainData[]? dataRight = chunkRight?.GetRawData();

        {
            pen = Vector2.Zero;
            int vIdx = 0;
            for (int y = 0; y < rows; y++)
            {
                bool oddRow = y % 2 != 0;
                pen.X = oddRow ? halfTileSize.X : 0;

                pen.X += tileSize.X * rowWidth;
                vIdx += rowWidth;
                if (oddRow)
                {
                    pen.Y += halfTileSize.Y;
                    continue;
                }

                int x = rowWidth + 1;
                TerrainData terrainData = dataRight != null ? dataRight[y * rowWidth] : default;
                ref VertexData_Pos_UV_Normal_Color vert = ref vertices[vIdx];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = GetNormalForVert(0, y, rightChunkCoord, tileSize, halfTileSize);
                vert.Color = terrainData.Color.ToUint();
                vert.UV = Vector2.Zero;

                //Engine.Renderer.DbgAddPoint(vert.Position.ToVec2().ToVec3(0));
                //Engine.Renderer.DbgAddText(vert.Position.ToVec2().ToVec3(0), $"{verticesUsed}");

                min = Vector3.Min(min, vert.Position);
                max = Vector3.Max(max, vert.Position);

                pen.X += tileSize.X;
                pen.Y += halfTileSize.Y;
                vIdx++;
            }
        }

        Vector2 bottomChunkCoord = chunkCoord + new Vector2(0, 1);
        TerrainChunk? chunkBottom = GetChunk(bottomChunkCoord);
        TerrainData[]? dataBottom = chunkBottom?.GetRawData();
        {
            int readIdx = 0;
            int y = rows;
            bool oddRow = y % 2 != 0;
            pen.X = oddRow ? halfTileSize.X : 0;

            for (int x = 0; x < rowWidth; x++)
            {
                TerrainData terrainData = dataBottom != null ? dataBottom[readIdx] : default;
                readIdx++;

                ref VertexData_Pos_UV_Normal_Color vert = ref vertices[verticesUsed];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = GetNormalForVert(x, 0, bottomChunkCoord, tileSize, halfTileSize);
                vert.Color = terrainData.Color.ToUint();
                vert.UV = Vector2.Zero;

                //Engine.Renderer.DbgAddPoint(vert.Position.ToVec2().ToVec3(0));
                //Engine.Renderer.DbgAddText(vert.Position.ToVec2().ToVec3(0), $"{verticesUsed}");

                min = Vector3.Min(min, vert.Position);
                max = Vector3.Max(max, vert.Position);

                pen.X += tileSize.X;

                verticesUsed++;
            }
        }

        Vector2 bottomRightChunkCoord = chunkCoord + new Vector2(1, 1);
        TerrainChunk? chunkBottomRight = GetChunk(bottomRightChunkCoord);
        TerrainData[]? dataBottomRight = chunkBottomRight?.GetRawData();

        {
            TerrainData terrainData = dataBottomRight != null ? dataBottomRight[0] : default;

            ref VertexData_Pos_UV_Normal_Color vert = ref vertices[verticesUsed];

            vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
            vert.Normal = GetNormalForVert(0, 0, bottomRightChunkCoord, tileSize, halfTileSize);
            vert.Color = terrainData.Color.ToUint();
            vert.UV = Vector2.Zero;

            //Engine.Renderer.DbgAddPoint(vert.Position.ToVec2().ToVec3(0));
            //Engine.Renderer.DbgAddText(vert.Position.ToVec2().ToVec3(0), $"{verticesUsed}");

            min = Vector3.Min(min, vert.Position);
            max = Vector3.Max(max, vert.Position);

            verticesUsed++; 
        }

        // Propagate the update (due to stitching vertices)
        if (propagate)
        {
            for (int cY = -1; cY <= 1; cY++)
            {
                for (int cX = -1; cX <= 1; cX++)
                {
                    if (cX == 0 && cY == 0) continue;

                    Vector2 stitchCoord = chunkCoord + new Vector2(cX, cY);
                    TerrainChunk? stitchChunk = GetChunk(stitchCoord);
                    if (stitchChunk != null && stitchChunk.State >= ChunkState.HasMesh)
                        RequestChunkMeshUpdate(stitchCoord, stitchChunk);
                }
            }
        }

        if (_indexBuffer == null)
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
        int oddRowWidth = (int)ChunkSize.X;
        int height = (int)(ChunkSize.Y + ChunkSize.Y + 1);

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
                if (!mouseRay.IntersectWithCube(chunkToRender.Bounds, out _, out _)) continue;

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
        Lower,
        Color
    }

    public struct TerrainBrush
    {
        public float Size;
        public float Strength;
        public Color Color;
    }

    public void ApplyBrushHeight(BrushOperation op, TerrainBrush brush)
    {
        SetEditorBrush(true, brush.Size, brush.Strength);

        Vector2 brushPos = GetEditorBrush();
        Vector2 chunkWorldSize = ChunkSize * TileSize;

        Engine.Renderer.DbgClear();
        Engine.Renderer.DbgAddPoint(brushPos.ToVec3(), 0.5f, Color.Red);

        ApplyBrushToTerrain? brushFunc = null;
        switch (op)
        {
            case BrushOperation.Rise:
                brushFunc = static (ref data, brush, str) => data.Height += str;
                break;
            case BrushOperation.Lower:
                brushFunc = static (ref data, brush, str) => data.Height -= str;
                break;
            case BrushOperation.Color:
                brushFunc = static (ref data, brush, str) => data.Color = Color.Lerp(data.Color, brush.Color, str);
                break;
        }
        if (brushFunc == null) return;

        float brushRadius = _editorBrushSize;
        int minChunkX = (int)MathF.Floor((brushPos.X - brushRadius) / chunkWorldSize.X);
        int maxChunkX = (int)MathF.Floor((brushPos.X + brushRadius) / chunkWorldSize.X);
        int minChunkY = (int)MathF.Floor((brushPos.Y - brushRadius) / chunkWorldSize.Y);
        int maxChunkY = (int)MathF.Floor((brushPos.Y + brushRadius) / chunkWorldSize.Y);
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
                ApplyBrushToSingleChunk(brushPos, chunkCoord, chunk, brush, brushFunc);
            }
        }
    }

    public delegate void ApplyBrushToTerrain(ref TerrainData data, TerrainBrush brush, float fallOff);

    private void ApplyBrushToSingleChunk(Vector2 brushPos, Vector2 chunkCoord, TerrainChunk chunk, TerrainBrush brush, ApplyBrushToTerrain func)
    {
        float radius = _editorBrushSize;
        float radiusSq = MathF.Pow(radius, 2);
        float strength = _editorBrushStr;

        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = TileSize / 2f;
        Vector2 chunkWorldOffset = chunkCoord * (ChunkSize * tileSize);

        Vector2 brushChunkSpace = brushPos - chunkWorldOffset;

        int minY = (int)MathF.Floor((brushChunkSpace.Y - radius) / halfTileSize.Y);
        int maxY = (int)MathF.Ceiling((brushChunkSpace.Y + radius) / halfTileSize.Y);
        minY = Math.Max(0, minY);
        maxY = (int)MathF.Min((ChunkSize.Y * 2) - 1, maxY);

        int rowWidth = (int)ChunkSize.X;

        TerrainData[] data = chunk.GetRawData();
        for (int y = minY; y <= maxY; y++)
        {
            bool oddRow = y % 2 != 0;
            float xOffset = oddRow ? halfTileSize.X : 0;

            int minX = (int)MathF.Floor((brushChunkSpace.X - xOffset - radius) / tileSize.X);
            int maxX = (int)MathF.Ceiling((brushChunkSpace.X - xOffset + radius) / tileSize.X);

            minX = Math.Max(0, minX);
            maxX = Math.Min(rowWidth - 1, maxX);

            int rowStartIndex = y * rowWidth;
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

                    //Engine.Renderer.DbgAddPoint(vertPos.ToVec3());
                    //Engine.Renderer.DbgAddText(vertPos.ToVec3(), falloff.ToString("0.00"));
                    func(ref terrain, brush, strength * falloff);
                }
            }
        }

        OnChunkChanged(chunkCoord, chunk);
    }

    #endregion

    #region Normals

    private Vector3 GetNormalForVert(int x, int y, Vector2 chunkCoord, Vector2 tileSize, Vector2 halfTileSize)
    {
        int gridX = (int)chunkCoord.X * (int)ChunkSize.X + x;
        int gridY = (int)chunkCoord.Y * (int)ChunkSize.Y * 2 + y;

        // Center (for fallback)
        float fallback = GetHeightAtGrid(gridX, gridY, 0);

        // Horizontal
        float hL = GetHeightAtGrid(gridX - 1, gridY, fallback);
        float hR = GetHeightAtGrid(gridX + 1, gridY, fallback);

        // Vertical
        float hT = GetHeightAtGrid(gridX, gridY - 2, fallback);
        float hB = GetHeightAtGrid(gridX, gridY + 2, fallback);

        float dX = tileSize.X * 2f;
        float dY = tileSize.Y * 2f;
        Vector3 normal = new Vector3(
            -dY * (hR - hL),
            -dX * (hB - hT),
            dX * dY
        );

        return Vector3.Normalize(normal);
    }

    private float GetHeightAtGrid(int gridX, int gridY, float fallback)
    {
        int chunkWidth = (int)ChunkSize.X;
        int chunkHeight = (int)ChunkSize.Y * 2;

        int chunkX = Maths.FloorDiv(gridX, chunkWidth);
        int chunkY = Maths.FloorDiv(gridY, chunkHeight);

        Vector2 coord = new Vector2(chunkX, chunkY);
        TerrainChunk? chunk = GetChunk(coord);
        if (chunk == null) return fallback;

        TerrainData[] data = chunk.GetRawData();

        int lX = (gridX % chunkWidth + chunkWidth) % chunkWidth;
        int lY = (gridY % chunkHeight + chunkHeight) % chunkHeight;
        int idx = lY * chunkWidth + lX;
        Assert(idx >= 0 && idx < data.Length);
        return data[idx].Height;
    }

    #endregion
}