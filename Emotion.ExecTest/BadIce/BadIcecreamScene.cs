using Emotion.Core;
using Emotion.Core.Systems.Scenography;
using Emotion.Game.World.TileMap;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Game.World;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.BadIce;

public class BadIcecreamScene : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        var camera = new PixelArtCamera(new Vector3(0));
        camera.Zoom = 2f;
        Engine.Renderer.Camera = camera;

        var map = new GameMap();
        var tileMap = new TileMapGrid();
        tileMap.Layers.Add(new TileMapLayer() { TileSize = new Vector2(18) });
        tileMap.Tilesets.Add(new TileMapTileset() { Texture = "Test/bad_ice/tiles.png", TileSize = new Vector2(18), BilinearFilterTexture = false });
        map.AddGrid(tileMap);

        Map = map;

        //var iceBlock = new Iceblock();
        //map.AddObject(iceBlock);

        yield break;
    }

    public override void RenderScene(Renderer c)
    {
        base.RenderScene(c);
    }
}
