#nullable enable

#region Using

using System.Runtime.CompilerServices;
using Emotion.Game.World2D;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World
{
    /// <summary>
    /// Used to filter objects. (Except by type and position in shape)
    /// </summary>
    public struct MapQueryFilterArgs
    {
        public ObjectState? InState;

        public MapQueryFilterArgs()
        {
            InState = ObjectState.Alive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Passes(MapQueryFilterArgs filter, BaseGameObject obj)
        {
            var inState = filter.InState;
            if (inState != null && obj.ObjectState != inState) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Passes(BaseGameObject obj, ObjectState? state)
        {
            if (state != null && obj.ObjectState != state) return false;
            return true;
        }
    }

    public partial class BaseMap
    {
        #region Enum

        public ref struct MapQueryEnumerator
        {
            public MapQueryFilterArgs Filter;
            private List<BaseGameObject> _objects;

            public BaseGameObject Current { get; private set; } = null!;
            private int _currentIndex;

            public MapQueryEnumerator(List<BaseGameObject> objects)
            {
                _objects = objects;
            }

            public bool MoveNext()
            {
                while (_currentIndex < _objects.Count)
                {
                    var obj = _objects[_currentIndex];
                    _currentIndex++;

                    if (!MapQueryFilterArgs.Passes(Filter, obj)) continue;
                    Current = obj;
                    return true;
                }

                Current = null!;
                return false;
            }

            public MapQueryEnumerator GetEnumerator()
            {
                return this;
            }
        }

        public ref struct MapQueryEnumeratorObjTypeOnly<ObjTypeFilter>
            where ObjTypeFilter : BaseGameObject
        {
            public MapQueryFilterArgs Filter;
            private List<BaseGameObject> _objects;

            public ObjTypeFilter Current { get; private set; } = null!;
            private int _currentIndex;

            public MapQueryEnumeratorObjTypeOnly(List<BaseGameObject> objects)
            {
                _objects = objects;
            }

            public bool MoveNext()
            {
                while (_currentIndex < _objects.Count)
                {
                    var obj = _objects[_currentIndex];
                    _currentIndex++;

                    if (!MapQueryFilterArgs.Passes(Filter, obj)) continue;
                    if (obj is not ObjTypeFilter nextAsType) continue;

                    Current = nextAsType;
                    return true;
                }

                Current = null!;
                return false;
            }

            public MapQueryEnumeratorObjTypeOnly<ObjTypeFilter> GetEnumerator()
            {
                return this;
            }
        }

        public ref struct MapQueryEnumeratorShapeOnly<ShapeFilterType>
            where ShapeFilterType : struct, IShape
        {
            public MapQueryFilterArgs Filter;
            public ShapeFilterType InShape;

            private WorldTree2DNode _currentNode;
            private Stack<WorldTree2DNode> _iterationStack;

            public BaseGameObject Current { get; private set; } = null!;
            private int _currentIndex;

            public MapQueryEnumeratorShapeOnly(WorldTree2DNode worldTreeRoot)
            {
                _iterationStack = _iterationStackPool.Get();
                _iterationStack.Clear();
                _iterationStack.Push(worldTreeRoot);
            }

            public void Dispose()
            {
                if (_iterationStack != null)
                {
                    _iterationStackPool.Return(_iterationStack);
                    _iterationStack = null;
                }
            }

            public bool MoveNext()
            {
                var stack = _iterationStack;
                var currentNode = _currentNode;

                var objects = currentNode?.Objects;
                while (objects != null && _currentIndex < objects.Count)
                {
                    var obj = objects[_currentIndex];
                    _currentIndex++;

                    if (!MapQueryFilterArgs.Passes(Filter, obj)) continue;

                    Rectangle bounds = obj.GetBoundsForQuadTree();
                    if (!InShape.Intersects(ref bounds)) continue;

                    Current = obj;
                    return true;
                }

                if (stack.Count > 0)
                {
                    _currentNode = stack.Pop();
                    _currentIndex = 0;

                    if (_currentNode.ChildNodes != null)
                        for (int i = 0; i < _currentNode.ChildNodes.Length; i++)
                        {
                            WorldTree2DNode? childNode = _currentNode.ChildNodes[i];
                            if (InShape.Intersects(ref childNode.Bounds))
                                stack.Push(childNode);
                        }

                    return MoveNext();
                }

                Current = null!;
                return false;
            }

            public MapQueryEnumeratorShapeOnly<ShapeFilterType> GetEnumerator()
            {
                return this;
            }
        }

        public ref struct MapQueryEnumerator<ObjTypeFilter, ShapeFilterType>
            where ObjTypeFilter : BaseGameObject
            where ShapeFilterType : struct, IShape
        {
            public MapQueryFilterArgs Filter;
            public ShapeFilterType InShape;

            private WorldTree2DNode _currentNode;
            private Stack<WorldTree2DNode> _iterationStack;

            public ObjTypeFilter Current { get; private set; } = null!;
            private int _currentIndex;

            public MapQueryEnumerator(WorldTree2DNode worldTreeRoot)
            {
                _iterationStack = _iterationStackPool.Get();
                _iterationStack.Clear();
                _iterationStack.Push(worldTreeRoot);
            }

            public void Dispose()
            {
                if (_iterationStack != null)
                {
                    _iterationStackPool.Return(_iterationStack);
                    _iterationStack = null;
                }
            }

            public bool MoveNext()
            {
                var stack = _iterationStack;
                var currentNode = _currentNode;

                var objects = currentNode?.Objects;
                while (objects != null && _currentIndex < objects.Count)
                {
                    var obj = objects[_currentIndex];
                    _currentIndex++;

                    if (!MapQueryFilterArgs.Passes(Filter, obj)) continue;
                    if (obj is not ObjTypeFilter nextAsType) continue;

                    Rectangle bounds = obj.GetBoundsForQuadTree();
                    if (!InShape.Intersects(ref bounds)) continue;

                    Current = nextAsType;
                    return true;
                }

                if (stack.Count > 0)
                {
                    _currentNode = stack.Pop();
                    _currentIndex = 0;

                    if (_currentNode.ChildNodes != null)
                        for (int i = 0; i < _currentNode.ChildNodes.Length; i++)
                        {
                            WorldTree2DNode? childNode = _currentNode.ChildNodes[i];
                            if (InShape.Intersects(ref childNode.Bounds))
                                stack.Push(childNode);
                        }

                    return MoveNext();
                }

                Current = null!;
                return false;
            }

            public MapQueryEnumerator<ObjTypeFilter, ShapeFilterType> GetEnumerator()
            {
                return this;
            }
        }

        /// <summary>
        /// Enum - No Type Constraint
        /// </summary>
        public MapQueryEnumerator ObjectsEnum(ObjectState? inState = ObjectState.Alive)
        {
            return new MapQueryEnumerator(_objects)
            {
                Filter = new MapQueryFilterArgs
                {
                    InState = inState
                }
            };
        }

        /// <summary>
        /// Enum - Type Constraint
        /// </summary>
        public MapQueryEnumeratorObjTypeOnly<ObjTypeFilter> ObjectsEnum<ObjTypeFilter>(ObjectState? inState = ObjectState.Alive)
            where ObjTypeFilter : BaseGameObject
        {
            return new MapQueryEnumeratorObjTypeOnly<ObjTypeFilter>(_objects)
            {
                Filter = new MapQueryFilterArgs
                {
                    InState = inState
                }
            };
        }

        /// <summary>
        /// Enum - No Type Constraint, Shape Constraint
        /// </summary>
        public MapQueryEnumeratorShapeOnly<ShapeFilterType> ObjectsEnum<ShapeFilterType>(ShapeFilterType inShape, int layer = 0, ObjectState? inState = ObjectState.Alive)
            where ShapeFilterType : struct, IShape
        {
            return new MapQueryEnumeratorShapeOnly<ShapeFilterType>(_worldTree.GetRootNodeForLayer(layer))
            {
                InShape = inShape,
                Filter = new MapQueryFilterArgs
                {
                    InState = inState
                }
            };
        }

        /// <summary>
        /// Enum - Type Constraint, Shape Constraint
        /// </summary>
        public MapQueryEnumerator<ObjTypeFilter, ShapeFilterType> ObjectsEnum<ObjTypeFilter, ShapeFilterType>(ShapeFilterType inShape, int layer = 0, ObjectState? inState = ObjectState.Alive)
            where ObjTypeFilter : BaseGameObject
            where ShapeFilterType : struct, IShape
        {
            return new MapQueryEnumerator<ObjTypeFilter, ShapeFilterType>(_worldTree.GetRootNodeForLayer(layer))
            {
                InShape = inShape,
                Filter = new MapQueryFilterArgs
                {
                    InState = inState
                }
            };
        }

        #endregion

        #region Callback

        public interface IMapForEachQuery
        {
            void Invoke(BaseGameObject o);
        }

        /// <summary>
        /// QueryEnvelope - No Type Constraint
        /// </summary>
        public struct MapQueryEnvelope<T1> : IMapForEachQuery
        {
            public MapQueryFilterArgs Filter;
            public Action<BaseGameObject, T1>? Func;
            public T1? Arg;

            public void Invoke(BaseGameObject obj)
            {
                if (!MapQueryFilterArgs.Passes(Filter, obj)) return;
                if (Func != null) Func(obj, Arg);
            }
        }

        /// <summary>
        /// QueryEnvelope - Type Constraint
        /// </summary>
        public struct MapQueryEnvelope<ObjTypeFilter, T1> : IMapForEachQuery where ObjTypeFilter : BaseGameObject
        {
            public MapQueryFilterArgs Filter;
            public Action<ObjTypeFilter, T1>? Func;
            public T1? Arg;

            public void Invoke(BaseGameObject obj)
            {
                if (!MapQueryFilterArgs.Passes(Filter, obj)) return;

                ObjTypeFilter? objAsFilterType = obj as ObjTypeFilter;
                if (objAsFilterType == null) return;

                if (Func != null) Func(objAsFilterType, Arg);
            }
        }

        /// <summary>
        /// ForEach Object - With Shape Constraint
        /// </summary>
        public void ObjectForEach<MapQueryObj, ShapeFilterType>(ref MapQueryObj callback, ref ShapeFilterType inShape, int layer = 0)
            where MapQueryObj : struct, IMapForEachQuery
            where ShapeFilterType : struct, IShape
        {
            //if (callback.Func == null) return;

            WorldTree2DRootNode? rootNode = _worldTree?.GetRootNodeForLayer(layer);
            if (rootNode != null)
                ForEachObjectInShape(rootNode, ref callback, ref inShape);
        }

        /// <summary>
        /// ForEach Object - No Shape Constraint
        /// </summary>
        public void ObjectForEach<MapQueryObj>(ref MapQueryObj callback)
            where MapQueryObj : struct, IMapForEachQuery
        {
            //if (callback.Func == null) return;

            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                callback.Invoke(obj);
            }
        }

        #endregion

        #region List Fill (Fastest)

        /// <summary>
        /// GetObjects - Type and Shape Constraint
        /// </summary>
        public List<ObjTypeFilter> ObjectsGet<ObjTypeFilter, ShapeFilterType>(List<ObjTypeFilter>? list, ShapeFilterType inShape, int layer = 0,
            ObjectState? inState = ObjectState.Alive)
            where ObjTypeFilter : BaseGameObject
            where ShapeFilterType : struct, IShape
        {
            list ??= new List<ObjTypeFilter>();
            list.Clear();

            MapQueryFilterArgs filter = new MapQueryFilterArgs
            {
                InState = inState
            };

            WorldTree2DRootNode? rootNode = _worldTree?.GetRootNodeForLayer(layer);
            if (rootNode != null)
                GetObjectsInShape(rootNode, list, ref inShape, ref filter);

            return list;
        }

        /// <summary>
        /// GetObjects - Type Constraint, No Shape Constraint
        /// </summary>
        public List<ObjTypeFilter> ObjectsGet<ObjTypeFilter>(List<ObjTypeFilter>? list,
            ObjectState? inState = ObjectState.Alive)
            where ObjTypeFilter : BaseGameObject
        {
            list ??= new List<ObjTypeFilter>();
            list.Clear();

            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                if (!MapQueryFilterArgs.Passes(obj, inState)) continue;
                if (obj is not ObjTypeFilter objOfType) continue;

                list.Add(objOfType);
            }

            return list;
        }

        /// <summary>
        /// GetObjects - No Type Constraint, No Shape Constraint
        /// </summary>
        public List<BaseGameObject> ObjectsGet(List<BaseGameObject>? list,
            ObjectState? inState = ObjectState.Alive)
        {
            list ??= new List<BaseGameObject>();
            list.Clear();

            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                if (!MapQueryFilterArgs.Passes(obj, inState)) continue;
                list.Add(obj);
            }

            return list;
        }

        #endregion

        // Enumerators for the world tree, used by other functions above.

        #region WorldTree Walkers

        private void ForEachObjectInShape<MapQueryObj, ShapeFilterType>(WorldTree2DNode node, ref MapQueryObj callback, ref ShapeFilterType shape)
            where MapQueryObj : struct, IMapForEachQuery
            where ShapeFilterType : struct, IShape
        {
            var objects = node.Objects;
            if (objects == null) return;

            for (var i = 0; i < objects.Count; i++)
            {
                BaseGameObject obj = objects[i];

                Rectangle bounds = obj.GetBoundsForQuadTree();
                if (!shape.Intersects(ref bounds)) continue;

                callback.Invoke(obj);
            }

            var childNodes = node.ChildNodes;
            if (childNodes == null) return;
            for (int i = 0; i < childNodes.Length; i++)
            {
                WorldTree2DNode? childNode = childNodes[i];
                if (shape.Intersects(ref childNode.Bounds))
                    ForEachObjectInShape(childNode, ref callback, ref shape);
            }
        }

        private void GetObjectsInShape<ObjTypeFilter, ShapeFilterType>(WorldTree2DNode node, List<ObjTypeFilter> list, ref ShapeFilterType shape, ref MapQueryFilterArgs filter)
            where ObjTypeFilter : BaseGameObject
            where ShapeFilterType : struct, IShape
        {
            var objects = node.Objects;
            if (objects == null) return;

            for (var i = 0; i < objects.Count; i++)
            {
                BaseGameObject obj = objects[i];

                if (!MapQueryFilterArgs.Passes(filter, obj)) continue;

                Rectangle bounds = obj.GetBoundsForQuadTree();
                if (!shape.Intersects(ref bounds)) continue;

                if (obj is not ObjTypeFilter objAsNext) continue;

                list.Add(objAsNext);
            }

            var childNodes = node.ChildNodes;
            if (childNodes == null) return;
            for (int i = 0; i < childNodes.Length; i++)
            {
                WorldTree2DNode? childNode = childNodes[i];
                if (shape.Intersects(ref childNode.Bounds))
                    GetObjectsInShape(childNode, list, ref shape, ref filter);
            }
        }

        // A pool of reusable stack objects used to iterate the tree.
        private static ObjectPool<Stack<WorldTree2DNode>> _iterationStackPool = new ObjectPool<Stack<WorldTree2DNode>>(
            () => { return new Stack<WorldTree2DNode>(32); }, s => { s.Clear(); }, 1);

        #endregion
    }
}