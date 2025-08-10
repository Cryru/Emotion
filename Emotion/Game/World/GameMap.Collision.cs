﻿#nullable enable

using Emotion.Game.World.Components;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TwoDee;
using Emotion.Primitives.DataStructures.OctTree;

namespace Emotion.Game.World;

public partial class GameMap
{
    private OctTree<GameObject> _octTree = new();

    public bool CollideWithRayFirst<T>(Ray2D ray, GameObject? exclude, out GameObject? hit, out Vector2 collisionPoint)
       where T : MapObject2D
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
        where T : MapObject2D
    {
        return CollideWithRayFirst<T>(ray, null, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, out GameObject? hit, out Vector2 collisionPoint)
    {
        return CollideWithRayFirst<MapObject2D>(ray, null, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, GameObject? exclude, out GameObject? hit, out Vector2 collisionPoint)
    {
        return CollideWithRayFirst<MapObject2D>(ray, exclude, out hit, out collisionPoint);
    }

    public bool CollideWithRayFirst(Ray2D ray, out GameObject? hit)
    {
        return CollideWithRayFirst<MapObject2D>(ray, null, out hit, out _);
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
        if (TerrainGrid != null)
        {
            if (TerrainGrid.CollideWithCube(cube, onIntersect, userData))
                return true;
        }

        // todo: objects

        return false;
    }

    public Vector3 SweepCube(Cube cube, Vector3 movement, GameObject? exclude)
    {
        if (TerrainGrid != null)
        {
            movement = TerrainGrid.SweepCube(cube, movement);
        }

        // todo: objects

        return movement;
    }

    public List<GameObject> ForEachObject()
    {
        return _objects;
    }

    public IEnumerable<T> ForEachObject<T>() where T : GameObject
    {
        // todo: convert to Ienumerable struct with lazy eval
        foreach (GameObject obj in _objects)
        {
            if (obj is T objT)
                yield return objT;
        }
    }
}
