#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Game.QuadTree;
using Emotion.Game.World.Prefab;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World;

public abstract class BaseGameObject : IQuadTreeObject
{
    #region Transform API

    public abstract Vector3 Position { get; set; }

    public abstract Vector2 Position2 { get; set; }

    public abstract Rectangle Bounds2D { get; set; }

    #endregion

    /// <summary>
    /// The unique id of the object. Is assigned when added to the map.
    /// </summary>
    [DontSerialize] public int UniqueId;

    /// <summary>
    /// The object's name. Should be unique map-wide, but
    /// isn't actually enforced.
    /// </summary>
    public string? ObjectName { get; set; }

    /// <summary>
    /// If the object is spawned from a prefab this is the a handle to that prefab.
    /// </summary>
    public GameObjectPrefabOriginData? PrefabOrigin { get; set; }

    /// <summary>
    /// The object's multiplicative color tint.
    /// </summary>
    public virtual Color Tint { get; set; } = Color.White;

    /// <summary>
    /// Flags that specify systemic treatment of the object.
    /// </summary>
    // [DontSerializeFlagValue((uint) ObjectFlags.Persistent)]
    public ObjectFlags ObjectFlags { get; set; }

    #region Runtime

    /// <summary>
    /// The map this object is in. Is set after Init.
    /// </summary>
    [DontSerialize]
    public BaseMap Map { get; protected set; }

    /// <summary>
    /// The object state, managed by the map in runtime.
    /// </summary>
    [DontSerialize]
    public ObjectState ObjectState { get; set; }

    /// <summary>
    /// Object flags managed by the map in runtime.
    /// </summary>
    [DontSerialize]
    public MapFlags MapFlags { get; set; }

    #endregion

    protected BaseGameObject(string name)
    {
        ObjectName = name;
        Map = null!;
    }

    // Serialization constructor.
    protected BaseGameObject()
    {
        ObjectName = null!;
        Map = null!;
    }

    /// <summary>
    /// Whether this serialized object should be spawned when the map is loaded.
    /// This should always return true in EditorMode
    /// </summary>
    public virtual bool ShouldSpawnSerializedObject(BaseMap map)
    {
        return true;
    }

    /// <summary>
    /// Load all assets in use by the object. When the object is added to the map this is called after AttachToMap.
    /// If the AddObject call is specified to be async then this could run
    /// parallel with other objects(which is why it is an async func)
    /// </summary>
    public virtual Task LoadAssetsAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// When the map does AddObject this is called first and populates the Map property.
    /// After this call the object will be returned by various map querries.
    /// Note that if the object's size or position is calculated based on assets or during init
    /// then it wont be where you expect it to be spatially.
    /// </summary>
    public virtual void AttachToMap(BaseMap map)
    {
        Map = map;
    }

    /// <summary>
    /// Init the game, this is called after LoadAssetsAsync has finished.
    /// All changes from this process shouldn't be serialized.
    /// </summary>
    public virtual void Init()
    {
    }

    /// <summary>
    /// Free any resources and cleanup.
    /// </summary>
    public virtual void Destroy()
    {
        Map = null!;
    }

    protected virtual void Moved()
    {
        OnMove?.Invoke(this, EventArgs.Empty);

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Map?.InvalidateObjectBounds(this);
    }

    protected virtual void Resized()
    {
        OnResize?.Invoke(this, EventArgs.Empty);

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Map?.InvalidateObjectBounds(this);
    }

    // todo: deprecate this and start making layers based on object type with some kind of
    // automatic detection based on what querries are ran.
    /// <summary>
    /// Whether the object is part of the specified layer in the map's world tree.
    /// Objects are always part of layer 0 - ALL.
    /// The layers must have been added using _worldTree.AddLayer prior
    /// to the object being added to the map, as this function is checked only then.
    /// The best place to do this is overriding the Map's SetupWorldTreeLayers function and adding them there.
    /// </summary>
    public virtual bool IsPartOfMapLayer(int layer)
    {
        return layer == 0;
    }

    // todo: is this wrapper (and the render one) necessary?
    public void Update(float dt)
    {
        UpdateInternal(dt);
    }

    public void Render(RenderComposer c)
    {
        RenderInternal(c);
    }

    /// <summary>
    /// Is run every tick. By default the map will update all of its objects,
    /// but by overriding the map's update function you can optimize or customize this.
    /// </summary>
    protected virtual void UpdateInternal(float dt)
    {
    }

    /// <summary>
    /// Is run every frame. By default the map will render all objects in layer 0 which are returned by
    /// a camera world bounds query.
    /// </summary>
    protected virtual void RenderInternal(RenderComposer c)
    {
    }

    public override string ToString()
    {
        return $"Object [{UniqueId}] {ObjectName ?? $"{GetType().Name}"}";
    }

    #region IQuadTreeObject

    /// <summary>
    /// Is invoked when the object moves.
    /// </summary>
    public event EventHandler? OnMove;

    /// <summary>
    /// Is invoked when the object's scale changes.
    /// </summary>
    public event EventHandler? OnResize;

    public Rectangle GetBoundsForQuadTree()
    {
        if (Map != null && Map.EditorMode)
        {
            Rectangle bounds = Bounds2D;
            bounds.Size = Vector2.Max(bounds.Size, new Vector2(10));
            return bounds;
        }

        return Bounds2D;
    }

    #endregion
}