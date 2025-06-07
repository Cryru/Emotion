using Emotion.Common;
using Emotion.Game.World2D.Tile;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.TileMap;
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
        map.TileMapData = new GameMapTileData();
        map.TileMapData.Layers.Add(new TileMapLayer() { TileSize = new Vector2(18) });
        map.TileMapData.Tilesets.Add(new TileMapTileset() { Texture = "Test/bad_ice/tiles.png", TileSize = new Vector2(18), BilinearFilterTexture = false });

        Map = map;

        var iceBlock = new Iceblock();
        map.AddObject(iceBlock);

        yield break;
    }

    public override void RenderScene(RenderComposer c)
    {
        base.RenderScene(c);
    }
}
