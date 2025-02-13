#nullable enable

using Emotion.Game.World3D;
using Emotion.Graphics.Data;
using System.Threading.Tasks;

namespace Emotion.Game.World.Grid;

public class TerrainHeightMapGrid : TextureSourceGrid
{
    private TerrainChunkObject[]? _chunks;
    
    public TerrainHeightMapGrid(string texturePath) : base(texturePath)
    {
    }

    // serialization constructor
    protected TerrainHeightMapGrid()
    {

    }

    public override async Task LoadAsync(BaseMap map)
    {
        await base.LoadAsync(map);

        int chunkSize = TerrainChunkObject.CHUNK_SIZE;
        Vector2 meshChunks = (SizeInTiles / new Vector2(chunkSize)).Ceiling();
        TerrainChunkObject[] chunks = new TerrainChunkObject[(int)(meshChunks.X * meshChunks.Y)];

        int chunkId = 0;
        for (int y = 0; y < meshChunks.Y; y++)
        {
            for (int x = 0; x < meshChunks.X; x++)
            {
                chunks[chunkId] = new TerrainChunkObject(new Vector2(x, y), SizeInTiles, _data);
                chunkId++;
            }
        }
        _chunks = chunks;

        for (int i = 0; i < _chunks.Length; i++)
        {
            var chunk = chunks[i];

            if (false)
            {
                Vector2 mapCenter = map.MapSize / 2f;
                Vector2 moveOffset = new Vector2(-1, 1);
                chunk.SetChunkOffsetAndScale((mapCenter * moveOffset).ToVec3(), chunk.Size3D.ToVec2());
            }

            map.AddObject(chunk);
        }
    }
}

public class TerrainChunkObject : GameObject3D
{
    public const int CHUNK_SIZE = 9;

    private Vector2 _chunkCoord;
    private Vector2 _terrainSize;
    private byte[] _terrainData;
    public TerrainChunkObject(Vector2 chunkCoord, Vector2 terrainSize, byte[] terrainData)
    {
        _chunkCoord = chunkCoord;
        _terrainSize = terrainSize;
        _terrainData = terrainData;

        GenerateChunkEntity();

        SetChunkOffsetAndScale(Vector3.Zero, new Vector2(10));
        ObjectFlags = ObjectFlags.Map3DDontThrowShadow | ObjectFlags.Map3DDontReceiveAmbient; // temp until normals are fixed
    }

    public void SetChunkOffsetAndScale(Vector3 offset, Vector2 scale)
    {
        Vector3 chunkPosOffset = (_chunkCoord * new Vector2(CHUNK_SIZE, -CHUNK_SIZE) * scale).ToVec3();
        Size3D = scale.ToVec3(1f);
        Position = chunkPosOffset + offset;
    }
    public void GenerateChunkEntity()
    {
        int samplesToTake = CHUNK_SIZE + 1; // adding one stitch sample
        VertexData[] vertices = new VertexData[samplesToTake * samplesToTake];
        VertexDataMesh3DExtra[] verticesExtraData = new VertexDataMesh3DExtra[vertices.Length];

        int samplesPerChunk = CHUNK_SIZE;
        Vector2 sampleOffset = _chunkCoord * new Vector2(samplesPerChunk);

        int vIdx = 0;
        for (int y = 0; y < samplesToTake; y++)
        {
            for (int x = 0; x < samplesToTake; x++)
            {
                int sampleX = (int)(sampleOffset.X + x);
                int sampleY = (int)(sampleOffset.Y + y);
                int sampleOneD = (int)((sampleY * _terrainSize.X) + sampleX);
                byte sample = sampleOneD < _terrainData.Length ? _terrainData[sampleOneD] : (byte)0;

                ref VertexData vData = ref vertices[vIdx];
                vData.Vertex = new Vector3(x, -y, sample);
                vData.Color = Color.Lerp(Color.Black, Color.White, sample / 255.0f).ToUint();
                vData.UV = Vector2.Zero;

                ref VertexDataMesh3DExtra vDataExtra = ref verticesExtraData[vIdx];
                vDataExtra.Normal = new Vector3(0, 0, 1f);

                vIdx++;
            }
        }

        int quadCount = CHUNK_SIZE;
        ushort[] indices = new ushort[quadCount * quadCount * 6];
        int indexOffset = 0;
        for (int qy = 0; qy < quadCount; qy++)
        {
            for (int qx = 0; qx < quadCount; qx++)
            {
                int i = (qy * samplesToTake) + qx;

                indices[indexOffset + 0] = (ushort)(i);
                indices[indexOffset + 1] = (ushort)(i + samplesToTake + 1);
                indices[indexOffset + 2] = (ushort)(i + 1);

                indices[indexOffset + 3] = (ushort)(i);
                indices[indexOffset + 4] = (ushort)(i + samplesToTake);
                indices[indexOffset + 5] = (ushort)(i + samplesToTake + 1);

                indexOffset += 6;
            }
        }

        Mesh chunkMesh = new Mesh(vertices, verticesExtraData, indices);
        Entity = new Graphics.ThreeDee.MeshEntity()
        {
            Name = $"TerrainChunk_{_chunkCoord}",
            Meshes = new Mesh[] { chunkMesh },
        };
    }
}