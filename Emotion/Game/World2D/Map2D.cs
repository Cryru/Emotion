#region Using

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Standard.Logging;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public class Map2D
    {
        /// <summary>
        /// The name of the map. Not to be confused with the asset name carried by the XMLAsset.
        /// </summary>
        public string MapName { get; set; }

        /// <summary>
        /// The size of the map in map units.
        /// </summary>
        [SerializeNonPublicGetSet]
        public Vector2 MapSize { get; protected set; }

        /// <summary>
        /// Contains tile information, if the map has a tile map portion.
        /// </summary>
        public Map2DTileMapData? TileData;

        /// <summary>
        /// Whether the map is loaded and ready to be used.
        /// </summary>
        public bool Initialized { get; protected set; }

        /// <summary>
        /// This list is only used for saving and initializing the map, and is managed by the map itself.
        /// Do not modify!
        /// </summary>
        [SerializeNonPublicGetSet]
        public List<GameObject2D> ObjectsToSerialize { get; protected set; }

        #region Runtime State

        /// <summary>
        /// Whether the map is currently open in the editor.
        /// </summary>
        [DontSerialize] public bool EditorMode = false;

        /// <summary>
        /// The area of the map to render. By default this is initialized to the
        /// area visible by the 2D camera.
        /// </summary>
        [DontSerialize] public Rectangle? Clip = null;

        // This list contains all runtime objects.
        protected List<GameObject2D> _objects { get; set; }

        /// <summary>
        /// Quad tree used to speed up queries.
        /// </summary>
        protected WorldTree2D? _worldTree;

        /// <summary>
        /// Positions of all named objects at map load.
        /// </summary>
        [DontSerialize]
        public Dictionary<string, Vector2> InitialPositions { get; } = new();

        #endregion

        public Map2D(Vector2 size)
        {
            MapName = "New Map";
            MapSize = size;

            _objects = new List<GameObject2D>();
            ObjectsToSerialize = new List<GameObject2D>();
        }

        // Serialization constructor
        protected Map2D()
        {
            MapName = "";
            _objects = null!;
            ObjectsToSerialize = null!;
        }

        #region Init

        /// <summary>
        /// Initialize the map. This will create the map internal structures, load assets, and fill caches.
        /// </summary>
        public async Task InitAsync()
        {
            var profiler = Stopwatch.StartNew();

            // Add all serialized objects, and start loading their assets.
            var objectAssetTasks = new Task[ObjectsToSerialize.Count + _objectsToAdd.Count];
            for (var i = 0; i < ObjectsToSerialize.Count; i++)
            {
                GameObject2D obj = ObjectsToSerialize[i];
                if (!obj.ShouldSpawnSerializedObject(this))
                {
                    objectAssetTasks[i] = Task.CompletedTask;
                    continue;
                }

                objectAssetTasks[i] = obj.LoadAssetsAsync();
                obj.MapFlags |= Map2DObjectFlags.Serializable;
                _objects.Add(obj);
            }

            // Add non serialized object that were added before map init.
            // We want to load those objects now rather than wait for the first tick.
            int taskIdx = ObjectsToSerialize.Count;
            while (_objectsToAdd.TryDequeue(out GameObject2D? obj))
            {
                objectAssetTasks[taskIdx] = obj.LoadAssetsAsync();
                taskIdx++;
                _objects.Add(obj);
            }

            // Load tile data in parallel.
            if (TileData != null)
            {
                Debug.Assert((TileData.SizeInTiles * TileData.TileSize).SmallerOrEqual(MapSize), "Tiles outside map.");
                await TileData.LoadTileDataAsync();
            }

            await Task.WhenAll(objectAssetTasks);

            // Now that all assets are loaded, init the objects.
            foreach (GameObject2D obj in GetObjects())
            {
                obj.Init(this);
            }

            // Wait for the GLThread work queue to empty up.
            // This ensures that all assets (texture uploads etc) and stuff are loaded.
            while (!GLThread.Empty) await Task.Delay(1);

            // Call late init and add to the world tree.
            _worldTree = InitWorldTree();
            foreach (GameObject2D obj in GetObjects())
            {
                obj.LateInit();

                _worldTree.AddObjectToTree(obj);
                obj.ObjectState = ObjectState.Alive;

                // Record initial positions of named objects as named points.
                if (!string.IsNullOrEmpty(obj.ObjectName))
                {
                    if (!InitialPositions.ContainsKey(obj.ObjectName))
                        InitialPositions.Add(obj.ObjectName, obj.Position2);
                    else
                        Engine.Log.Warning($"Duplicate object name - {obj.ObjectName}", MessageSource.Game, true);
                }
            }

            await PostMapLoad();
            Update(0); // Run the update tick once to prevent some flickering on first update.
            Initialized = true;

            Engine.Log.Info($"Map {MapName} loaded in {profiler.ElapsedMilliseconds}ms", "Map2D");
        }

        protected virtual WorldTree2D InitWorldTree()
        {
            return new WorldTree2D(MapSize);
        }

        protected virtual Task PostMapLoad()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Object Management

        private ConcurrentQueue<GameObject2D> _objectsToAdd = new();
        private ConcurrentQueue<GameObject2D> _objectsToRemove = new();
        private ConcurrentQueue<GameObject2D> _objectsToUpdate = new();
        private List<(GameObject2D, Task)> _objectLoading = new();

        /// <summary>
        /// Add an object to the map. The object should be uninitialized.
        /// </summary>
        public void AddObject(GameObject2D obj)
        {
            Debug.Assert(obj.ObjectState == ObjectState.None);

            // Serializable objects added before the map is initialized will be initialized in
            // map load and be part of the map loading.
            if (obj.MapFlags.HasFlag(Map2DObjectFlags.Serializable) && !Initialized)
            {
                ObjectsToSerialize.Add(obj);
                return;
            }

            _objectsToAdd.Enqueue(obj);
        }

        /// <summary>
        /// Initialize an object now and add it.
        /// By default objects are initialized on update ticks, or during map load, but it is possible
        /// for the map to create critical objects during runtime, such as for player characters.
        /// </summary>
        protected async Task ObjectLoadAndAdd(GameObject2D obj)
        {
            Debug.Assert(_worldTree != null);

            await obj.LoadAssetsAsync();
            obj.Init(this);
            obj.LateInit();
            _objects.Add(obj);
            _worldTree.AddObjectToTree(obj);
            obj.ObjectState = ObjectState.Alive;
        }

        public void RemoveObject(GameObject2D obj)
        {
            _objectsToRemove.Enqueue(obj);
        }

        public void InvalidateObjectBounds(GameObject2D obj)
        {
            if (obj.MapFlags.HasFlag(Map2DObjectFlags.UpdateWorldTree)) return;
            _objectsToUpdate.Enqueue(obj);
            obj.MapFlags |= Map2DObjectFlags.UpdateWorldTree;
        }

        protected virtual void ProcessObjectChanges()
        {
            Debug.Assert(_worldTree != null);

            // Check if objects we're waiting to load, have loaded.
            for (int i = _objectLoading.Count - 1; i >= 0; i--)
            {
                (GameObject2D? obj, Task? loadingTask) = _objectLoading[i];
                if (!loadingTask.IsCompleted) continue;

                _objectLoading.RemoveAt(i);
                _objectsToAdd.Enqueue(obj);
            }

            // Check objects to add.
            while (_objectsToAdd.TryDequeue(out GameObject2D? obj))
            {
                // Check if the object requires loading, and put it in that state first.
                if (obj.ObjectState == ObjectState.None)
                {
                    Task loading = Task.Run(obj.LoadAssetsAsync);
                    if (!loading.IsCompleted)
                    {
                        obj.ObjectState = ObjectState.Loading;
                        _objectLoading.Add((obj, loading));
                        continue;
                    }
                }

                _objects.Add(obj);
                obj.Init(this);
                obj.LateInit();
                _worldTree.AddObjectToTree(obj);
                obj.ObjectState = ObjectState.Alive;

                // Objects added during runtime are added to the map's serialization if the flag is turned on, off by default.
                // This should technically only concern objects added by the editor.
                if (obj.MapFlags.HasFlag(Map2DObjectFlags.Serializable)) ObjectsToSerialize.Add(obj);
            }

            // Check for objects to remove.
            while (_objectsToRemove.TryDequeue(out GameObject2D? obj))
            {
                _objects.Remove(obj);
                obj.Destroy();
                _worldTree.RemoveObjectFromTree(obj);
                obj.ObjectState = ObjectState.Destroyed;
            }

            // Check for objects that have moved within the world tree.
            while (_objectsToUpdate.TryDequeue(out GameObject2D? obj))
            {
                if (obj.ObjectState == ObjectState.Destroyed) continue;
                _worldTree.UpdateObjectInTree(obj);
                obj.MapFlags &= ~Map2DObjectFlags.UpdateWorldTree;
            }
        }

        /// <summary>
        /// Get an object from the map by name.
        /// </summary>
        public GameObject2D? GetObjectByName(string? name)
        {
            if (name == null) return null;
            foreach (GameObject2D obj in GetObjects())
            {
                if (obj.ObjectName == name) return obj;
            }

            return null;
        }

        #endregion

        #region Iteration and Query

        public IEnumerable<GameObject2D> GetObjects()
        {
            for (var i = 0; i < _objects.Count; i++)
            {
                yield return _objects[i];
            }
        }

        public int GetObjectCount()
        {
            return _objects.Count;
        }

        public GameObject2D GetObjectByIndex(int idx)
        {
            return _objects[idx];
        }

        public void GetObjects(IList list, int layer, IShape shape)
        {
            WorldTree2DRootNode? rootNode = _worldTree?.GetRootNodeForLayer(layer);
            rootNode?.AddObjectsIntersectingShape(list, shape);
        }

        #endregion

        public virtual void Update(float dt)
        {
            if (!Initialized) return;

            ProcessObjectChanges();

            // todo: obj update, clipped?
            for (var i = 0; i < GetObjectCount(); i++)
            {
                GameObject2D obj = GetObjectByIndex(i);
                obj.Update(dt);
            }
        }

        public virtual void Render(RenderComposer c)
        {
            if (!Initialized) return;

            Rectangle clipArea = Clip ?? c.Camera.GetWorldBoundingRect();
            TileData?.RenderTileMap(c, clipArea);

            // todo: render clip
            foreach (GameObject2D obj in GetObjects())
            {
                obj.Render(c);
            }
        }

        public void RenderDebugWorldTree(RenderComposer c)
        {
            c.RenderOutline(new Rectangle(0, 0, MapSize), Color.Red);
        }
    }
}