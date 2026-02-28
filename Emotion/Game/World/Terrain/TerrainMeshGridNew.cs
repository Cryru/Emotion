#nullable enable

using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using System.Runtime.InteropServices;

namespace Emotion.Game.World.Terrain;

[StructLayout(LayoutKind.Sequential)]
public struct TerrainData
{
    public static TerrainData DefaultData = new TerrainData() { Height = 0, Color = Color.White };

    public float Height;
    public Vector4 Weights;
    public Color Color;
}

[StructLayout(LayoutKind.Sequential)]
public struct TerrainVertex
{
    public readonly static VertexDataFormat Format = new VertexDataFormat()
        .AddVertexPosition()
        .AddNormal()
        .AddVertexColor()
        .AddCustomVector4()
        .Build();

    public Vector3 Position;
    public Vector3 Normal;
    public uint Color;
    public Vector4 Weights;
}

public class TerrainChunk : MeshGridStreamableChunk<TerrainData, ushort>
{

}

public partial class TerrainMeshGridNew : MeshGrid<TerrainData, TerrainChunk, ushort>, IMapGrid, ICustomReflectorMeta_CustomCreateNew<TerrainMeshGridNew>
{
    public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");

    public TerrainMeshGridNew(Vector2 tileSize, float chunkSize) : base(tileSize, chunkSize)
    {
        // This API is utter trash
        _vertexFormat = TerrainVertex.Format;
    }

    protected TerrainMeshGridNew() : this(Vector2.Zero, 0)
    {

    }

    public IEnumerator InitRoutine(GameMap.GridFriendAdapter adapter)
    {
        GetMeshMaterial().EnsureAssetsLoaded();
        yield return GLThread.ExecuteOnGLThreadAsync(PrepareIndexBuffer);
        yield return base.InitRoutine();
    }

    #region Serialization

    public static new TerrainMeshGridNew CustomCreateNew()
    {
        return new TerrainMeshGridNew();
    }

    #endregion

    #region Rendering

    [DontSerialize]
    public MeshMaterial TerrainMeshMaterial = new MeshMaterial()
    {
        Name = "TerrainChunkMaterial",
        State =
        {
            FaceCulling = true,
            FaceCullingBackFace = true,
            Shader = "Shaders3D/TerrainShaderNew.glsl"
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

        Texture.EnsureBound(Texture.Checkerboard.Pointer, 0);
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

        Span<TerrainVertex> vertices = ResizeVertexMemoryAndGetSpan<TerrainVertex>(ref chunk.VertexMemory, chunkCoord, vertexCount);

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
                ref TerrainVertex vert = ref vertices[verticesUsed];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = GetNormalForVert(x, y, chunkCoord, tileSize, halfTileSize);
                vert.Color = terrainData.Color.ToUint();
                vert.Weights = terrainData.Weights;

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
                TerrainData terrainData = dataRight != null ? dataRight[y * rowWidth] : TerrainData.DefaultData;
                ref TerrainVertex vert = ref vertices[vIdx];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = GetNormalForVert(0, y, rightChunkCoord, tileSize, halfTileSize);
                vert.Color = terrainData.Color.ToUint();
                vert.Weights = terrainData.Weights;

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
                TerrainData terrainData = dataBottom != null ? dataBottom[readIdx] : TerrainData.DefaultData;
                readIdx++;

                ref TerrainVertex vert = ref vertices[verticesUsed];

                vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
                vert.Normal = GetNormalForVert(x, 0, bottomChunkCoord, tileSize, halfTileSize);
                vert.Color = terrainData.Color.ToUint();
                vert.Weights = terrainData.Weights;

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
            TerrainData terrainData = dataBottomRight != null ? dataBottomRight[0] : TerrainData.DefaultData;

            ref TerrainVertex vert = ref vertices[verticesUsed];

            vert.Position = (pen + chunkWorldOffset).ToVec3(terrainData.Height);
            vert.Normal = GetNormalForVert(0, 0, bottomRightChunkCoord, tileSize, halfTileSize);
            vert.Color = terrainData.Color.ToUint();
            vert.Weights = terrainData.Weights;

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

    public override Vector2 GetTilePosOfWorldPos(Vector2 worldSpace)
    {
        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = tileSize / 2f;

        int gy = (int)MathF.Floor(worldSpace.Y / halfTileSize.Y);
        bool oddRow = (gy % 2) != 0;
        float rowOffset = oddRow ? halfTileSize.X : 0f;
        int gx = (int)MathF.Floor((worldSpace.X - rowOffset) / tileSize.X);

        return new Vector2(gx, gy);
    }

    public override Vector2 GetWorldPosOfTile(Vector2 tileCoord)
    {
        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = tileSize / 2f;

        bool odd = (tileCoord.Y % 2) != 0;
        float off = odd ? halfTileSize.X : 0f;
        return new Vector2(tileCoord.X * tileSize.X + off, tileCoord.Y * halfTileSize.Y);
    }

    public Vector3 GetWorldPosOfTileWithHeight(Vector2 tileCoord)
    {
        Vector2 worldPos = GetWorldPosOfTile(tileCoord);
        GetHeightAtGridCoord(tileCoord, out float height);
        return worldPos.ToVec3(height);
    }

    public override float GetHeightAt(Vector2 worldSpace)
    {
        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = tileSize / 2f;

        // Find the closest quad start (top left vertex on an even row)
        int gy = (int)MathF.Floor(worldSpace.Y / tileSize.Y) * 2;
        int gx = (int)MathF.Floor(worldSpace.X / tileSize.X);

        Vector2 centerCoord = new Vector2(gx, gy + 1);
        Vector2 topLeftCoord = new Vector2(gx, gy);
        Vector2 topRightCoord = new Vector2(gx + 1, gy);
        Vector2 botLeftCoord = new Vector2(gx, gy + 2);
        Vector2 botRightCoord = new Vector2(gx + 1, gy + 2);

        Vector3 centerPos = GetWorldPosOfTileWithHeight(centerCoord);
        Vector3 topLeftPos = GetWorldPosOfTileWithHeight(topLeftCoord);
        Vector3 topRightPos = GetWorldPosOfTileWithHeight(topRightCoord);

        Triangle posInTri = Triangle.Invalid;
        Triangle currentTri = new Triangle(centerPos, topRightPos, topLeftPos); // Top triangle
        if (currentTri.IsPoint2DInTriangle(worldSpace))
        {
            posInTri = currentTri;
        }
        else
        {
            Vector3 botLeftPos = GetWorldPosOfTileWithHeight(botLeftCoord);
            currentTri = new Triangle(centerPos, topLeftPos, botLeftPos); // Left triangle
            if (currentTri.IsPoint2DInTriangle(worldSpace))
            {
                posInTri = currentTri;
            }
            else
            {
                Vector3 botRightPos = GetWorldPosOfTileWithHeight(botRightCoord);
                currentTri = new Triangle(centerPos, botLeftPos, botRightPos); // Bottom triangle
                if (currentTri.IsPoint2DInTriangle(worldSpace))
                {
                    posInTri = currentTri;
                }
                else
                {
                    currentTri = new Triangle(centerPos, botRightPos, topRightPos); // Right triangle
                    if (currentTri.IsPoint2DInTriangle(worldSpace))
                    {
                        posInTri = currentTri;
                    }
                }
            }
        }

        // Sanity check!
        if (!posInTri.Valid) return centerPos.Z;

        // Vertical triangle?
        Vector3 n = posInTri.Normal;
        if (MathF.Abs(n.Z) < Maths.EPSILON) return posInTri.A.Z;

        return posInTri.A.Z - (n.X * (worldSpace.X - posInTri.A.X) + n.Y * (worldSpace.Y - posInTri.A.Y)) / n.Z;
    }

    public void SetHeightAt(Vector2 worldSpace)
    {
        // todo: find the chunk, the tiles, and interpolate setting them (maybe this just calls the editor brush function with some brush?)
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
        ChangeHeight,
        TextureWeights,
        Color
    }

    public struct TerrainBrush
    {
        public BrushOperation Operation;
        public float Size;
        public float Strength;
        public Color Color;
    }

    public void ApplyBrushHeight(TerrainBrush brush)
    {
        BrushOperation op = brush.Operation;
        SetEditorBrush(true, brush.Size, brush.Strength);

        Vector2 brushPos = GetEditorBrush();
        Vector2 chunkWorldSize = ChunkSize * TileSize;

        Engine.Renderer.DbgClear();
        Engine.Renderer.DbgAddPoint(brushPos.ToVec3(), 0.5f, Color.Red);

        ApplyBrushToTerrain? brushFunc = null;
        switch (op)
        {
            case BrushOperation.ChangeHeight:
                brushFunc = static (ref data, brush, str) => data.Height += str;
                break;
            case BrushOperation.Color:
                brushFunc = static (ref data, brush, str) =>
                {
                    data.Color = Color.Lerp(data.Color, brush.Color, str);
                };
                break;
            case BrushOperation.TextureWeights:
                brushFunc = static (ref data, brush, str) =>
                {
                    var colors = new Color[5] { Color.Red, Color.Green, Color.Blue, Color.PrettyOrange, Color.Black };
                    int colIdx = colors.IndexOf(brush.Color);

                    Vector4 w = data.Weights;
                    float sum = w.X + w.Y + w.Z + w.W;
                    float w4 = Math.Max(1f - sum, 0f);
                    float[] weights = [w.X, w.Y, w.Z, w.W, w4];
                    weights[colIdx] += brush.Strength;

                    float total = 0;
                    for (int i = 0; i < weights.Length; i++)
                    {
                        total += weights[i];
                    }

                    // Normalize
                    for (int i = 0; i < weights.Length; i++)
                    {
                        weights[i] /= total;
                    }
                    data.Weights = new Vector4(weights[0], weights[1], weights[2], weights[3]);
                };
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

        IntVector2 gridPos = new IntVector2(gridX, gridY);

        // Center (for fallback)
        GetHeightAtGridCoord(gridPos, out float fallback);

        // Horizontal
        if (!GetHeightAtGridCoord(gridPos - new IntVector2(1, 0), out float hL))
            hL = fallback;
        if (!GetHeightAtGridCoord(gridPos + new IntVector2(1, 0), out float hR))
            hR = fallback;

        // Vertical
        if (!GetHeightAtGridCoord(gridPos - new IntVector2(0, 2), out float hT))
            hT = fallback;
        if (!GetHeightAtGridCoord(gridPos + new IntVector2(0, 2), out float hB))
            hB = fallback;

        float dX = tileSize.X * 2f;
        float dY = tileSize.Y * 2f;
        Vector3 normal = new Vector3(
            -dY * (hR - hL),
            -dX * (hB - hT),
            dX * dY
        );

        return Vector3.Normalize(normal);
    }

    private bool GetHeightAtGridCoord(Vector2 gridPos, out float height)
    {
        return GetHeightAtGridCoord(gridPos.ToIVec2Ceil(), out height);
    }

    private bool GetHeightAtGridCoord(IntVector2 gridPos, out float height)
    {
        height = 0;

        int chunkWidth = (int)ChunkSize.X;
        int chunkHeight = (int)ChunkSize.Y * 2;
        IntVector2 chunkCoord = gridPos.FloorDiv(new IntVector2(chunkWidth, chunkHeight));

        TerrainChunk? chunk = GetChunk(chunkCoord);
        if (chunk == null) return false;

        TerrainData[] data = chunk.GetRawData();

        int lX = (gridPos.X % chunkWidth + chunkWidth) % chunkWidth;
        int lY = (gridPos.Y % chunkHeight + chunkHeight) % chunkHeight;
        int idx = lY * chunkWidth + lX;
        Assert(idx >= 0 && idx < data.Length);
        height = data[idx].Height;
        return true;
    }

    #endregion
}