#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.Tools.InterfaceTool;
using Emotion.Game.Systems.UI;
using Emotion.Game.World.Terrain;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Game.World.Terrain.MeshGridStreaming;

namespace Emotion.Editor.Tools.ChunkStreamVisualizer;

public class ChunkStreamVisualizer : EditorWindow
{
    private ITerrainGrid3D _grid;

    public ChunkStreamVisualizer(ITerrainGrid3D grid) : base("Chunk Stream Visualizer")
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

    protected void RenderChunkStreamData(UIBaseWindow viewport, Renderer c)
    {
        int renderable = 0;
        int simulated = 0;
        int loaded = 0;

        Vector2 chunkSize = (_grid.ChunkSize * _grid.TileSize).Round();
        foreach (var item in _grid.DebugOnly_ForEachStreamableChunk())
        {
            Vector2 chunkCoord = item.Item1;
            Vector2 chunkPosWorld = chunkCoord * chunkSize;
            IStreamableGridChunk chunk = item.Item2;

            ChunkState chunkState = chunk.State;
            Color colorShouldBe = GetChunkColorFromState(chunkState);
            c.RenderSprite(chunkPosWorld.ToVec3(0), new Vector2(chunkSize.X / 2f, chunkSize.Y), colorShouldBe);

            ChunkState stateItThinksItsIn = chunk.DebugOnly_CalculatedState;
            Color colorIs = GetChunkColorFromState(stateItThinksItsIn);
            c.RenderSprite(chunkPosWorld.ToVec3(0) + new Vector3(chunkSize.X / 2f, 0, 0), new Vector2(chunkSize.X / 2f, chunkSize.Y), colorIs);

            if (chunk.AwaitingUpdate)
                c.RenderSprite(chunkPosWorld, chunkSize, Color.Blue * 0.5f);
            if (chunk.Busy)
                c.RenderSprite(chunkPosWorld, chunkSize, Color.Blue * 0.5f);

            if (chunkState == ChunkState.HasGPUData)
                renderable++;

            if (chunkState == ChunkState.HasGPUData || chunkState == ChunkState.HasMesh)
                simulated++;

            loaded++;
        }

        c.RenderGrid(-(chunkSize * 100).ToVec3(), chunkSize * 200, chunkSize, Color.Black, chunkSize / 2f + Vector2.One / 2f);

        int rangeSim = _grid.SimulationRange;
        int rangeRender = _grid.RenderRange;
        foreach (Vector2 actorPos in _grid.DebugOnly_ForEachStreamActorPos())
        {
            c.RenderCircle(actorPos.ToVec3(), 5, Color.Magenta);
            c.RenderCircleOutline(actorPos.ToVec3(), rangeSim, Color.Yellow, true);
            c.RenderCircleOutline(actorPos.ToVec3(), rangeRender, Color.Green, true);
        }

        c.RenderString(Vector3.Zero, Color.Black, $"Renderable: {renderable}\nSimulated: {simulated}\nLoaded: {loaded}", FontAsset.GetDefaultBuiltIn().GetAtlas(15));
    }
}
