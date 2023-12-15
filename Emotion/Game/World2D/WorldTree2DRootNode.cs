#nullable enable

#region Using

using System.Collections;
using Emotion.Game.World;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World2D
{
    public class WorldTree2DRootNode : WorldTree2DNode
    {
        /// <summary>
        /// A map of each object in the world tree and which subnode holds it.
        /// </summary>
        protected Dictionary<BaseGameObject, WorldTree2DNode> _objToNode = new();

        /// <summary>
        /// A pool of reusable stack objects used to iterate the tree.
        /// </summary>
        private static ObjectPool<Stack<WorldTree2DNode>> _iterationStackPool = new ObjectPool<Stack<WorldTree2DNode>>(
            () => { return new Stack<WorldTree2DNode>(32); }, s => { s.Clear(); }, 1);

        public WorldTree2DRootNode(Rectangle bounds) : base(null, bounds)
        {
        }

        /// <summary>
        /// Add an object to the world tree.
        /// </summary>
        public void AddObjectToRoot(BaseGameObject obj)
        {
            Rectangle bounds = obj.GetBoundsForQuadTree();
            WorldTree2DNode node = GetNodeForBounds(bounds);
            node = node.AddObject(bounds, obj);

            // Crash prevention in error cases.
            if (_objToNode.ContainsKey(obj))
            {
                Assert(false, "Object already present in WorldTreeNode");
                return;
            }

            _objToNode.Add(obj, node);
        }

        /// <summary>
        /// Removes an object from the world tree.
        /// </summary>
        public void RemoveObjectFromRoot(BaseGameObject obj)
        {
            if (!_objToNode.TryGetValue(obj, out WorldTree2DNode? node)) return;
            node.RemoveObject(obj);
            _objToNode.Remove(obj);
        }

        /// <summary>
        /// Returns the subnode that holds the specified object.
        /// Used internally.
        /// </summary>
        public WorldTree2DNode? GetNodeForObject(BaseGameObject obj)
        {
            _objToNode.TryGetValue(obj, out WorldTree2DNode? node);
            return node;
        }

        /// <summary>
        /// Tells the world tree that the bounds of a specific object have changed.
        /// </summary>
        public void UpdateObject(BaseGameObject obj)
        {
            _objToNode.TryGetValue(obj, out WorldTree2DNode? node);
            AssertNotNull(node);

            Rectangle bounds = obj.GetBoundsForQuadTree();
            WorldTree2DNode newNode = GetNodeForBounds(bounds);
            if (newNode == node) return;

            // Remove from old node, and add to new.
            _objToNode.Remove(obj);
            node.RemoveObject(obj);
            newNode = newNode.AddObject(bounds, obj);
            _objToNode.Add(obj, newNode);
        }

        /// <summary>
        /// Creates a coroutine that returns all objects whose bounds intersect
        /// with the passed in IShape.
        /// </summary>
        public IEnumerator<BaseGameObject> AddObjectsIntersectingShape(IShape shape)
        {
            Stack<WorldTree2DNode> stack = _iterationStackPool.Get();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var temp = stack.Pop();

                if (temp.Objects == null) continue;
                for (var i = 0; i < temp.Objects.Count; i++)
                {
                    BaseGameObject obj = temp.Objects[i];
                    Rectangle bounds = obj.GetBoundsForQuadTree();
                    if (!shape.Intersects(ref bounds)) continue;

                    yield return obj;
                }

                if (temp.ChildNodes == null) continue;
                for (int i = 0; i < temp.ChildNodes.Length; i++)
                {
                    WorldTree2DNode? node = temp.ChildNodes[i];
                    if (shape.Intersects(ref node.Bounds)) stack.Push(node);
                }
            }

            _iterationStackPool.Return(stack);
        }

        /// <summary>
        /// Creates a coroutine that yields all objects in the world tree one after another.
        /// </summary>
        public IEnumerator<BaseGameObject> AddAllObjects()
        {
            Stack<WorldTree2DNode> stack = _iterationStackPool.Get();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var temp = stack.Pop();

                if (temp.Objects == null) continue;
                for (var i = 0; i < temp.Objects.Count; i++)
                {
                    BaseGameObject obj = temp.Objects[i];
                    yield return obj;
                }

                if (temp.ChildNodes == null) continue;
                for (int i = 0; i < temp.ChildNodes.Length; i++)
                {
                    WorldTree2DNode? node = temp.ChildNodes[i];
                    stack.Push(node);
                }
            }

            _iterationStackPool.Return(stack);
        }

        /// <summary>
        /// Inserts all objects from the world tree node into the list.
        /// Creates no allocations.
        /// </summary>
        public void AddAllObjects(IList list)
        {
            Stack<WorldTree2DNode> stack = _iterationStackPool.Get();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var temp = stack.Pop();

                if (temp.Objects == null) continue;
                for (var i = 0; i < temp.Objects.Count; i++)
                {
                    BaseGameObject obj = temp.Objects[i];
                    list.Add(obj);
                }

                if (temp.ChildNodes == null) continue;
                for (int i = 0; i < temp.ChildNodes.Length; i++)
                {
                    WorldTree2DNode? node = temp.ChildNodes[i];
                    stack.Push(node);
                }
            }

            _iterationStackPool.Return(stack);
        }
    }
}