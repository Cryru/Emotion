#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.Animation.TwoDee;
using Emotion.Game.Systems.UI;
using Emotion.Graphics.Assets;

namespace Emotion.Game.Systems.UI2.Editor;

public class UIPicture : UIBaseWindow
{
    public enum UIPictureMode
    {
        Expand,
        Stretch
    }

    public UIPictureMode TextureFitMode
    {
        get => _textureFitMode;
        set
        {
            if (value == _textureFitMode) return;
            _textureFitMode = value;
            InvalidateLayout();
        }
    }

    private UIPictureMode _textureFitMode = UIPictureMode.Expand;

    public AssetOrObjectReference<TextureAsset, Texture> Texture
    {
        get => _textureBacking;
        set
        {
            _textureBacking = value;
            InvalidateLoaded();
        }
    }
    private AssetOrObjectReference<TextureAsset, Texture> _textureBacking = AssetOrObjectReference<TextureAsset, Texture>.Invalid;
    private AssetOrObjectReference<TextureAsset, Texture>? _loadedTexture = null;

    /// <summary>
    /// An override scale for the image. By default it is drawn either in full size, uv size, or render size.
    /// </summary>
    public Vector2? ImageScale;

    /// <summary>
    /// Whether to enable smoothing on the texture. This is only done when loaded and will overwrite the
    /// texture setting for other users using the texture.
    /// </summary>
    public bool Smooth;

    public Color ImageColor = Color.White;

    #region UV

    /// <summary>
    /// Custom UVs for the texture. By default the whole texture is drawn.
    /// </summary>
    public Rectangle? UV
    {
        get => _uvBacking;
        set
        {
            _uvBacking = value;
            InvalidateLayout();
        }
    }

    private Rectangle? _uvBacking;

    // <summary>
    /// The column of the texture to display.
    /// </summary>
    public int Column
    {
        get => _columnBacking;
        set
        {
            _columnBacking = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// The row of the texture to display.
    /// </summary>
    public int Row
    {
        get => _rowBacking;
        set
        {
            _rowBacking = value;
            InvalidateLayout();
        }
    }

    private int _columnBacking = 1;
    private int _rowBacking = 1;

    /// <summary>
    /// The total number of columns in the texture.
    /// </summary>
    public int Columns = 1;

    /// <summary>
    /// The total number of rows in the texture.
    /// </summary>
    public int Rows = 1;

    /// <summary>
    /// Space between rows and columns in texture.
    /// </summary>
    public Vector2 RowAndColumnSpacing = Vector2.Zero;

    public bool FlipX;

    protected Rectangle? _renderWithUV;

    #endregion

    public UIPicture()
    {
    }

    protected override void OnClose()
    {
        base.OnClose();

        // Dispose of the loaded asset
        if (_loadedTexture != null)
        {
            _loadedTexture.Cleanup();
            _loadedTexture = null;
        }
    }

    #region Loading

    protected override Coroutine? InternalLoad()
    {
        if (Texture.ReadyToUse() || !Texture.IsValid())
            return null;

        // Dispose of the old one.
        _loadedTexture?.Cleanup();
        _loadedTexture = null;
        return Engine.CoroutineManager.StartCoroutine(LoadAsset());
    }

    private IEnumerator LoadAsset()
    {
        yield return 1000;
        yield return Texture.PerformLoading(this, ProxyInvalidateLayout);
        _loadedTexture = Texture;
        InvalidateLayout();
    }

    #endregion

    protected override IntVector2 InternalGetWindowMinSize()
    {
        Assert(!IsLoading());

        Texture? texture = Texture.GetObject();
        if (texture == null)
            return IntVector2.Zero;

        if (Smooth != texture.Smooth)
            texture.Smooth = Smooth;

        Vector2 textureSize = texture.Size;
        if (_uvBacking.HasValue)
        {
            textureSize = _uvBacking.Value.Size;
            _renderWithUV = _uvBacking;
        }
        else if (Rows != 1 || Columns != 1)
        {
            var colRowVec = new Vector2(Columns, Rows);
            Vector2 frameSize = (textureSize - RowAndColumnSpacing * colRowVec) / colRowVec;

            int rowIdx = Maths.Clamp(_rowBacking, 1, Rows) - 1;
            int columnIdx = Maths.Clamp(_columnBacking, 1, Columns) - 1;
            Rectangle uv = Animation2DHelpers.GetGridFrameBounds(textureSize, frameSize, RowAndColumnSpacing, rowIdx, columnIdx);

            textureSize = uv.Size;
            _renderWithUV = uv;
        }
        else
        {
            _renderWithUV = null;
        }

        if (ImageScale.HasValue)
            textureSize *= ImageScale.Value;

        textureSize *= CalculatedMetrics.Scale;

        switch (TextureFitMode)
        {
            case UIPictureMode.Expand:
                return IntVector2.FromVec2Ceiling(textureSize);
            case UIPictureMode.Stretch:
                return IntVector2.Zero;
        }

        return IntVector2.Zero;
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);
        r.RenderSprite(CalculatedMetrics.Position.ToVec3(), CalculatedMetrics.Size.ToVec2(), ImageColor, Texture.GetObject(), _renderWithUV, FlipX);
    }
}
