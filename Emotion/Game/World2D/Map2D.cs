#region Using

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Game.Text;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public partial class Map2D
    {
        /// <summary>
        /// The name of the map. Not to be confused with the asset name carried by the XMLAsset.
        /// </summary>
        public string MapName { get; set; }

        /// <summary>
        /// The file the map was loaded from, if any. Should equal the XMLAsset name.
        /// </summary>
        public string? FileName { get; set; }

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
        [DontSerialize]
        public bool Initialized { get; protected set; }

        /// <summary>
        /// Whether map loading has started.
        /// </summary>
        protected bool _mapLoadedStarted { get; set; }

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
        [DontSerialize] public bool EditorMode;

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

            _worldTree = new WorldTree2D(MapSize);
            _mapLoadedStarted = true;

            SetupWorldTreeLayers(_worldTree);

            // It is possible for objects to have been added before the world tree is initialized.
            // We need to add them to the world tree now.
            for (var i = 0; i < _objects.Count; i++)
            {
                GameObject2D obj = _objects[i];
                _worldTree.AddObjectToTree(obj);
            }

            // Spawn all serialized objects
            for (var i = 0; i < ObjectsToSerialize.Count; i++)
            {
                GameObject2D obj = ObjectsToSerialize[i];
                if (!obj.ShouldSpawnSerializedObject(this)) continue;

                AddObject(obj);
                obj.MapFlags |= Map2DObjectFlags.Serializable; // Flags aren't saved, so we need to apply it.
            }

            // Load tile data. During this time object loading is running async.
            if (TileData != null)
            {
                Debug.Assert((TileData.SizeInTiles * TileData.TileSize).SmallerOrEqual(MapSize), "Tiles outside map.");
                await TileData.LoadTileDataAsync();
            }

            // Wait for the assets of all objects to load.
            await AwaitAllObjectsLoaded();

            // Run map post processing.
            await PostMapLoad();

            // It is possible for PostMapLoad to have added more objects. Wait for them to load too.
            await AwaitAllObjectsLoaded();

            // Wait for the GLThread work queue to empty up.
            // This ensures that all assets (texture uploads etc) and stuff are loaded.
            while (!GLThread.Empty) await Task.Delay(1);
            Update(0); // Run the update tick once to prevent some flickering on first update.
            Initialized = true;

            SetupDebug();

            Engine.Log.Info($"Map {MapName} loaded in {profiler.ElapsedMilliseconds}ms", "Map2D");
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
        /// Reset the map as if it was reloaded.
        /// </summary>
        public void Reset()
        {

        }

        #endregion

        #region Object Management

        private ConcurrentQueue<GameObject2D> _objectsToRemove = new();
        private ConcurrentQueue<GameObject2D> _objectsToUpdate = new();
        private List<(GameObject2D, Task)> _objectLoading = new();

        /// <summary>
        /// Add an object to the map. The object should be uninitialized.
        /// </summary>
        public void AddObject(GameObject2D obj)
        {
            Debug.Assert(obj.ObjectState == ObjectState.None);

            // Serializable objects added before map load will be loaded in bulk.
            if (obj.MapFlags.HasFlag(Map2DObjectFlags.Serializable) && !_mapLoadedStarted)
            {
                ObjectsToSerialize.Add(obj);
                return;
            }

            // Attaches the object to the map
            obj.AttachToMap(this);
            _objects.Add(obj);
            _worldTree?.AddObjectToTree(obj);

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

        protected void ObjectPostLoad(GameObject2D obj)
        {
            obj.Init();
            obj.ObjectState = ObjectState.Alive;
        }

        protected async Task AwaitAllObjectsLoaded()
        {
            for (int i = _objectLoading.Count - 1; i >= 0; i--)
            {
                (GameObject2D? obj, Task? loadingTask) = _objectLoading[i];
                if (!loadingTask.IsCompleted)
                {
                    await loadingTask;
                    Debug.Assert(loadingTask.IsCompleted);
                }

                _objectLoading.RemoveAt(i);
                ObjectPostLoad(obj);
            }
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
                ObjectPostLoad(obj);
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

        public void GetObjects(IList list, int layer, IShape shape, QueryFlags queryFlags = 0)
        {
            WorldTree2DRootNode? rootNode = _worldTree?.GetRootNodeForLayer(layer);
            rootNode?.AddObjectsIntersectingShape(list, shape, queryFlags);
        }

        #endregion

        public virtual void Update(float dt)
        {
            if (!Initialized) return;

            ProcessObjectChanges();

            // todo: obj update, clipped?
            int objCount = GetObjectCount();
            for (var i = 0; i < objCount; i++)
            {
                GameObject2D obj = GetObjectByIndex(i);
                obj.Update(dt);
            }

            UpdateDebug();
        }

        public virtual void Render(RenderComposer c)
        {
            if (!Initialized) return;

            Rectangle clipArea = Clip ?? c.Camera.GetWorldBoundingRect();
            TileData?.RenderTileMap(c, clipArea);

            var renderObjectsList = new List<GameObject2D>();
            GetObjects(renderObjectsList, 0, clipArea);
            for (var i = 0; i < renderObjectsList.Count; i++)
            {
                GameObject2D obj = renderObjectsList[i];
                obj.Render(c);
            }

            RenderDebug(c);
        }

        public void RenderDebugWorldTree(RenderComposer c)
        {
            c.RenderOutline(new Rectangle(0, 0, MapSize), Color.Red);
        }

        public virtual void Dispose()
        {
            Engine.Host.OnKey.RemoveListener(DebugInputHandler);
        }
    }
}