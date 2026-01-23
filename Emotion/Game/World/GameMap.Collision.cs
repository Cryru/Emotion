#nullable enable

using Emotion.Game.World.Components;
using Emotion.Game.World.Enumeration;
using Emotion.Game.World.Terrain;
using static Emotion.Game.World.Enumeration.ObjectEnumerationSystem;

namespace Emotion.Game.World;

public partial class GameMap
{
    //private OctTree<GameObject> _octTree = new();
    private readonly ObjectEnumerationSystem _objectStorage = new();

    public bool CollideWithRayFirst<T>(Ray2D ray, GameObject? exclude, out GameObject? hit, out Vector2 collisionPoint)
       where T : GameObject
    {
        foreach (GameObject obj in ForEachObject())
        {
            if (obj == exclude) continue;
            if (obj is not T) continue;

            Rectangle rect = obj.GetBoundingRect();
            if (ray.IntersectWithRectangle(rect, out collisionPoint))
            {
                hit = obj;
                return true;
            }
        }

        collisionPoint = Vector2.Zero;
        hit = null;
        return false;
    }

    public bool CollideWithRayFirst<T>(Ray2D ray, out GameObject? hit, out Vector2 collisionPoint)
        where T : GameObject
    {
        return CollideWithRayFirst<T>(ray, null, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, out GameObject? hit, out Vector2 collisionPoint)
    {
        return CollideWithRayFirst<GameObject>(ray, null, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, GameObject? exclude, out GameObject? hit, out Vector2 collisionPoint)
    {
        return CollideWithRayFirst<GameObject>(ray, exclude, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, out GameObject? hit)
    {
        return CollideWithRayFirst<GameObject>(ray, null, out hit, out _);
    }

    public bool CollideWithRayFirst(Ray3D ray, GameObject? exclude, out GameObject? hit, out Vector3 collisionPoint)
    {
        // todo: oct-tree
        foreach (GameObject obj in ForEachObject())
        {
            if (obj == exclude) continue;

            if (obj.GetComponent<MeshComponent>(out MeshComponent? meshComponent))
            {
                // Object vertices check for mesh objects.
                if (ray.IntersectWithObject(obj, meshComponent, out _, out collisionPoint, out _, out _))
                {
                    hit = obj;
                    return true;
                }
            }
            else
            {
                Cube bounds = obj.GetBoundingCube();
                if (ray.IntersectWithCube(bounds, out collisionPoint, out Vector3 _))
                {
                    hit = obj;
                    return true;
                }
            }
        }

        collisionPoint = Vector3.Zero;
        hit = null;
        return false;
    }

    public bool CollideWithRayFirst(Ray3D ray, out GameObject? hit)
    {
        return CollideWithRayFirst(ray, null, out hit, out _);
    }

    public bool CollideWithCube<TUserData>(Cube cube, GameObject? exclude, Func<Cube, TUserData, bool> onIntersect, TUserData userData)
    {
        var terrainGrid = GetFirstGridOfType<ITerrainGrid3D>();
        if (terrainGrid != null)
        {
            if (terrainGrid.CollideWithCube(cube, onIntersect, userData))
                return true;
        }

        // todo: objects

        return false;
    }

    public Vector3 SweepCube(Cube cube, Vector3 movement, GameObject? exclude)
    {
        var terrainGrid = GetFirstGridOfType<ITerrainGrid3D>();
        if (terrainGrid != null)
        {
            movement = terrainGrid.SweepCube(cube, movement);
        }

        // todo: objects

        return movement;
    }

    public ObjectEnumerator ForEachObject()
    {
        return _objectStorage.GetEnumerator();
    }

    public GameObject? GetObjectById(uint id)
    {
        Assert(id > 0);

        // todo;
        foreach (GameObject obj in ForEachObject())
        {
            if (obj.ObjectId == id)
                return obj;
        }

        return null;
    }

    //public IEnumerable<T> ForEachObject<T>(bool includeNonLoaded = false) where T : GameObject
    //{
    //    foreach (GameObject obj in ForEachObject())
    //    {
    //        if (obj is T objT)
    //            yield return objT;
    //    }

    //    if (includeNonLoaded)
    //    {
    //        foreach (GameObject obj in _objectsToLoad)
    //        {
    //            if (obj is T objT)
    //                yield return objT;
    //        }
    //    }
    //}
}
