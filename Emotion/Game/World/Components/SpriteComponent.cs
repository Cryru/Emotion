#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.TwoDee;

namespace Emotion.Game.World.Components;

public class SpriteComponent : IRenderableComponent, IGameObjectComponent, IGameObjectTransformProvider, IUpdateableComponent
{
    public GameObject Object { get; private set; } = null!;
    private bool _componentInitialized = false;

    public SpriteEntity Entity { get => _entity; }
    private AssetOwner<SpriteEntityAsset, SpriteEntity> _entityOwner = new();

    private SpriteEntity _entity = null!;

    [DontSerialize]
    public SpriteEntityMetaState RenderState { get; private set; } = null!;

    public SpriteComponent(SpriteReference entity)
    {
        _entityOwner.SetOnChangeCallback(static (_, component) => (component as SpriteComponent)?.OnEntityChanged(), this);
        _entityOwner.Set(entity, true);
    }

    public SpriteComponent()
    {

    }

    public virtual IRoutineWaiter? Init(GameObject obj)
    {
        Object = obj;
        return _entityOwner.GetCurrentLoading();
    }

    public virtual void Done(GameObject obj)
    {
        _entityOwner.Done();
    }

    #region Entity Setting

    public Coroutine? SetEntity(SpriteReference entity)
    {
        return _entityOwner.Set(entity);
    }

    protected virtual void OnEntityChanged()
    {
        SpriteEntity? entity = _entityOwner.GetCurrentObject();
        _entity = entity ?? SpriteEntity.MissingEntity;
        _componentInitialized = true;

        if (Object.Name == GameObject.DEFAULT_OBJECT_NAME)
            Object.Name = _entity.Name;

        RenderState = new SpriteEntityMetaState(_entity);

        // If the same animation name exists in the new entity this will take care of it.
        // This will also update bounds.
        SetAnimation(_currentAnimationName);
    }

    #endregion

    #region Animation and Bones

    private string _currentAnimationName = string.Empty;
    private float _animationTime;

    public string GetCurrentAnimation()
    {
        return _currentAnimationName;
    }

    public bool HasAnimation(string name)
    {
        if (_entity.Animations != null)
        {
            foreach (SpriteAnimation animation in _entity.Animations)
            {
                if (animation.Name == name)
                    return true;
            }
        }
        return false;
    }

    public virtual void SetAnimation(string? name, bool forceIfMissing = false)
    {
        name ??= string.Empty;
        _currentAnimationName = name;

        if (!_componentInitialized)
            return;

        _animationTime = 0;
        RenderState.SetAnimation(_currentAnimationName);

        _boundingRectBase = _entity.GetBounds(_currentAnimationName);
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

    public void Update(float dt)
    {
        // Update current animation
        if (_entity != null && RenderState != null)
        {
            _animationTime += dt;
            _animationTime = RenderState.UpdateAnimation(_animationTime);
        }
    }

    public virtual void Render(Renderer r)
    {
        r.RenderEntityStandalone(Entity, RenderState, Object.GetModelMatrix());
    }
}
