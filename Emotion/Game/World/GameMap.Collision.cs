#nullable enable

using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TwoDee;
using Emotion.Primitives.DataStructures.OctTree;

namespace Emotion.Game.World;

public partial class GameMap
{
    private OctTree<MapObject> _octTree = new();

    public bool CollideWithRayFirst<T>(Ray2D ray, MapObject? exclude, out MapObject? hit, out Vector2 collisionPoint)
       where T : MapObject2D
    {
        foreach (MapObject obj in ForEachObject())
        {
            if (obj == exclude) continue;
            if (obj is not T) continue;

            Rectangle rect = obj.BoundingRect;
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

    public bool CollideWithRayFirst<T>(Ray2D ray, out MapObject? hit, out Vector2 collisionPoint)
        where T : MapObject2D
    {
        return CollideWithRayFirst<T>(ray, null, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, out MapObject? hit, out Vector2 collisionPoint)
    {
        return CollideWithRayFirst<MapObject2D>(ray, null, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, MapObject? exclude, out MapObject? hit, out Vector2 collisionPoint)
    {
        return CollideWithRayFirst<MapObject2D>(ray, exclude, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, out MapObject? hit)
    {
        return CollideWithRayFirst<MapObject2D>(ray, null, out hit, out _);
    }

    public bool CollideWithRayFirst(Ray3D ray, MapObject? exclude, out MapObject? hit, out Vector3 collisionPoint)
    {
        // todo: oct-tree
        foreach (MapObject obj in ForEachObject())
        {
            if (obj == exclude) continue;

            if (obj is MapObjectMesh meshObject)
            {
                // Object vertices check for mesh objects.
                if (ray.IntersectWithObject(meshObject, out _, out collisionPoint, out _, out _))
                {
                    hit = obj;
                    return true;
                }
            }
            else
            {
                Cube bounds = obj.BoundingCube;
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

    public bool CollideWithRayFirst(Ray3D ray, out MapObject? hit)
    {
        return CollideWithRayFirst(ray, null, out hit, out _);
    }

    public bool CollideWithCube<TUserData>(Cube cube, MapObject? exclude, Func<Cube, TUserData, bool> onIntersect, TUserData userData)
    {
        if (TerrainGrid != null)
        {
            if (TerrainGrid.CollideWithCube(cube, onIntersect, userData))
                return true;
        }

        // todo: objects

        return false;
    }

    public Vector3 SweepCube(Cube cube, Vector3 movement, MapObject? exclude)
    {
        if (TerrainGrid != null)
        {
            movement = TerrainGrid.SweepCube(cube, movement);
        }

        // todo: objects

        return movement;
    }

    public List<MapObject> ForEachObject()
    {
        return _objects;
    }

    public IEnumerable<T> ForEachObject<T>() where T : MapObject
    {
        // todo: convert to Ienumerable struct with lazy eval
        foreach (MapObject obj in _objects)
        {
            if (obj is T objT)
                yield return objT;
        }
    }
}
