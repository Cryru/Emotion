#nullable enable

using Emotion.Game.World;
using Emotion.Game.World2D;
using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Game.OctTree;

public interface IOctTreeStorable
{
    public Cube GetOctTreeBound();
}

public class OctTreeNode<T> where T : IOctTreeStorable
{
    public OctTreeNode<T>? Parent;
    public int MaxDepth = 0;
    public int Capacity = 0;

    public Cube Bounds;

    protected OctTreeNode<T>[]? _childNodes;
    protected List<T>? _items;

    public OctTreeNode(OctTreeNode<T>? parent, Cube bounds, int capacity = 3, int maxDepth = 5)
    {
        Parent = parent;
        Bounds = bounds;
        Capacity = capacity;
        MaxDepth = maxDepth;
    }

    public OctTreeNode<T> _InternalAdd(Cube bounds, T item)
    {
        _items ??= new List<T>();
        if (_items.Count + 1 > Capacity && _childNodes == null && MaxDepth > 0)
        {
            //float halfWidth = Bounds.Width / 2;
            //float halfHeight = Bounds.Height / 2;

            _childNodes = new OctTreeNode<T>[8];
            //_childNodes[0] = new OctTreeNode<T>(this, new Rectangle(Bounds.X, Bounds.Y, halfWidth, halfHeight), Capacity, MaxDepth - 1);
            //_childNodes[1] = new OctTreeNode<T>(this, new Rectangle(Bounds.X + halfWidth, Bounds.Y, halfWidth, halfHeight), Capacity, MaxDepth - 1);
            //_childNodes[2] = new OctTreeNode<T>(this, new Rectangle(Bounds.X, Bounds.Y + halfHeight, halfWidth, halfHeight), Capacity, MaxDepth - 1);
            //_childNodes[3] = new OctTreeNode<T>(this, new Rectangle(Bounds.X + halfWidth, Bounds.Y + halfHeight, halfWidth, halfHeight), Capacity, MaxDepth - 1);

            OctTreeNode<T> subNode = GetNodeForBounds(bounds);
            return subNode._InternalAdd(bounds, item);
        }

        Assert(_items.IndexOf(item) == -1);
        _items.Add(item);
        return this;
    }

    public OctTreeNode<T> GetNodeForBounds(Cube bounds)
    {
        if (_childNodes == null) return this;

        for (var i = 0; i < _childNodes.Length; i++)
        {
            OctTreeNode<T> node = _childNodes[i];
            if (node.Bounds.ContainsInclusive(bounds))
                return node.GetNodeForBounds(bounds);
        }

        return this;
    }
}

public class OctTree<T> : OctTreeNode<T> where T : IOctTreeStorable
{
    public int MaxObjectsOutsideBounds { get; init; }

    private Dictionary<T, OctTreeNode<T>> _objToNode = new();

    public OctTree(int maxObjectsOutsideBounds = 5) : base(null, Cube.Empty)
    {
        MaxObjectsOutsideBounds = maxObjectsOutsideBounds;
    }

    public void Add(T item)
    {
        Cube bounds = item.GetOctTreeBound();
        OctTreeNode<T> node = GetNodeForBounds(bounds);
        node = node._InternalAdd(bounds, item);

        if (node == this)
        {
            // MaxObjectsOutsideBounds

        }

        // Crash prevention in error cases.
        if (_objToNode.ContainsKey(item))
        {
            Assert(false, "Object already present in WorldTreeNode");
            return;
        }

        _objToNode.Add(item, node);
    }

    public void Remove(T item)
    {

    }

    public void Update(T item)
    {
        if (!_objToNode.TryGetValue(item, out OctTreeNode<T>? node))
            Add(item);


    }

    public void RenderDebug(RenderComposer c)
    {

    }
}
