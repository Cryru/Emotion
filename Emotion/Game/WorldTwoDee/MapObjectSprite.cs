using Emotion.Common.Serialization;
using Emotion.Game.TwoDee;
using Emotion.IO;
using Emotion.WIPUpdates.One.Work;

#nullable enable

namespace Emotion.Game.WorldTwoDee;

public class MapObjectSprite : MapObject
{
    [DontSerialize]
    public SpriteEntity Entity
    {
        get => _entity;
    }
    protected SpriteEntity _entity;

    [DontSerialize]
    public SpriteEntityMetaState? RenderState;

    public SerializableAsset<SpriteAsset>? EntityAsset;

    public MapObjectSprite()
    {
        SetEntity(SpriteEntity.MissingEntity);
        AssertNotNull(_entity);
    }

    public MapObjectSprite(string entityFile) : this()
    {
        SetEntity(entityFile);
    }

    public MapObjectSprite(SpriteEntity entity) : this()
    {
        SetEntity(entity);
    }

    #region Set Entity

    public void SetEntity(string assetPath)
    {
        SpriteAsset assetHandle = Engine.AssetLoader.ONE_Get<SpriteAsset>(assetPath, this);
        SetEntity(assetHandle);
    }

    public void SetEntity(SpriteAsset asset)
    {
        UnloadOldEntityAsset();
        EntityAsset = asset;
        asset.OnLoaded += OnEntityAssetChanged;

        // Loaded inline or already loaded.
        if (asset.Loaded)
        {
            if (asset.Entity != null)
                OnSetEntity(asset.Entity);
        }
    }

    protected void OnEntityAssetChanged(Asset asset)
    {
        if (asset is not SpriteAsset meshAsset) return;
        if (meshAsset.Entity == null) return;
        OnSetEntity(meshAsset.Entity);
    }

    public void SetEntity(SpriteEntity entity)
    {
        UnloadOldEntityAsset();
        OnSetEntity(entity);
    }

    protected void UnloadOldEntityAsset()
    {
        SpriteAsset? oldHandle = EntityAsset?.Get();
        if (oldHandle != null)
        {
            oldHandle.OnLoaded -= OnEntityAssetChanged;
            Engine.AssetLoader.RemoveReferenceFromAsset(oldHandle, this);
            EntityAsset = null;
        }
    }

    protected void OnSetEntity(SpriteEntity entity)
    {
        RenderState = null;

        _currentAnimation = null;
        _animationTime = 0;

        _entity = entity;

        if (_entity != null)
        {
            RenderState = new SpriteEntityMetaState(_entity);

            // Reset the animation (or set it to the one set before the entity was loaded).
            // This will also set the default bone matrices.
            // This will also calculate bounds
            SetAnimation(_initSetAnimation);
            _initSetAnimation = null;

            RenderState.UpdateAnimation(_currentAnimation, 0);
        }
        else
        {
            _boundingRectangleBase = new Rectangle(0, 0, 1, 1);
        }

        InvalidateModelMatrix();
    }

    #endregion

    #region Transform

    public override Rectangle BoundingRect
    {
        get
        {
            return GetBoundingRectangle();
        }
        set
        {
            Position2D = value.Center; // ?
            Assert(false, "Settings bounds on a sprite entity object? What should happen?");
        }
    }

    protected Rectangle _boundingRectangle;
    protected Rectangle _boundingRectangleBase;
    protected bool _dirtyBounds = true;

    public Rectangle GetBoundingRectangle()
    {
        if (_dirtyBounds)
        {
            _boundingRectangle = Rectangle.Transform(_boundingRectangleBase, GetModelMatrix());
            _dirtyBounds = false;
        }

        return _boundingRectangle;
    }

    protected override void InvalidateModelMatrix()
    {
        base.InvalidateModelMatrix();
        _dirtyBounds = true;
    }

    #endregion

    #region Animation and Bones

    private SpriteAnimation? _currentAnimation;
    private float _animationTime;
    private string? _initSetAnimation;

    public string GetCurrentAnimation()
    {
        return _currentAnimation?.Name ?? string.Empty;
    }

    public bool HasAnimation(string name)
    {
        if (_entity.Animations != null)
        {
            for (var i = 0; i < _entity.Animations.Count; i++)
            {
                SpriteAnimation anim = _entity.Animations[i];
                if (anim.Name == name) return true;
            }
        }
        return false;
    }

    public virtual void SetAnimation(string? name, bool forceIfMissing = false)
    {
        if (!Initialized)
        {
            _initSetAnimation = name;
            return;
        }

        // Try to find the animation instance.
        // todo: case insensitive?
        SpriteAnimation? animInstance = null;
        if (_entity.Animations != null)
        {
            // Default to first animation
            if (_entity.Animations.Count > 0)
                animInstance = _entity.Animations[0];

            if (name != null)
            {
                for (var i = 0; i < _entity.Animations.Count; i++)
                {
                    SpriteAnimation anim = _entity.Animations[i];
                    if (anim.Name == name) animInstance = anim;
                }
            }
        }
        if (animInstance == null && name != null && !forceIfMissing)
            return;

        _currentAnimation = animInstance;
        _animationTime = 0; // Reset time

        _entity.GetBounds(_currentAnimation, out Rectangle baseRect);
        _boundingRectangleBase = baseRect;
        Assert(baseRect.Size.X != 0 && baseRect.Size.Y != 0, "Entity bounds is 0?");
        InvalidateModelMatrix();
    }

    protected override Matrix4x4 UpdateModelMatrix()
    {
        _translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
        _rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        _scaleMatrix = Matrix4x4.CreateScale(_scaleX, _scaleY, _scaleZ);
        return _scaleMatrix * _rotationMatrix * _translationMatrix;
    }

    #endregion

    public override void Update(float dt)
    {
        // Update current animation
        if (_currentAnimation != null && _entity != null && RenderState != null)
        {
            _animationTime += dt;
            RenderState.UpdateAnimation(_currentAnimation, _animationTime);
        }

        base.Update(dt);
    }

    public override void Render(RenderComposer c)
    {
        if (RenderState != null)
        {
            c.PushModelMatrix(GetModelMatrix());
            c.RenderEntityStandalone(Entity, RenderState, Vector3.Zero);
            c.PopModelMatrix();
        }

        base.Render(c);
    }
}
