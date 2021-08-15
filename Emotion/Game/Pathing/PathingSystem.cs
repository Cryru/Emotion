#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Game.QuadTree;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Pathing
{
    public class PathingSystem
    {
        public QuadTree<PathingActor> Actors;
        public QuadTree<PathingActor> DynamicActors;

        public PathingSystem(PathingWorld world)
        {
            Actors = new QuadTree<PathingActor>(new Rectangle(0, 0, world.Size));
            DynamicActors = new QuadTree<PathingActor>(new Rectangle(0, 0, world.Size));
            for (var i = 0; i < world.Actors.Count; i++)
            {
                PathingActor actor = world.Actors[i];
                Actors.Add(world.Actors[i]);
                if (actor.Type == PathingActorType.Dynamic) DynamicActors.Add(actor);
            }
        }

        public bool ValidateWorld()
        {
            foreach (PathingActor dynActor in DynamicActors)
            {
                List<PathingActor> objects = Actors.GetObjects(dynActor.Bounds);
                if (objects.Count > 1) return false;
            }

            return true;
        }

        class PotentialPosition
        {
            public Vector2 Position;
        }

        unsafe void GetPotentialPositionsAround(PathingActor actor, float neighbourhoodDistance, List<Vector2> test)
        {
           // Rectangle* segments = stackalloc Rectangle[1];
            var queryMemory = new List<PathingActor>();

            // Find extent in all possible movement directions.
            Rectangle actorBound = actor.Bounds;
            actorBound.Size = actorBound.Size * new Vector2(10);
            foreach (Vector2 dir in Maths.CardinalDirections2D)
            {
                Vector2 vec = dir * neighbourhoodDistance;
                var directionPolygon = new Polygon(
                    actorBound.TopLeft.ToVec3(), (actorBound.TopLeft + vec).ToVec3(),
                    actorBound.TopRight.ToVec3(), (actorBound.TopRight + vec).ToVec3(),
                    actorBound.BottomLeft.ToVec3(), (actorBound.BottomLeft + vec).ToVec3(),
                    actorBound.BottomRight.ToVec3(), (actorBound.BottomRight + vec).ToVec3()
                );
                directionPolygon.CleanupPolygon();
                //directionPolygon.Triangulate();

                foreach (var line in actorBound.GetLineSegments())
                {
                    test.Add(line.Start);
                }

                var TwoDeeCont = directionPolygon.Get2DContour();
                for (int i = 0; i < TwoDeeCont.Count; i++)
                {
                    test.Add(TwoDeeCont[i].Start);
                    //test.Add(TwoDeeCont[i].End);
                }
                break;
                //segments[0] = Rectangle.FromMinMaxPointsChecked(actorBound.TopLeft, actorBound.TopLeft * vec);

                //queryMemory.Clear();
                //Actors.GetObjects(ref segments[0], queryMemory);
                //for (int i = 0; i < queryMemory.Count; i++)
                //{
                //    var collidedWith = queryMemory[i];
                //    if (collidedWith != actor)
                //    {

                //    }
                //}

                //segments[1] = new(actorBound.TopRight, actorBound.TopLeft * vec);
                //segments[2] = new(actorBound.BottomLeft, actorBound.TopLeft * vec);
                //segments[3] = new(actorBound.BottomRight, actorBound.TopLeft * vec);

            }
        }

        public List<Vector2> CalculatePathTo(PathingActor actor, Vector2 point)
        {
            var p = new List<Vector2>();

            var actorBound = actor.Bounds;
            var moveIncrement = 1.0f;

            GetPotentialPositionsAround(actor, 30f, p);

            //p.Add(actor.Center);
            //p.Add(point);


            return p;
        }
    }
}