using Emotion.Game.Terrain.GridStreaming;
using Emotion.IO;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.Tools.InterfaceTool;

namespace Emotion.WIPUpdates.One.Tools.ChunkStreamVisualizer;

#nullable enable

public class ChunkStreamVisualizer : EditorWindow
{
    private IStreamableGrid _grid;

    public ChunkStreamVisualizer(IStreamableGrid grid) : base("Chunk Stream Visualizer")
    {
        _grid = grid;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var contentParent = GetContentParent();

        var chunkViewport = new UIViewport
        {
            OnRender = RenderChunkStreamData,
            WindowColor = Color.CornflowerBlue
        };
        contentParent.AddChild(chunkViewport);
    }

    private Color GetChunkColorFromState(ChunkState chunkState)
    {
        Color stateColor = Color.Black;
        switch (chunkState)
        {
            case ChunkState.DataOnly:
                stateColor = Color.PrettyRed;
                break;
            case ChunkState.HasMesh:
                stateColor = Color.PrettyYellow;
                break;
            case ChunkState.HasGPUData:
                stateColor = Color.PrettyGreen;
                break;
        }
        return stateColor;
    }

    protected void RenderChunkStreamData(UIBaseWindow viewport, RenderComposer c)
    {
        int renderable = 0;
        int simulated = 0;
        int loaded = 0;

        Vector2 chunkSize = (_grid.ChunkSize * _grid.TileSize).Round();
        foreach (var item in _grid.DebugOnly_StreamableGridForEachChunk())
        {
            Vector2 chunkCoord = item.Item1;
            Vector2 chunkPosWorld = (chunkCoord * chunkSize);
            IStreamableGridChunk chunk = item.Item2;

            ChunkState chunkState = chunk.State;
            Color colorShouldBe = GetChunkColorFromState(chunkState);
            c.RenderSprite(chunkPosWorld.ToVec3(0), new Vector2(chunkSize.X / 2f, chunkSize.Y), colorShouldBe);

            ChunkState stateItThinksItsIn = chunk.DebugOnly_CalculatedState;
            Color colorIs = GetChunkColorFromState(stateItThinksItsIn);
            c.RenderSprite(chunkPosWorld.ToVec3(0) + new Vector3(chunkSize.X / 2f, 0, 0), new Vector2(chunkSize.X / 2f, chunkSize.Y), colorIs);

            if (chunk.LoadingStatePromotion)
                c.RenderRectOutline(chunkPosWorld, chunkSize, Color.Blue);

            if (chunkState == ChunkState.HasGPUData)
                renderable++;

            if (chunkState == ChunkState.HasGPUData || chunkState == ChunkState.HasMesh)
                simulated++;

            loaded++;
        }

        c.RenderGrid(-(chunkSize * 100).ToVec3(), chunkSize * 200, chunkSize, Color.Black, chunkSize / 2f + Vector2.One / 2f);

        ChunkStreamManager streamer = _grid.ChunkStreamManager;
        int rangeSim = streamer.SimulationRange;
        int rangeRender = streamer.RenderRange;
        foreach (Vector2 actorPos in streamer.DebugOnly_ForEachStreamActorPos())
        {
            c.RenderCircle(actorPos.ToVec3(), 5, Color.Magenta);
            c.RenderCircleOutline(actorPos.ToVec3(), rangeSim, Color.Pink, true);
            c.RenderCircleOutline(actorPos.ToVec3(), rangeRender, Color.Magenta, true);
        }

        c.RenderString(Vector3.Zero, Color.Black, $"Renderable: {renderable}\nSimulated: {simulated}\nLoaded: {loaded}", FontAsset.GetDefaultBuiltIn().GetAtlas(15));
    }
}
