#nullable enable

namespace Emotion.Game.World2D
{
	public class WorldTree2DRootNode : WorldTree2DNode
	{
		protected Dictionary<GameObject2D, WorldTree2DNode> _objToNode = new();

		public WorldTree2DRootNode(Rectangle bounds) : base(null, bounds)
		{
		}

		public void AddObjectToRoot(GameObject2D obj)
		{
			Rectangle bounds = obj.GetBoundsForQuadTree();
			WorldTree2DNode node = GetNodeForBounds(bounds);
			node = node.AddObject(bounds, obj);

			if (_objToNode.ContainsKey(obj)) return; // Crash prevention in error cases.
			_objToNode.Add(obj, node);
		}

		public void RemoveObjectFromRoot(GameObject2D obj)
		{
			if (!_objToNode.TryGetValue(obj, out WorldTree2DNode? node)) return;
			node.RemoveObject(obj);
			_objToNode.Remove(obj);
		}

		public WorldTree2DNode? GetNodeForObject(GameObject2D obj)
		{
			_objToNode.TryGetValue(obj, out WorldTree2DNode? node);
			return node;
		}

		public void UpdateObject(GameObject2D obj)
		{
			_objToNode.TryGetValue(obj, out WorldTree2DNode? node);
			Assert(node != null);

			Rectangle bounds = obj.GetBoundsForQuadTree();
			WorldTree2DNode newNode = GetNodeForBounds(bounds);
			if (newNode == node) return;

			// Remove from old node, and add to new.
			_objToNode.Remove(obj);
			node.RemoveObject(obj);
			newNode = newNode.AddObject(bounds, obj);
			_objToNode.Add(obj, newNode);
		}
	}
}