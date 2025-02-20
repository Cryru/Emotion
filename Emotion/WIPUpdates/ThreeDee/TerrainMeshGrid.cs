﻿#nullable enable

using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shader;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.WIPUpdates.Grids;

namespace Emotion.WIPUpdates.ThreeDee;

public class TerrainMeshGrid : ChunkedGrid<float, GenericGridChunk<float>>
{
    public Vector2 TileSize { get; private set; }

    public MeshMaterial TerrainMeshMaterial;

    public TerrainMeshGrid(Vector2 tileSize, float chunkSize) : base(chunkSize)
    {
        TileSize = tileSize;

        TerrainMeshMaterial = new MeshMaterial()
        {
            Name = "TerrainChunkMaterial",
            Shader = Engine.AssetLoader.ONE_Get<NewShaderAsset>("Shaders3D/TerrainShader.glsl")
        };
    }

    // serialization
    protected TerrainMeshGrid()
    {

    }

    // todo: 3d culling
    public void Render(RenderComposer c, Rectangle clipArea)
    {
        ResetChunksMarkedToRender();

        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = tileSize / 2f;
        Vector2 chunkWorldSize = ChunkSize * tileSize;

        Rectangle cacheAreaChunkSpace = clipArea;
        cacheAreaChunkSpace.SnapToGrid(chunkWorldSize);

        cacheAreaChunkSpace.GetMinMaxPoints(out Vector2 min, out Vector2 max);
        min /= chunkWorldSize;
        max /= chunkWorldSize;

        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 chunkCoord = new Vector2(x, y);
                GenericGridChunk<float>? chunk = GetChunk(chunkCoord);
                if (chunk == null) continue;
                MarkChunkForRender(chunk, chunkCoord);
            }
        }

        bool editorBrushEnabled = true;
        Vector2 brushWorldSpace = new Vector2(float.NaN);
        if (editorBrushEnabled)
        {
            CameraBase camera = Engine.Renderer.Camera;
            Ray3D mouseRay = camera.GetCameraMouseRay();
            Vector3 mousePosWorld = mouseRay.IntersectWithPlane(RenderComposer.Up, Vector3.Zero);

            if (_renderThisPass != null)
            {
                for (int i = 0; i < _renderThisPass.Count; i++)
                {
                    TerrainGridRenderCacheChunk chunkToRender = _renderThisPass[i];
                    Mesh? mesh = chunkToRender.CachedMesh;
                    if (mesh == null) continue;

                    if (mouseRay.IntersectWithMeshLocalSpace(mesh, out Vector3 collisionPoint, out _, out _))
                    {
                        brushWorldSpace = collisionPoint.ToVec2();
                        break;
                    }
                }
            }
        }

        if (_renderThisPass != null)
        {
            PrepareChunkRendering(c);
            c.CurrentState.Shader.SetUniformVector2("brushWorldSpace", brushWorldSpace);

            for (int i = 0; i < _renderThisPass.Count; i++)
            {
                TerrainGridRenderCacheChunk chunkToRender = _renderThisPass[i];
                Mesh? mesh = chunkToRender.CachedMesh;
                if (mesh == null) continue;

                var mem = c.RenderStream.GetStreamMemory<VertexDataWithNormal>(
                    (uint) mesh.VerticesONE.Length,
                    (uint) mesh.Indices.Length,
                    Graphics.Batches.BatchMode.SequentialTriangles
                );
                mesh.VerticesONE.CopyTo(mem.VerticesData);
                mesh.Indices.CopyTo(mem.IndicesData);

                for (int idx = 0; idx < mem.IndicesData.Length; idx++)
                {
                    mem.IndicesData[i] += mem.StructIndex;
                }
                //c.MeshEntityRenderer.RenderMeshEntityStandalone(entity, entityState);
            }

            FlushChunkRendering(c);
        }
    }

    private void PrepareChunkRendering(RenderComposer c)
    {
        Engine.Renderer.FlushRenderStream();
        Engine.Renderer.SetFaceCulling(true, true);

        AssetHandle<NewShaderAsset>? shaderHandle = TerrainMeshMaterial.Shader?.GetAssetHandle();
        NewShaderAsset? asset = shaderHandle?.Asset;
        if (asset != null && asset.CompiledShader != null)
            c.SetShader(asset.CompiledShader);

        c.CurrentState.Shader.SetUniformInt("diffuseTexture", 0);
        Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer);
    }

    private void FlushChunkRendering(RenderComposer c)
    {
        c.SetShader(null);
    }

    private List<TerrainGridRenderCacheChunk>? _renderThisPass;
    private Dictionary<Vector2, TerrainGridRenderCacheChunk> _cachedChunks = new();

    private void ResetChunksMarkedToRender()
    {
        _renderThisPass?.Clear();
    }

    private void MarkChunkForRender(GenericGridChunk<float> chunk, Vector2 chunkCoord)
    {
        _renderThisPass ??= new List<TerrainGridRenderCacheChunk>(32);

        _cachedChunks.TryGetValue(chunkCoord, out TerrainGridRenderCacheChunk? cachedChunk);
        if (cachedChunk == null)
        {
            cachedChunk = new TerrainGridRenderCacheChunk();
            _cachedChunks.Add(chunkCoord, cachedChunk);
        }
        _renderThisPass.Add(cachedChunk);

        UpdateChunkRenderCache(cachedChunk, chunk, chunkCoord);
    }

    private void UpdateChunkRenderCache(TerrainGridRenderCacheChunk chunkCache, GenericGridChunk<float> chunk, Vector2 chunkCoord)
    {
        // We already have the latest version of this
        //if (chunkCache.CachedVersion == chunk.ChunkVersion) return;
        if (chunkCache.CachedMesh != null) return;

        Vector2 tileSize = TileSize;
        Vector2 halfTileSize = TileSize / 2f;
        Vector2 chunkWorldSize = ChunkSize * tileSize;

        GenericGridChunk<float>? chunkLeft = GetChunk(chunkCoord + new Vector2(-1, 0));
        GenericGridChunk<float>? chunkTop = GetChunk(chunkCoord + new Vector2(0, -1));
        GenericGridChunk<float>? chunkDiag = GetChunk(chunkCoord + new Vector2(-1, -1));

        Vector2 chunkWorldOffset = (chunkCoord * chunkWorldSize) + halfTileSize;

        int vertexCount = (int)(ChunkSize.X * ChunkSize.Y);
        int stichingVertices = (int)(ChunkSize.X + ChunkSize.Y + 1);
        vertexCount += stichingVertices;

        var vertices = new VertexDataWithNormal[vertexCount];

        // Get data for stiching vertices
        float[] dataTop = chunkTop?.GetRawData() ?? Array.Empty<float>();
        float[] dataLeft = chunkLeft?.GetRawData() ?? Array.Empty<float>();
        float[] dataMe = chunk?.GetRawData() ?? Array.Empty<float>();

        int vIdx = 0;
        for (int y = -1; y < ChunkSize.Y; y++)
        {
            for (int x = -1; x < ChunkSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);

                float heightSample = 0;
                if (x == -1 && y == -1)
                {
                    if (chunkDiag != null)
                    {
                        float[] data = chunkDiag.GetRawData();
                        heightSample = data[^1];
                    }
                }
                else if (y == -1)
                {
                    int dataOffset = (int)((ChunkSize.Y - 1) * ChunkSize.X) + (x);
                    heightSample = dataTop.Length == 0 ? 0 : dataTop[dataOffset];
                }
                else if (x == -1)
                {
                    int dataOffset = (int)((y * ChunkSize.X) + ChunkSize.X - 1);
                    heightSample = dataLeft.Length == 0 ? 0 : dataLeft[dataOffset];
                }
                else
                {
                    int dataOffset = GridHelpers.GetCoordinate1DFrom2D(tileCoord, ChunkSize);
                    heightSample = dataMe[dataOffset];
                }

                Vector2 worldPos = chunkWorldOffset + (tileCoord * tileSize);

                ref VertexDataWithNormal vData = ref vertices[vIdx];
                vData.Vertex = worldPos.ToVec3(heightSample);

                Vector2 percent = (tileCoord + Vector2.One) / (ChunkSize + Vector2.One);
                vData.Color = Color.Lerp(Color.Black, Color.White, (heightSample + 150) / 300f).ToUint();
                vData.UV = Vector2.Zero;
                vData.Normal = new Vector3(0, 0, -1);

                vIdx++;
            }
        }

        // ChunkSize - 1 + stiching 1
        int quads = (int)(ChunkSize.X * ChunkSize.Y);
        int stride = (int)ChunkSize.X + 1;

        ushort[] indices = new ushort[quads * 6];
        int indexOffset = 0;
        for (int i = 0; i < vertexCount - stride; i++)
        {
            if ((i + 1) % stride == 0) continue;

            indices[indexOffset + 0] = (ushort)(i + stride);
            indices[indexOffset + 1] = (ushort)(i + stride + 1);
            indices[indexOffset + 2] = (ushort)(i + 1);

            indices[indexOffset + 3] = (ushort)(i + 1);
            indices[indexOffset + 4] = (ushort)(i);
            indices[indexOffset + 5] = (ushort)(i + stride);

            indexOffset += 6;
        }

        Mesh chunkMesh = new Mesh(vertices, indices)
        {
            Name = $"TerrainChunk_{chunkCoord}",
            Material = TerrainMeshMaterial
        };

        chunkCache.CachedMesh = chunkMesh;
    }

    private class TerrainGridRenderCacheChunk
    {
        public int CachedVersion = -1;
        public Mesh? CachedMesh = null;
    }
}

