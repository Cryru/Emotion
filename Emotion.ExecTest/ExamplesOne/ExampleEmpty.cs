#nullable enable

using Emotion.Core;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Scenography;
using Emotion.ExecTest.Experiment;
using Emotion.Game.Systems.UI;
using Emotion.Game.World;
using Emotion.Game.World.TileMap;
using Emotion.Graphics;
using Emotion.Primitives;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.ExamplesOne;

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        GameMap map = new GameMap();

        map.TileMapData.AddTileset(new TileMapTileset()
        {
            Texture = "Test/bad_ice/tiles.png",
            TileSize = new Vector2(18),
            BilinearFilterTexture = false
        });

        var layer = new TileMapLayer()
        {
            Name = "Pepegich",
            TileSize = new Vector2(18)
        };
        layer.ExpandingSetAt(new Vector2(-1, 0), new TileMapTile(6, 0));
        layer.ExpandingSetAt(Vector2.Zero, new TileMapTile(7, 0));
        layer.ExpandingSetAt(new Vector2(1, 0), new TileMapTile(8, 0));
        map.AddGrid(layer);

        GameMapAsset ass = GameMapAsset.CreateFromMap(map, "TestMap");
        ass.Save("TestMap");

        GameMapAsset gameMapAsset = Engine.AssetLoader.ONE_Get<GameMapAsset>("TestMap.gamemap");
        yield return SetCurrentMap(gameMapAsset);
    }

    public override void RenderScene(Renderer c)
    {
        base.RenderScene(c);

        //FontAsset font = FontAsset.GetDefaultBuiltIn();
        //GPURenderTextExperiment.RenderFont(new Vector2(15, 15), "This is a test", font.Font, 15f, Color.White);
    }
}
