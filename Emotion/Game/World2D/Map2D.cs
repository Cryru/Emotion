#region Using

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Standard.XML;

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
		public Vector2 MapSize { get; set; }

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
		/// Whether the map has been disposed. It can be reset to be reused.
		/// </summary>
		[DontSerialize]
		public bool Disposed { get; protected set; }

		/// <summary>
		/// Whether map loading has started.
		/// </summary>
		protected bool _mapLoadedStarted { get; set; }

		/// <summary>
		/// Whether currently loading serialized objects. Used to optimize a check in AddObject away.
		/// </summary>
		private bool _loadingPersistentObjects;

		/// <summary>
		/// List of objects that are part of the map file. Used on map initialization and by the editor.
		/// Do not modify in gameplay - that's weird!
		/// </summary>
		public List<GameObject2D> PersistentObjects { get; set; }

		#region Runtime State

		/// <summary>
		/// Whether the map is currently open in the editor.
		/// </summary>
		[DontSerialize] public bool EditorMode;

		// This list contains all runtime objects.
		protected List<GameObject2D> _objects { get; set; }

		/// <summary>
		/// Quad tree used to speed up queries.
		/// </summary>
		protected WorldTree2D? _worldTree;

		/// <summary>
		/// The next sequential unique object id.
		/// </summary>
		protected int _nextObjectUid = 1;

		#endregion

		public Map2D(Vector2 size, string mapName = "Unnamed Map")
		{
			MapName = mapName;
			MapSize = size;

			_objects = new List<GameObject2D>();
			PersistentObjects = new List<GameObject2D>();
		}

		// Serialization constructor
		protected Map2D()
		{
			MapName = null!;
			_objects = new List<GameObject2D>();
			PersistentObjects = null!;
		}

		#region Init

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

			_worldTree = new WorldTree2D(MapSize);
			_mapLoadedStarted = true;

			SetupWorldTreeLayers(_worldTree);

			// It is possible for non-persisted objects to have been added before the world tree is initialized.
			// We need to add them to the world tree now.
			for (var i = 0; i < _objects.Count; i++)
			{
				GameObject2D obj = _objects[i];
				_worldTree.AddObjectToTree(obj);
			}

			// Spawn all persisted objects
			_loadingPersistentObjects = true;
			for (var i = 0; i < PersistentObjects.Count; i++)
			{
				GameObject2D obj = PersistentObjects[i];
				obj.ObjectState = ObjectState.None;
				AddObject(obj);

				// We gotta remark the objects as this flag in particular is not serialized (removed in the Trim function).
				// The reason being that all objects will have it, so no point in saving it.
				obj.ObjectFlags |= ObjectFlags.Persistent;
			}

			_loadingPersistentObjects = false;

			// Bump id up to differentiate objects that were initialized from the serialize.
			if (_nextObjectUid < 10_000) _nextObjectUid = 10_000;

			// Load tile data. During this time object loading is running async.
			if (TileData != null) await TileData.LoadTileDataAsync();

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
		public virtual async Task Reset()
		{
			Initialized = false; // Stop updates and renders
			_mapLoadedStarted = false;

			// Clear queues
			_objectLoading.Clear();
			ProcessObjectChanges();

			_worldTree = null;
			_nextObjectUid = 1;

			// Clear existing objects.
			for (var i = 0; i < _objects.Count; i++)
			{
				GameObject2D obj = _objects[i];
				obj.Destroy();
				obj.ObjectState = ObjectState.Destroyed;
				obj.TrimPropertiesForSerialize();

				if (EditorMode) EditorObjectRemoved(obj);
			}

			_objects.Clear();

			// Ensure reset is identical to deserialize by serialize-copying objects.
			// This will clear any initialized values etc.
			string objectsXML = XMLFormat.To(PersistentObjects);
			PersistentObjects = XMLFormat.From<List<GameObject2D>>(objectsXML);

			Dispose();

			// todo: should we reset tile data?
			await InitAsync();
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

			if (obj.UniqueId == 0)
			{
				obj.UniqueId = _nextObjectUid;
				_nextObjectUid++;
			}

#if DEBUG

			for (var i = 0; i < _objects.Count; i++)
			{
				GameObject2D otherObj = _objects[i];
				if (_objectsToRemove.Contains(otherObj)) continue;
				Debug.Assert(otherObj.UniqueId != obj.UniqueId);
			}

#endif
			if (obj.ObjectFlags.HasFlag(ObjectFlags.Persistent) && !_loadingPersistentObjects)
			{
				Debug.Assert(!PersistentObjects.Contains(obj));
				PersistentObjects.Add(obj);

				// Serializable objects added before map load will be loaded in bulk and join the map file.
				// This usually occurs when a map is being generated by code.
				if (!_mapLoadedStarted) return;
			}

			// Check if not spawning this object.
			if (!obj.ShouldSpawnSerializedObject(this))
			{
				_objects.Add(obj);
				obj.ObjectState = ObjectState.ConditionallyNonSpawned;
				_worldTree?.AddObjectToTree(obj);

				if (EditorMode) EditorObjectAdded(obj);

				return;
			}

			// Attaches the object to the map
			obj.AttachToMap(this);
			_objects.Add(obj);
			_worldTree?.AddObjectToTree(obj);

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

			if (EditorMode) EditorObjectAdded(obj);
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

		/// <summary>
		/// Remove the object from the map. If the object is serialized with the map it will only
		/// be removed from the loaded map, but not from the map file, and will be restored on map reload.
		/// If the "removeFromMapFile" is specified then it will be removed from the map file all together.
		/// Take note that for this to take permanent persistent effect the map file needs to be saved to the disk.
		/// </summary>
		public void RemoveObject(GameObject2D obj, bool removeFromMapFile = false)
		{
			_objectsToRemove.Enqueue(obj);

			if (removeFromMapFile && PersistentObjects.Contains(obj)) PersistentObjects.Remove(obj);

			if (EditorMode) EditorObjectRemoved(obj);
		}

		/// <summary>
		/// Tell the world tree that an object has moved or resized.
		/// This will cause it to find a new node on the next update tick.
		/// Take note that until the next update tick it will be returned in
		/// queries that target the bounds of its old node. This only
		/// affects objects that have moved/resized a great lot.
		/// </summary>
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

			Rectangle clipArea = c.Camera.GetCameraFrustum();
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

		public virtual void Dispose()
		{
			DisposeDebug();
			Disposed = true;
		}
	}
}