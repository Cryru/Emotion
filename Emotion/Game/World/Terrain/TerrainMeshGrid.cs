#nullable enable

using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using Emotion.Primitives.Grids;
using TerrainMeshGridChunk = Emotion.Game.World.Terrain.MeshGridStreaming.MeshGridStreamableChunk<float, ushort>;

namespace Emotion.Game.World.Terrain;

public partial class TerrainMeshGrid : MeshGrid<float, TerrainMeshGridChunk, ushort>
{
    public TerrainMeshGrid(Vector2 tileSize, float chunkSize) : base(tileSize, chunkSize)
    {
    }

    public override IEnumerator InitRuntimeDataRoutine()
    {
        yield return GLThread.ExecuteOnGLThreadAsync(PrepareIndexBuffer);
        yield return base.InitRuntimeDataRoutine();
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

    private const bool HEIGHT_BASED_VERTEX_NORMALS = true;

    protected override void UpdateChunkVertices(Vector2 chunkCoord, TerrainMeshGridChunk chunk)
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

        Vector2 chunkWorldOffset = chunkCoord * chunkWorldSize + tileSize;

        int vertexCount = (int)(ChunkSize.X * ChunkSize.Y);
        int stichingVertices = (int)(ChunkSize.X + ChunkSize.Y + 1);
        vertexCount += stichingVertices;

        Span<VertexData_Pos_UV_Normal_Color> vertices = chunk.ResizeVertexMemoryAndGetSpan(chunkCoord, vertexCount);

        // Get my data
        float[] dataMe = chunk.GetRawData() ?? Array.Empty<float>();

        // Get data around for stitching vertices
        Vector2 leftChunkCoord = chunkCoord + new Vector2(-1, 0);
        TerrainMeshGridChunk? chunkLeft = GetChunk(leftChunkCoord);
        float[] dataLeft = chunkLeft?.GetRawData() ?? Array.Empty<float>();

        Vector2 topChunkCoord = chunkCoord + new Vector2(0, -1);
        TerrainMeshGridChunk? chunkTop = GetChunk(topChunkCoord);
        float[] dataTop = chunkTop?.GetRawData() ?? Array.Empty<float>();

        Vector2 topLeftChunkCoord = chunkCoord + new Vector2(-1, -1);
        TerrainMeshGridChunk? chunkTopLeft = GetChunk(topLeftChunkCoord);
        float[] dataTopLeft = chunkTopLeft?.GetRawData() ?? Array.Empty<float>();

        // Propagate data changes to stitching chunks.
        // Note: Chunks shouldnt read from each other's vertices, just data.
        if (propagate)
        {
            TerrainMeshGridChunk? stitchChunk;

            // For normals
            if (chunkTopLeft != null && chunkTopLeft.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(topLeftChunkCoord, chunkTopLeft);

            // For normals
            if (chunkTop != null && chunkTop.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(topChunkCoord, chunkTop);

            // For normals
            Vector2 topRightCoord = chunkCoord + new Vector2(1, -1);
            stitchChunk = GetChunk(topRightCoord);
            if (stitchChunk != null && stitchChunk.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(topRightCoord, stitchChunk);

            // For normals
            if (chunkLeft != null && chunkLeft.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(leftChunkCoord, chunkLeft);

            // For stitching
            Vector2 rightChunkCoord = chunkCoord + new Vector2(1, 0);
            stitchChunk = GetChunk(rightChunkCoord);
            if (stitchChunk != null && stitchChunk.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(rightChunkCoord, stitchChunk);

            // For normals
            Vector2 bottomLeftCoord = chunkCoord + new Vector2(-1, 1);
            stitchChunk = GetChunk(bottomLeftCoord);
            if (stitchChunk != null && stitchChunk.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(bottomLeftCoord, stitchChunk);

            // For stitching
            Vector2 bottomChunkCoord = chunkCoord + new Vector2(0, 1);
            stitchChunk = GetChunk(bottomChunkCoord);
            if (stitchChunk != null && stitchChunk.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(bottomChunkCoord, stitchChunk);

            // For stitching
            Vector2 bottomRightChunkCoord = chunkCoord + new Vector2(1, 1);
            stitchChunk = GetChunk(bottomRightChunkCoord);
            if (stitchChunk != null && stitchChunk.State >= ChunkState.HasMesh)
                RequestChunkMeshUpdate(bottomRightChunkCoord, stitchChunk);
        }

        Vector3 min = Vector3.Zero;
        Vector3 max = Vector3.Zero;

        int vIdx = 0;
        for (int y = -1; y < ChunkSize.Y; y++)
        {
            for (int x = -1; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);

                float heightSample = SampleHeightMap(ChunkSize, x, y, dataMe, dataTopLeft, dataTop, dataLeft);
                Vector2 worldPos = chunkWorldOffset + tileCoord * tileSize;

                ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];
                vData.Position = worldPos.ToVec3(heightSample);
                vData.Normal = Vector3.Zero;

                //Vector2 percent = (tileCoord + Vector2.One) / (ChunkSize + Vector2.One);
                vData.Color = Color.WhiteUint;// Color.Lerp(Color.Black, Color.White, (heightSample * 20) / 280).ToUint();
                //vData.UV = new Vector2(1.0f - (x / ChunkSize.X), 1.0f - (y / ChunkSize.Y));

                Vector2 mapSize = new Vector2(533.3333f, 533.33105f);
                vData.UV = new Vector2(1.0f - worldPos.X / mapSize.X, 1.0f - worldPos.Y / mapSize.Y);

                if (vIdx == 0)
                {
                    min = vData.Position;
                    max = vData.Position;
                }
                else
                {
                    min = Vector3.Min(min, vData.Position);
                    max = Vector3.Max(max, vData.Position);
                }

                vIdx++;
            }
        }
        uint verticesGenerated = (uint)vIdx;

        // Generate vertex normals
        if (HEIGHT_BASED_VERTEX_NORMALS)
        {
            float[] dataTopRight = GetChunk(chunkCoord + new Vector2(1, -1))?.GetRawData() ?? Array.Empty<float>();
            float[] dataBottomLeft = GetChunk(chunkCoord + new Vector2(-1, 1))?.GetRawData() ?? Array.Empty<float>();
            float[] dataBottomRight = GetChunk(chunkCoord + new Vector2(1, 1))?.GetRawData() ?? Array.Empty<float>();

            float[] dataRight = GetChunk(chunkCoord + new Vector2(1, 0))?.GetRawData() ?? Array.Empty<float>();
            float[] dataBottom = GetChunk(chunkCoord + new Vector2(0, 1))?.GetRawData() ?? Array.Empty<float>();

            vIdx = 0;
            for (int y = -1; y < ChunkSize.Y; y++)
            {
                for (int x = -1; x < ChunkSize.X; x++)
                {
                    Vector2 tilePos = new Vector2(x, y);
                    ref VertexData_Pos_UV_Normal_Color vData = ref vertices[vIdx];

                    // Blur normals using a box blur
                    // Each normal sample is also determined from the height of surrounding heights.
                    float blurKernel = 1; // 3-9 is a good range, use only odd numbers

                    if (blurKernel == 1)
                    {
                        Vector3 normalAtSample = SampleNormalMap(
                            tileSize,
                            ChunkSize,
                            (int)tilePos.X,
                            (int)tilePos.Y,
                            dataMe: dataMe,

                            dataTopLeft,
                            dataTop,
                            dataLeft,
                            dataBottomLeft,
                            dataBottom,
                            dataRight,
                            dataTopRight,
                            dataBottomRight
                        );

                        vData.Normal = Vector3.Normalize(normalAtSample);
                    }
                    else
                    {
                        // todo: 3 for low, 5 for medium, 7 for high
                        float range = (blurKernel - 1) / 2f;
                        float ratio = 1f / (blurKernel * blurKernel);
                        Vector3 accum = Vector3.Zero;
                        for (float sY = -range; sY <= range; sY++)
                        {
                            for (float sX = -range; sX <= range; sX++)
                            {
                                Vector2 samplePos = tilePos + new Vector2(sY, sX);
                                var normalAtSample = SampleNormalMap(
                                    tileSize,
                                    ChunkSize,
                                    (int)samplePos.X,
                                    (int)samplePos.Y,
                                    dataMe: dataMe,

                                    dataTopLeft,
                                    dataTop,
                                    dataLeft,
                                    dataBottomLeft,
                                    dataBottom,
                                    dataRight,
                                    dataTopRight,
                                    dataBottomRight
                                );
                                accum += normalAtSample * ratio;
                            }
                        }

                        vData.Normal = Vector3.Normalize(accum);
                    }

                    vIdx++;
                }
            }
        }

        // The indices used are the same for all chunks
        AssertNotNull(_indices);
        AssertNotNull(_indexBuffer);
        chunk.SetIndices(_indices, _indexBuffer, _indicesLength);
        chunk.VerticesUsed = verticesGenerated;
        chunk.Bounds = Cube.FromMinAndMax(min, max);
        chunk.VerticesGeneratedForVersion = chunkVersionUpdateTo;

#if DEBUG
        Interlocked.Decrement(ref chunk.DEBUG_UpdateVerticesThreadCount);
        Assert(chunk.DEBUG_UpdateVerticesThreadCount == 0);
#endif
    }

    private static Vector2[] _heightSamplePattern =
{
        new Vector2(-1,  0),
        new Vector2( 1,  0),

        new Vector2( 0, -1),
        new Vector2( 0,  1),

        new Vector2( 0,  0),
    };

    [ThreadStatic]
    private static float[]? _samplesAroundReusableStorage;

    private static Vector3 SampleNormalMap(
        Vector2 tileSize,
        Vector2 chunkSize,
        int x,
        int y,
        float[] dataMe,

        float[] dataTopLeft,
        float[] dataTop,
        float[] dataLeft,
        float[]? dataBottomLeft,
        float[]? dataBottom,
        float[]? dataRight,
        float[]? dataTopRight,
        float[]? dataBottomRight
    )
    {
        float[] heightSamples;

        // Initialize thread static
        if (_samplesAroundReusableStorage == null)
        {
            _samplesAroundReusableStorage = new float[_heightSamplePattern.Length];
            heightSamples = _samplesAroundReusableStorage;
        }
        else
        {
            heightSamples = _samplesAroundReusableStorage;
        }

        for (int i = 0; i < _heightSamplePattern.Length; i++)
        {
            Vector2 sampleOffset = _heightSamplePattern[i];
            float heightSample = SampleHeightMap(
                chunkSize,
                (int)(x + sampleOffset.X),
                (int)(y + sampleOffset.Y),
                dataMe: dataMe,

                dataTopLeft,
                dataTop,
                dataLeft,
                dataBottomLeft,
                dataBottom,
                dataRight,
                dataTopRight,
                dataBottomRight
            );

            heightSamples[i] = heightSample;
        }

        float left = heightSamples[0];
        float right = heightSamples[1];
        float up = heightSamples[2];
        float down = heightSamples[3];
        float me = heightSamples[4];

        float dzdx = (right - left) / (2.0f * tileSize.X);
        float dzdy = (down - up) / (2.0f * tileSize.X);
        Vector3 normal = new Vector3(-dzdx, -dzdy, 1.0f);
        return Vector3.Normalize(normal);
    }

    protected static float SampleHeightMap(
            Vector2 chunkSize,
            int relativeX,
            int relativeY,
            float[] dataMe,

            // Needed for stiching
            float[] dataTopLeft,
            float[] dataTop,
            float[] dataLeft,

            // Used for normal sampling
            float[]? dataBottomLeft = null,
            float[]? dataBottom = null,
            float[]? dataRight = null,
            float[]? dataTopRight = null,
            float[]? dataBottomRight = null
        )
    {
        int chunkCoordX = relativeX < 0 ? -1 : relativeX >= chunkSize.X ? 1 : 0;
        int chunkCoordY = relativeY < 0 ? -1 : relativeY >= chunkSize.Y ? 1 : 0;

        float[]? array = null;
        int x = relativeX;
        int y = relativeY;
        if (chunkCoordX == 0 && chunkCoordY == 0)
        {
            array = dataMe;
        }
        else
        {
            if (chunkCoordX == -1)
                x = (int)(chunkSize.X + relativeX);
            else if (chunkCoordX == 1)
                x = (int)(relativeX - chunkSize.X);

            if (chunkCoordY == -1)
                y = (int)(chunkSize.Y + relativeY);
            else if (chunkCoordY == 1)
                y = (int)(relativeY - chunkSize.Y);

            if (chunkCoordX == -1 && chunkCoordY == -1)
                array = dataTopLeft;
            else if (chunkCoordX == -1 && chunkCoordY == 0)
                array = dataLeft;
            else if (chunkCoordX == -1 && chunkCoordY == 1)
                array = dataBottomLeft;

            else if (chunkCoordX == 0 && chunkCoordY == -1)
                array = dataTop;
            //else if (chunkCoordX == 0 && chunkCoordY == 0)
            //    array = dataMe;
            else if (chunkCoordX == 0 && chunkCoordY == 1)
                array = dataBottom;

            else if (chunkCoordX == 1 && chunkCoordY == -1)
                array = dataTopRight;
            else if (chunkCoordX == 1 && chunkCoordY == 0)
                array = dataRight;
            else if (chunkCoordX == 1 && chunkCoordY == 1)
                array = dataBottomRight;
        }

        if (array == null || array.Length == 0)
            return 0;

        int dataOffset = GridHelpers.GetCoordinate1DFrom2D(new Vector2(x, y), chunkSize);
        return array[dataOffset];
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

        int vertexCount = (int)(ChunkSize.X * ChunkSize.Y);
        int stichingVertices = (int)(ChunkSize.X + ChunkSize.Y + 1);
        vertexCount += stichingVertices;

        // ChunkSize - 1 + stiching 1
        int quads = (int)(ChunkSize.X * ChunkSize.Y);
        int stride = (int)ChunkSize.X + 1;
        int indexCount = quads * 6;

        _indexBuffer ??= new IndexBuffer();

        ushort[] indices = new ushort[indexCount];

        int indexOffset = 0;
        bool flipX = false;
        for (int i = 0; i < vertexCount - stride; i++)
        {
            if ((i + 1) % stride == 0)
            {
                // New line - flip the x so that it forms a star
                //flipX = !flipX;
                continue;
            }

            if (flipX)
            {
                indices[indexOffset + 0] = (ushort)(i + stride);
                indices[indexOffset + 1] = (ushort)(i + stride + 1);
                indices[indexOffset + 2] = (ushort)(i + 1);

                indices[indexOffset + 3] = (ushort)(i + 1);
                indices[indexOffset + 4] = (ushort)i;
                indices[indexOffset + 5] = (ushort)(i + stride);
            }
            else
            {
                indices[indexOffset + 0] = (ushort)(i + stride);
                indices[indexOffset + 1] = (ushort)(i + stride + 1);
                indices[indexOffset + 2] = (ushort)i;

                indices[indexOffset + 3] = (ushort)i;
                indices[indexOffset + 4] = (ushort)(i + stride + 1);
                indices[indexOffset + 5] = (ushort)(i + 1);
            }

            indexOffset += 6;

            //flipX = !flipX;
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

        TerrainMeshGridChunk? chunk00 = GetChunkAt(floorPos, out Vector2 relCoord00);
        TerrainMeshGridChunk? chunk10 = GetChunkAt(new Vector2(ceilPos.X, floorPos.Y), out Vector2 relCoord10);
        TerrainMeshGridChunk? chunk01 = GetChunkAt(new Vector2(floorPos.X, ceilPos.Y), out Vector2 relCoord01);
        TerrainMeshGridChunk? chunk11 = GetChunkAt(ceilPos, out Vector2 relCoord11);

        float h00 = chunk00 != null ? GetAtForChunk(chunk00, relCoord00) : 0;
        float h10 = chunk10 != null ? GetAtForChunk(chunk10, relCoord10) : 0;
        float h01 = chunk01 != null ? GetAtForChunk(chunk01, relCoord01) : 0;
        float h11 = chunk11 != null ? GetAtForChunk(chunk11, relCoord11) : 0;

        float fracX = tilePos.X - floorPos.X;
        float fracY = tilePos.Y - floorPos.Y;

        float h0 = Maths.Lerp(h00, h10, fracX); // bottom
        float h1 = Maths.Lerp(h01, h11, fracX); // top
        float height = Maths.Lerp(h0, h1, fracY); // both

        return height;
    }

    #endregion

    #region Brush

    private bool _editorBrush;
    private float _editorBrushSize;
    private Vector2 _editorBrushPosition;

    public void SetEditorBrush(bool enabled, float brushSize)
    {
        _editorBrush = enabled;
        _editorBrushSize = brushSize;
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

            foreach (TerrainMeshGridChunk chunkToRender in _renderThisPass)
            {
                VertexDataAllocation vertices = chunkToRender.VertexMemory;
                if (!vertices.Allocated) continue;

                ushort[]? indices = chunkToRender.CPUIndexBuffer;
                if (indices == null) continue;

                if (mouseRay.IntersectWithVertices(indices, vertices, out Vector3 collisionPoint, out _, out _))
                    brushPoint = collisionPoint.ToVec2();
            }
        }

        _editorBrushPosition = brushPoint;
        return _editorBrushPosition;
    }

    #endregion
}