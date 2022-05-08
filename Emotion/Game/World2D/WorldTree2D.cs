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

        public void AddObjectToTree(GameObject2D obj)
        {
            _objects.Add(obj);
            foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
            {
                if (obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.AddObjectRoot(obj);
            }
        }

        public void RemoveObjectFromTree(GameObject2D obj)
        {
            if (!_objects.Remove(obj)) return;
            foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
            {
                if (obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.RemoveObjectRoot(obj);
            }
        }

        public void UpdateObjectInTree(GameObject2D obj)
        {
            foreach (KeyValuePair<int, WorldTree2DRootNode> rootNode in _rootNodes)
            {
                if (obj.IsPartOfMapLayer(rootNode.Key)) rootNode.Value.UpdateObject(obj);
            }
        }

        public WorldTree2DRootNode? GetRootNodeForLayer(int layer)
        {
            _rootNodes.TryGetValue(layer, out WorldTree2DRootNode? rootNode);
            return rootNode;
        }
    }
}