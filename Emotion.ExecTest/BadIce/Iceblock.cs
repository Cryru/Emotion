using Emotion.Game.World.TwoDee;
using Emotion.Graphics;
using Emotion.Graphics.Assets;
using System.Numerics;

namespace Emotion.ExecTest.BadIce;

public class Iceblock : MapObjectSprite
{
    private TextureAsset _texture;

    //public override void LoadAssets(AssetLoader assetLoader)
    //{
    //    _texture = assetLoader.ONE_Get<TextureAsset>("Test/bad_ice/iceblock.png");

    //    base.LoadAssets(assetLoader);
    //}

    public override void Render(Renderer c)
    {
        c.RenderSprite(Vector3.Zero, _texture);
    }
}
