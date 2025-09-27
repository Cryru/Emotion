#nullable enable

namespace Emotion.Game.Systems.UI;

public class UITriSlice : UITexture
{
    public Vector2 LeftRight;

    private Rectangle _leftUV;
    private Vector3 _leftDraw;
    private Vector2 _leftDrawSize;

    private Rectangle _rightUV;
    private Vector3 _rightDraw;
    private Vector2 _rightDrawSize;

    private Rectangle _tileUV;
    private Vector3 _tileStartDraw;
    private Vector2 _tileDrawSize;

    private Vector2 _tileDrawAreaSize;

    protected override void AfterLayout()
    {
        base.AfterLayout();
        if (TextureAsset == null) return;

        Vector2 imageScale = (ImageScale ?? Vector2.One) * GetScale();

        Vector2 textureSize = TextureAsset.Texture.Size;

        float leftSize = LeftRight.X * imageScale.X;
        _leftUV = new Rectangle(0, 0, LeftRight.X, textureSize.Y);
        _leftDraw = Position;
        _leftDrawSize = new Vector2(leftSize, Height);

        float rightSize = LeftRight.Y * imageScale.X;
        _rightUV = new Rectangle(textureSize.X - LeftRight.Y, 0, LeftRight.Y, textureSize.Y);
        _rightDraw = Position + new Vector3(Width - rightSize, 0, 0);
        _rightDrawSize = new Vector2(rightSize, Height);

        _tileUV = new Rectangle(LeftRight.X, 0, textureSize.X - LeftRight.X - LeftRight.Y, textureSize.Y);
        _tileStartDraw = Position + new Vector3(leftSize, 0, 0);
        _tileDrawSize = new Vector2(Width - leftSize - rightSize, Height);
        _tileDrawAreaSize = new Vector2(Width - leftSize - rightSize, Height);

        base.AfterLayout();
    }

    private void DrawTiledInside(Renderer composer, Vector3 startPos, Vector2 tileSize, Vector2 tileArea, Rectangle uv)
    {
        if (tileSize.X <= 0) return;

        float rightX = startPos.X + tileArea.X;
        for (float x = startPos.X; x < rightX; x += tileSize.X)
        {
            float widthLeft = MathF.Min(rightX - x, tileSize.X);
            Rectangle tileUv = uv;
            if (widthLeft < tileSize.X) tileUv.Width = tileUv.Width * widthLeft / tileSize.X;

            composer.RenderSprite(new Vector3(x, startPos.Y, Z), new Vector2(widthLeft, tileSize.Y), _calculatedColor, TextureAsset.Texture, tileUv);
        }
    }

    protected override Vector2 InternalMeasure(Vector2 space)
    {
        return Vector2.Zero;
    }

//#if !NEW_UI
//    protected override Vector2 NEW_InternalMeasure(Vector2 space)
//    {
//        return InternalMeasure(space);
//    }
//#endif

    protected override bool RenderInternal(Renderer composer)
    {
        if (TextureAsset == null) return false;

        DrawTiledInside(composer, _tileStartDraw, _tileDrawSize, _tileDrawAreaSize, _tileUV);
        composer.RenderSprite(_leftDraw, _leftDrawSize, _calculatedColor, TextureAsset.Texture, _leftUV);
        composer.RenderSprite(_rightDraw, _rightDrawSize, _calculatedColor, TextureAsset.Texture, _rightUV);

        return true;
    }
}