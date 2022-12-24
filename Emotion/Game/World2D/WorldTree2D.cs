#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common.Serialization;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    [DontSerialize]
    public class WorldTree2D
    {
        private Dictionary<int, WorldTree2DRootNode> _rootNodes = new();
        private Vector2 _mapSize;

        private List<GameObject2D> _objects = new();

        public WorldTree2D(Vector2 mapSize)
        {
            _mapSize = mapSize;
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
                if (obj.IsPartOfMapLayer(layerId)) newLayerTopNode.AddObjectRoot(obj);
            }
        }

        /// <summary>
        /// Add an object to the tree. It will be part of the layers filtered by its IsPartOfMapLayer function.
        /// </summary>
        public void AddObjectToTree(GameObject2D obj)
        {
            _objects.Add(obj);
            foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
            {
                if (rootNode.Key == 0 || obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.AddObjectRoot(obj);
            }
        }

        /// <summary>
        /// Remove an object from all layers in the tree.
        /// </summary>
        public void RemoveObjectFromTree(GameObject2D obj)
        {
            if (!_objects.Remove(obj)) return;
            foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
            {
                if (rootNode.Key == 0 || obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.RemoveObjectRoot(obj);
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