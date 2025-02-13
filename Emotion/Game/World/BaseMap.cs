#nullable enable

#region Using

using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Editor;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Time.Routines;
using Emotion.Game.World.Grid;
using Emotion.Game.World2D;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World;

public abstract partial class BaseMap
{
    /// <summary>
    /// The size of the map in map units.
    /// </summary>
    public Vector2 MapSize { get; set; }

    /// <summary>
    /// The name of the map. Not to be confused with the asset name carried by the XMLAsset.
    /// </summary>
    public string MapName { get; set; }

    /// <summary>
    /// The file the map was loaded from, if any. Should equal the XMLAsset name.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Whether the map is loaded and ready to be used.
    /// </summary>
    [DontSerialize]
    public bool Initialized { get; protected set; }

    /// <summary>
    /// Whether the map has been disposed. It can be reset to be reused.
    /// </summary>
    [DontSerialize]
    public bool Disposed { get; protected set; }

    /// <summary>
    /// Whether map loading has started.
    /// </summary>
    protected bool _mapInitStarted { get; set; }

    /// <summary>
    /// Whether currently loading serialized objects. Used to optimize a check in AddObject away.
    /// </summary>
    protected bool _loadingPersistentObjects;

    /// <summary>
    /// List of objects that are part of the map file. Used on map initialization and by the editor.
    /// Do not modify in gameplay - that's weird!
    /// </summary>
    [DontShowInEditor]
    public List<BaseGameObject> PersistentObjects { get; set; }

    /// <summary>
    /// List of all grids that pertain to the map.
    /// </summary>
    public List<IMapGrid> Grids { get; set; }

    #region Events

    public event Action? OnMapReset;

    #endregion

    #region Runtime State

    // This list contains all runtime objects.
    protected List<BaseGameObject> _objects { get; set; }

    /// <summary>
    /// Quad tree used to speed up queries.
    /// </summary>
    protected WorldTree2D? _worldTree;

    /// <summary>
    /// The next sequential unique object id.
    /// </summary>
    protected int _nextObjectUid = 1;

    /// <summary>
    /// Coroutines running on the map.
    /// </summary>
    [DontSerialize]
    public CoroutineManager CoroutineManager { get; private set; } = new();

    #endregion

    #region Events

    public event Action<BaseGameObject>? OnObjectRemoved;
    public event Action<BaseGameObject>? OnObjectAdded;

    #endregion

    protected BaseMap(Vector2 size, string mapName = "Unnamed Map")
    {
        MapSize = size;
        MapName = mapName;
        PersistentObjects = new List<BaseGameObject>();
        _objects = new List<BaseGameObject>();
        Grids = new List<IMapGrid>();
    }

    // Serialization constructor
    protected BaseMap()
    {
        MapName = null!;
        _objects = new();
        PersistentObjects = new();
        Grids = new();
    }

    #region Editor Support

    /// <summary>
    /// Whether the map is currently open in the editor.
    /// Mostly used to conditionally render/represent things.
    /// </summary>
    [DontSerialize] public bool EditorMode;

    // We need to do some manual init as the serialization constructor will expect these to be present.
    public virtual void EditorCreateInitialize()
    {
        MapSize = new Vector2(1, 1);
        PersistentObjects = new List<BaseGameObject>();
        InitAsync().Wait();
    }

    /// <summary>
    /// Reinitializes an object that is already part of the map.
    /// This is only used by the editor to reload objects.
    /// </summary>
    public void Editor_ReinitializeObject(BaseGameObject obj)
    {
        Assert(EditorMode);
        Assert(!_loadingPersistentObjects);

        _objects.Remove(obj);
        _worldTree?.RemoveObjectFromTree(obj);
        _loadingPersistentObjects = true;
        AddObject(obj);
        _loadingPersistentObjects = false;
    }

    public virtual List<Type> GetValidObjectTypes()
    {
        return EditorUtility.GetTypesWhichInherit<BaseGameObject>();
    }

    #endregion

    /// <summary>
    /// Initialize the map. This will create the map internal structures, load assets, and fill caches.
    /// </summary>
    public async Task InitAsync()
    {
        if (Disposed)
        {
            Engine.Log.Warning("Initializing a disposed map. Resetting instead, verify if you intended this and reset it yourself otherwise.", MessageSource.Game);
            Disposed = false;
            await Reset();
            return;
        }

        if (Initialized)
        {
            Engine.Log.Warning("Initializing an already initialized map. Resetting instead, verify if you intended this and reset it yourself otherwise.", MessageSource.Game);
            Initialized = false;
            await Reset();
            return;
        }

        var profiler = Stopwatch.StartNew();

        CoroutineManager.StopAll(); // Clear
        _mapInitStarted = true;

        // Load grids
        {
            Task[] gridLoading = new Task[Grids.Count];
            for (int i = 0; i < Grids.Count; i++)
            {
                var grid = Grids[i];
                gridLoading[i] = grid.LoadAsync(this);
            }
            await Task.WhenAll(gridLoading);
            for (int i = 0; i < Grids.Count; i++)
            {
                var grid = Grids[i];
                //grid.ResizeToMapSize(MapSize);
            }
        }

        _worldTree = new WorldTree2D(MapSize);
        SetupWorldTreeLayers(_worldTree);

        // It is possible for non-persisted objects to have been added before the world tree is initialized.
        // We need to add them to the world tree now.
        for (var i = 0; i < _objects.Count; i++)
        {
            BaseGameObject obj = _objects[i];
            _worldTree.AddObjectToTree(obj);
        }

        // Spawn all persisted objects
        _loadingPersistentObjects = true;
        for (var i = 0; i < PersistentObjects.Count; i++)
        {
            BaseGameObject obj = PersistentObjects[i];

            // Object type not found by deserializer.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (obj == null) continue;

            obj.ObjectState = ObjectState.None;
            AddObject(obj, true);

            // We gotta remark the objects as this flag in particular is not serialized
            // (removed by serialization since all persistent objects will have it).
            // The reason being that all objects will have it, so no point in saving it.
            obj.ObjectFlags |= ObjectFlags.Persistent;
        }

        _loadingPersistentObjects = false;

        // Run inherited type loading.
        // During this time object loading is going async.
        await InitAsyncInternal();

        // Wait for the assets of all objects to load.
        await AwaitAllObjectsLoaded();

        // Run map post processing.
        await PostMapLoad();

        // It is possible for PostMapLoad to have added more objects. Wait for them to load too.
        await AwaitAllObjectsLoaded();

        // Wait for the GLThread work queue to empty up.
        // This ensures that all assets (texture uploads etc) and stuff are loaded.
        while (!GLThread.Empty) await Task.Delay(1);

        // Run the update tick once to prevent some flickering on first update as some objects might
        // initialize something (such as animations) on first update.
        Update(0);

        // Force garbage collection now as the init prob generated a lot of garbage.
        GC.Collect();
        Initialized = true;

        Engine.Log.Info($"Map {MapName} loaded in {profiler.ElapsedMilliseconds}ms", "World");
    }

    protected virtual Task InitAsyncInternal()
    {
        return Task.CompletedTask;
    }

    protected virtual void SetupWorldTreeLayers(WorldTree2D worldTree)
    {
        // Define all world tree layers in the inheriting map here.
        // Example:
        // worldTree.AddTreeLayer(MY_LAYER_ID);
        // where
        // public const int MY_LAYER_ID = 1;
    }

    protected virtual Task PostMapLoad()
    {
        return Task.CompletedTask;
    }

    protected virtual void PostMapFullyLoaded()
    {
    }

    /// <summary>
    /// Reset the map to its "loaded from file" state.
    /// </summary>
    public virtual async Task Reset()
    {
        GLThread.ExecuteGLThread(Dispose);
        Disposed = false;

        // Ensure reset is identical to deserialize by serialize-copying objects.
        // This will clear any initialized values etc.
        string objectsXML = XMLFormat.To(PersistentObjects) ?? "";
        PersistentObjects = XMLFormat.From<List<BaseGameObject>>(objectsXML) ?? new List<BaseGameObject>();

        await InitAsync();

        OnMapReset?.Invoke();
    }


    #region Internal API

    public virtual void Update(float dt)
    {
        if (!Initialized) return;

        if (!EditorMode)
            CoroutineManager.Update(dt);

        ProcessObjectChanges();

        // todo: should we update objects at all in editor mode?
        // todo: should we update all objects or just clip visible ones?
        dt = EditorMode ? 0 : dt;
        foreach (var obj in ObjectsEnum())
            obj.Update(dt);
    }

    public virtual void Render(RenderComposer c)
    {
        for (int i = 0; i < Grids.Count; i++)
        {
            var grid = Grids[i];
            grid.Render(c);
        }
    }

    #endregion

    #region Object Management

    protected ConcurrentQueue<BaseGameObject> _objectsToRemove = new();
    protected ConcurrentQueue<BaseGameObject> _objectsToUpdate = new();
    protected List<(BaseGameObject, Task)> _objectLoading = new();

    /// <summary>
    /// Add an object to the map. The object should be uninitialized.
    /// </summary>
    public void AddObject(BaseGameObject obj, bool async = false)
    {
        Assert(obj.ObjectState == ObjectState.None);

        if (obj.UniqueId == 0)
        {
            obj.UniqueId = _nextObjectUid;
            _nextObjectUid++;
        }

#if DEBUG

        for (var i = 0; i < _objects.Count; i++)
        {
            BaseGameObject otherObj = _objects[i];
            if (_objectsToRemove.Contains(otherObj)) continue; // During reset?
            Assert(otherObj.UniqueId != obj.UniqueId);
        }

#endif
        if (obj.ObjectFlags.HasFlag(ObjectFlags.Persistent) && !_loadingPersistentObjects)
        {
            Assert(!PersistentObjects.Contains(obj));
            PersistentObjects.Add(obj);

            // Serializable objects added before map load will be loaded in bulk and join the map file.
            // This usually occurs when a map is being generated by code.
            if (!_mapInitStarted) return;
        }

        // Check if not spawning this object.
        if (!obj.ShouldSpawnSerializedObject(this))
        {
            _objects.Add(obj);
            obj.ObjectState = ObjectState.ConditionallyNonSpawned;
            obj.AttachToMap(this);
            _worldTree?.AddObjectToTree(obj);
            OnObjectAdded?.Invoke(obj);
            return;
        }

        // Attaches the object to the map
        obj.AttachToMap(this);
        _objects.Add(obj);
        _worldTree?.AddObjectToTree(obj);

        if (async)
        {
            // Start loading object assets. While loading the object is considered part of the world.
            // If the object is added during map load its loading will be awaited.
            // Otherwise the object will be drawn and updated before being considered loaded.
            // This can easily be checked using the ObjectState, and Init wouldn't have been called.
            Task loading = Task.Run(obj.LoadAssetsAsync);
            if (!loading.IsCompleted)
            {
                obj.ObjectState = ObjectState.Loading;
                _objectLoading.Add((obj, loading));
            }
            else // Consider loaded instantly if no asset loading
            {
                ObjectPostLoad(obj);
            }
        }
        else
        {
            obj.ObjectState = ObjectState.Loading;
            Task loadingTask = obj.LoadAssetsAsync();
            loadingTask.Wait();
            ObjectPostLoad(obj);
        }
    }

    protected void ObjectPostLoad(BaseGameObject obj)
    {
        obj.Init();
        obj.ObjectState = ObjectState.Alive;
        OnObjectAdded?.Invoke(obj);
    }

    protected async Task AwaitAllObjectsLoaded()
    {
        for (int i = _objectLoading.Count - 1; i >= 0; i--)
        {
            (BaseGameObject? obj, Task? loadingTask) = _objectLoading[i];
            if (!loadingTask.IsCompleted)
            {
                await loadingTask;
                Assert(loadingTask.IsCompleted);
            }

            _objectLoading.RemoveAt(i);
            ObjectPostLoad(obj);
        }
    }

    /// <summary>
    /// Remove the object from the map. If the object is serialized with the map it will only
    /// be removed from the loaded map, but not from the map file, and will be restored on map reload.
    /// If the "removeFromMapFile" is specified then it will be removed from the map file all together.
    /// Take note that for this to take permanent persistent effect the map file needs to be saved to the disk.
    /// </summary>
    public void RemoveObject(BaseGameObject obj, bool removeFromMapFile = false)
    {
        _objectsToRemove.Enqueue(obj);
        if (removeFromMapFile && PersistentObjects.Contains(obj)) PersistentObjects.Remove(obj);
    }

    /// <summary>
    /// Tell the world tree that an object has moved or resized.
    /// This will cause it to find a new node on the next update tick.
    /// Take note that until the next update tick it will be returned in
    /// queries that target the bounds of its old node. This only
    /// affects objects that have moved/resized a great lot.
    /// </summary>
    public void InvalidateObjectBounds(BaseGameObject obj)
    {
        if (obj.ObjectState == ObjectState.ConditionallyNonSpawned) return;
        if (obj.MapFlags.EnumHasFlag(MapFlags.UpdateWorldTree)) return;
        _objectsToUpdate.Enqueue(obj);
        obj.MapFlags |= MapFlags.UpdateWorldTree;
    }

    protected virtual void ProcessObjectChanges()
    {
        AssertNotNull(_worldTree);

        // Check if objects we're waiting to load, have loaded.
        for (int i = _objectLoading.Count - 1; i >= 0; i--)
        {
            (BaseGameObject? obj, Task? loadingTask) = _objectLoading[i];
            if (!loadingTask.IsCompleted) continue;

            _objectLoading.RemoveAt(i);
            ObjectPostLoad(obj);
        }

        // Check for objects to remove.
        while (_objectsToRemove.TryDequeue(out BaseGameObject? obj))
        {
            _objects.Remove(obj);
            obj.Destroy();
            _worldTree.RemoveObjectFromTree(obj);
            obj.ObjectState = ObjectState.Destroyed;
            OnObjectRemoved?.Invoke(obj);
        }

        // Check for objects that have moved within the world tree.
        while (_objectsToUpdate.TryDequeue(out BaseGameObject? obj))
        {
            if (obj.ObjectState == ObjectState.Destroyed) continue;
            _worldTree.UpdateObjectInTree(obj);
            obj.MapFlags &= ~MapFlags.UpdateWorldTree;
        }
    }

    #endregion

    #region Iteration and Query

    /// <summary>
    /// Get an object from the map by name.
    /// </summary>
    public BaseGameObject? GetObjectByName(string? name, bool includeNonSpawned = false)
    {
        if (name == null) return null;

        // todo: cache per name?
        foreach (var obj in ObjectsEnum(includeNonSpawned ? null : ObjectState.Alive))
        {
            if (obj.ObjectName == name)
                return obj;
        }

        return null;
    }

    public ObjTypeFilter? GetFirstObjectOfType<ObjTypeFilter>(bool includeNonSpawned = false)
        where ObjTypeFilter : BaseGameObject
    {
        foreach (var obj in ObjectsEnum<ObjTypeFilter>(includeNonSpawned ? null : ObjectState.Alive))
        {
            return obj;
        }

        return null;
    }

    public WorldTree2D? GetWorldTree()
    {
        return _worldTree;
    }

    public int GetObjectCount()
    {
        return _objects.Count;
    }

    public BaseGameObject GetObjectByIndex(int idx)
    {
        return _objects[idx];
    }

    #endregion

    public virtual void Dispose()
    {
        // Stop updates and rendering
        Initialized = false;
        _mapInitStarted = false;

        // Clear queues
        _objectLoading.Clear();
        ProcessObjectChanges();

        _worldTree = null;
        CoroutineManager.StopAll();
        _nextObjectUid = 1;

        // Clear existing objects.
        for (var i = 0; i < _objects.Count; i++)
        {
            BaseGameObject obj = _objects[i];
            obj.Destroy();
            obj.ObjectState = ObjectState.Destroyed;
        }

        _objects.Clear();

        Disposed = true;
    }
}