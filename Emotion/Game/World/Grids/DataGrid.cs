using Emotion.Graphics.Camera;
using Emotion.Primitives.Grids;
using Emotion.Primitives.Grids.Chunked;

namespace Emotion.Game.World.Grids;

public class DataGrid<T> : ChunkedGrid<T, GenericGridChunk<T>>, IMapGrid, IGridWorldSpaceTiles
    where T : unmanaged
{
    public Vector2 TileSize { get; set; }

    public DataGrid(Vector2 tileSize, float chunkSize) : base(chunkSize)
    {
        TileSize = tileSize;
    }

    public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");

    public void Done()
    {

    }

    public IEnumerator InitRoutine(GameMap.GridFriendAdapter mapAdapter)
    {
        yield break;
    }

    public void Render(GameMap map, Renderer r, CameraCullingContext culling)
    {
        //foreach (KeyValuePair<Vector2, GenericGridChunk<T>> chunkPair in _chunks)
        //{
        //    GenericGridChunk<T> chunk = chunkPair.Value;
        //    for (int y = 0; y < ChunkSize.Y; y++)
        //    {
        //        for (int x = 0; x < ChunkSize.X; x++)
        //        {
        //            T val = GetAtForChunk(chunk, new Vector2(x, y));
        //            //r.RenderSprite((new Vector2(x, y) * TileSize).ToVec3(), TileSize, valAsbool ? Color.Green : Color.Red);
        //        }
        //    }
        //}
    }

    public void Update(float dt)
    {

    }
}
