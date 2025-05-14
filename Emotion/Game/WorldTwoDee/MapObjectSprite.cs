using Emotion.Common.Serialization;
using Emotion.Game.TwoDee;
using Emotion.IO;
using Emotion.WIPUpdates.One.Work;

#nullable enable

namespace Emotion.Game.WorldTwoDee;

public class MapObjectSprite : MapObject
{
    [DontSerialize]
    public SpriteEntity SpriteEntity
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

        //_currentAnimation = null;
        //_animationTime = 0;

        _entity = entity;

        if (_entity != null)
        {
            RenderState = new SpriteEntityMetaState(_entity);

            // Reset the animation (or set it to the one set before the entity was loaded).
            // This will also set the default bone matrices.
            // This will also calculate bounds
            //SetAnimation(_initSetAnimation);
            //_initSetAnimation = null;

            //RenderState.UpdateAnimationRigBones(_currentAnimation, 0);
        }
        else
        {
            //_boundingSphereBase = new Sphere();
            //_boundingCubeBase = new Cube();
        }

        InvalidateModelMatrix();
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
        if (name != null && _entity.Animations != null)
        {
            for (var i = 0; i < _entity.Animations.Count; i++)
            {
                SpriteAnimation anim = _entity.Animations[i];
                if (anim.Name == name) animInstance = anim;
            }
        }
        if (animInstance == null && name != null && !forceIfMissing)
            return;

        _currentAnimation = animInstance;
        _animationTime = 0; // Reset time

        //CacheVerticesForCollision();

        //_entity.GetBounds(null, out Sphere baseSphere, out Cube baseCube);
        //_boundingSphereBase = baseSphere;
        //_boundingCubeBase = baseCube;

        //Assert(_boundingSphereBase.Radius != 0, "Entity bounds is 0 - no vertices?");
    }

    #endregion

    public override void Update(float dt)
    {
        // Update current animation
        if (_currentAnimation != null && _entity != null && RenderState != null)
        {
            _animationTime += dt;

            //float duration = _currentAnimation.Duration;
            //RenderState.UpdateAnimationRigBones(_currentAnimation, _animationTime % duration);
            //if (_animationTime > duration) _animationTime -= duration;
        }

        base.Update(dt);
    }

    public override void Render(RenderComposer c)
    {
        if (_entity != null && RenderState != null)
        {
            //c.MeshEntityRenderer.EnsureAssetsLoaded();

            //c.MeshEntityRenderer.StartScene(c);
            //c.MeshEntityRenderer.SubmitObjectForRendering(this, _entity, RenderState);
            //c.MeshEntityRenderer.EndScene(c, LightModel.DefaultLightModel);
        }

        base.Render(c);
    }
}
