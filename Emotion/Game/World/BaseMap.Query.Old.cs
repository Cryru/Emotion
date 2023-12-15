#nullable enable

#region Using

using System.Collections;
using System.Linq;
using Emotion.Game.World2D;

#endregion

namespace Emotion.Game.World
{
    public partial class BaseMap
    {
        [Obsolete("Use the new generics object querying API - ObjectsEnum!")]
        public IEnumerable<BaseGameObject> GetObjects(bool includeNonSpawned = false)
        {
            for (var i = 0; i < _objects.Count; i++)
            {
                BaseGameObject obj = _objects[i];
                var objState = obj.ObjectState;
                bool validState = objState == ObjectState.Alive || (includeNonSpawned && objState == ObjectState.ConditionallyNonSpawned);
                if (!validState) continue;
                yield return obj;
            }
        }

        [Obsolete("Use the new generics object querying API - ObjectsEnum!")]
        public IEnumerator<BaseGameObject> GetAllObjectsCoroutine(bool includeNonSpawned = false)
        {
            for (var i = 0; i < _objects.Count; i++)
            {
                BaseGameObject obj = _objects[i];
                var objState = obj.ObjectState;
                bool validState = objState == ObjectState.Alive || (includeNonSpawned && objState == ObjectState.ConditionallyNonSpawned);
                if (!validState) continue;
                yield return obj;
            }
        }

        [Obsolete("Use the new generics object querying API - ObjectsGet!")]
        public void GetObjectsByType<T>(List<T> list, int layer, IShape shape, QueryFlags queryFlags = 0) where T : BaseGameObject
        {
            var objects = new List<BaseGameObject>();
            GetObjects(objects, layer, shape, queryFlags);

            foreach (BaseGameObject obj in objects)
            {
                if (obj is T objT)
                {
                    if (queryFlags.HasFlag(QueryFlags.Unique) && list.Contains(obj)) continue;
                    list.Add(objT);
                }
            }
        }

        [Obsolete("Use the new generics object querying API - ObjectsGet!")]
        public void GetObjectsByType<T>(List<T> list, QueryFlags queryFlags = 0) where T : BaseGameObject
        {
            for (var i = 0; i < _objects.Count; i++)
            {
                BaseGameObject obj = _objects[i];
                if (obj.ObjectState != ObjectState.Alive) continue;
                if (queryFlags.HasFlag(QueryFlags.Unique) && list.Contains(obj)) continue;

                if (obj is T objT) list.Add(objT);
            }
        }

        [Obsolete("Use the new generics object querying API - ObjectsEnum!")]
        public IEnumerator<T> GetObjectsByType<T>(bool includeNonSpawned = false)
        {
            var enumerator = GetAllObjectsCoroutine(includeNonSpawned);
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current;
                if (obj is T objAsT) yield return objAsT;
            }
        }

        [Obsolete("Use the new generics object querying API - ObjectsGet!")]
        public void GetObjects(IList list, int layer, IShape shape, QueryFlags queryFlags = 0)
        {
            WorldTree2DRootNode? rootNode = _worldTree?.GetRootNodeForLayer(layer);
            var enumerator = rootNode?.AddObjectsIntersectingShape(shape);
            if (enumerator == null) return;
            while (enumerator.MoveNext())
            {
                BaseGameObject currentObject = enumerator.Current;
                if (queryFlags.HasFlag(QueryFlags.Unique) && list.Contains(currentObject)) continue;
                list.Add(currentObject);
            }
        }

        public void GetObjects(IList list, int layer)
        {
            WorldTree2DRootNode? rootNode = _worldTree?.GetRootNodeForLayer(layer);
            rootNode?.AddAllObjects(list);
        }

        [Obsolete("Use the new generics object querying API - ObjectsGet!")]
        public List<ObjType> GetObjects<ObjType>(List<ObjType>? list = null) where
            ObjType : BaseGameObject
        {
            list ??= new List<ObjType>();

            var enumerator = GetAllObjectsCoroutine();
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current;
                if (obj is ObjType objAsT) list.Add(objAsT);
            }

            return list;
        }
    }
}