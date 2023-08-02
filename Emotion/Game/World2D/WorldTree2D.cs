#region Using

using Emotion.Common.Serialization;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	/// <summary>
	/// The world tree is a quad tree that makes querying objects faster.
	/// The tree also groups objects in "layers". Each layer has a game specific meaning
	/// and speeds up filtering through objects by grouping them in advance.
	/// There are two special layers:
	/// Layer -1 contains all ConditionallyNonSpawned objects
	/// Layer 0 contains all objects.
	/// </summary>
	[DontSerialize]
	public class WorldTree2D
	{
		private Dictionary<int, WorldTree2DRootNode> _rootNodes = new();
		private Vector2 _mapSize;

		private List<GameObject2D> _objects = new();

		public WorldTree2D(Vector2 mapSize)
		{
			_mapSize = mapSize;
			AddTreeLayer(-1);
			AddTreeLayer(0);
		}

		/// <summary>
		/// Add a layer to the world tree. Layers are used to separate objects when querying, and
		/// are denominated by a numerical index.
		/// </summary>
		public void AddTreeLayer(int layerId)
		{
			var newLayerTopNode = new WorldTree2DRootNode(new Rectangle(0, 0, _mapSize));
			_rootNodes.Add(layerId, newLayerTopNode);

			for (var i = 0; i < _objects.Count; i++)
			{
				GameObject2D obj = _objects[i];
				if (obj.ObjectState == ObjectState.ConditionallyNonSpawned) continue;
				if (obj.IsPartOfMapLayer(layerId)) newLayerTopNode.AddObjectToRoot(obj);
			}
		}

		/// <summary>
		/// Add an object to the tree. It will be part of the layers filtered by its IsPartOfMapLayer function.
		/// </summary>
		public void AddObjectToTree(GameObject2D obj)
		{
			_objects.Add(obj);

			if (obj.ObjectState == ObjectState.ConditionallyNonSpawned)
			{
				_rootNodes[-1].AddObjectToRoot(obj);
				return;
			}

			foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
			{
				if (rootNode.Key == 0 || obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.AddObjectToRoot(obj);
			}
		}

		/// <summary>
		/// Remove an object from all layers in the tree.
		/// </summary>
		public void RemoveObjectFromTree(GameObject2D obj)
		{
			if (!_objects.Remove(obj)) return;

			// We check if the object is part of the "Unspawned Objects" layer
			// rather than its state because it could have changed. If it was part of this
			// layer there is no shot it was part of any other layer.
			// Also we cant use IsPartOfMapLayer to check since the object is not natively
			// part of this layer and must instead ask the layer to tell us whether it is present.
			var unspawnedNode = _rootNodes[-1];
			if (unspawnedNode.GetNodeForObject(obj) != null)
			{
				unspawnedNode.RemoveObjectFromRoot(obj);

				if (Engine.Configuration.DebugMode)
					foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
					{
						Assert(rootNode.Value.GetNodeForObject(obj) == null);
					}

				return;
			}

			foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
			{
				if (rootNode.Key == 0 || obj.IsPartOfMapLayer(rootNode.Key))
					rootNode.Value.RemoveObjectFromRoot(obj);
			}
		}

		/// <summary>
		/// Update an object's bounds in all layers it can be found in.
		/// </summary>
		public void UpdateObjectInTree(GameObject2D obj)
		{
			foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
			{
				if (rootNode.Key == 0 || obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.UpdateObject(obj);
			}
		}

		/// <summary>
		/// Get the root node for a specific tree layer.
		/// </summary>
		public WorldTree2DRootNode? GetRootNodeForLayer(int layer)
		{
			_rootNodes.TryGetValue(layer, out WorldTree2DRootNode? rootNode);
			return rootNode;
		}

		/// <summary>
		/// Loop through all tree layers.
		/// </summary>
		public IEnumerable<int> ForEachLayer()
		{
			foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
			{
				yield return rootNode.Key;
			}
		}
	}
}