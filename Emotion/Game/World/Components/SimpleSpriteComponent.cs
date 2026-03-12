#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Systems;
using Emotion.Graphics.Assets;

namespace Emotion.Game.World.Components;

public class SimpleSpriteComponent :
    IGameObjectComponent,
    IGameObjectTransformProvider,
    ISystemicComponent<SimpleSpriteComponent, SimpleSpriteRenderSystem>
{
    public GameObject Object { get; private set; } = null!;
    public Vector3 CalculatedOffset { get; private set; }

    public RectangleAnchor SpriteAnchor { get; init; } = RectangleAnchor.TopLeft;
    public Texture Texture { get => _texture; }
    private AssetOwner<TextureAsset, Texture> _textureOwner = new();

    private Texture _texture = null!;

    public SimpleSpriteComponent(TextureReference texture, RectangleAnchor anchor = RectangleAnchor.TopLeft)
    {
        _textureOwner.SetOnChangeCallback(static (_, component) => (component as SimpleSpriteComponent)?.OnTextureChanged(), this);
        _textureOwner.Set(texture, true);
        SpriteAnchor = anchor;
    }

    public virtual IRoutineWaiter? Init(GameObject obj)
    {
        Object = obj;
        return _textureOwner.GetCurrentLoading();
    }

    public virtual void Done(GameObject obj)
    {
        _textureOwner.Done();
    }

    #region Texture Setting

    public Coroutine? SetTexture(TextureReference entity)
    {
        return _textureOwner.Set(entity);
    }

    protected virtual void OnTextureChanged()
    {
        Texture? texture = _textureOwner.GetCurrentObject();
        if (texture != null) texture.Smooth = true;
        _texture = texture ?? Texture.EmptyWhiteTexture;

        var rect = new Rectangle(0, 0, _texture.Size);
        Vector2 offset = -rect.GetRectangleAnchorSpot(SpriteAnchor);
        CalculatedOffset = offset.ToVec3();
        _boundingRectBase = rect.Offset(offset);

        Object.InvalidateModelMatrix();
    }

    #endregion

    #region Model Matrix

    public Vector3 GetForwardModelSpace()
    {
        return new Vector3(1, 0, 0);
    }

    public void OnModelMatrixInvalidated()
    {
        _dirtyBoundingRect = true;
    }

    public Matrix4x4 CalculateModelMatrix(GameObject obj, out Matrix4x4 scaleMatrix, out Matrix4x4 rotationMatrix, out Matrix4x4 translationMatrix)
    {
        float entityScale = 1f;//_entity.Scale * RenderState.Scale;
        Matrix4x4 entityLocalTransform = Matrix4x4.Identity;// _entity.LocalTransform;

        Vector3 objScale = obj.Scale3D;
        scaleMatrix = Matrix4x4.CreateScale(objScale.X * entityScale, objScale.Y * entityScale, objScale.Z * entityScale);

        Vector3 objRotation = obj.Rotation;
        rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(objRotation.Y, objRotation.X, objRotation.Z);

        translationMatrix = Matrix4x4.CreateTranslation(obj.Position3D);
        return entityLocalTransform * scaleMatrix * rotationMatrix * translationMatrix;
    }

    protected Rectangle _boundingRect;
    protected Rectangle _boundingRectBase;
    protected bool _dirtyBoundingRect = true;

    public Rectangle GetBoundingRect(GameObject obj)
    {
        if (_dirtyBoundingRect)
        {
            _boundingRect = Rectangle.Transform(_boundingRectBase, obj.GetModelMatrix());
            _dirtyBoundingRect = false;
        }

        return _boundingRect;
    }

    public Sphere GetBoundingSphere(GameObject obj)
    {
        Rectangle rect = GetBoundingRect(obj);
        return new Sphere(rect.Center.ToVec3(), MathF.Max(rect.Width, rect.Height) / 2f);
    }

    public Cube GetBoundingCube(GameObject obj)
    {
        Rectangle rect = GetBoundingRect(obj);
        return new Cube(rect.Center.ToVec3(), new Vector3(rect.Width / 2f, rect.Height / 2f, 1));
    }

    #endregion
}
