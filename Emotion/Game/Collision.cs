#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game
{
    public class Collision
    {
        public class CollisionNode<T>
        {
            public LineSegment Surface;
            public T Entity;
        }

        public class CollisionResult<T>
        {
            public Vector2 UnobstructedMovement;
            public bool Collided;
            public Vector2 CollidedSurfaceNormal;
            public T Entity;
        }

#if DEBUG
        public static List<CollisionDebugData> LastCollision = new();
        public class CollisionDebugData
        {
            public LineSegment Line;
            public Vector2 LineNormal;
            public Vector2 Movement;
            public float Weight;
            public string CollisionType;
        }
#endif

        /// <summary>
        /// Super simple rectangle collision resolver.
        /// </summary>
        public static Vector2 GenericRectangleCollision(Vector2 movementVector, Rectangle mapBounds, Rectangle c, List<Rectangle> collisions)
        {
            // Check if going out of the map. This should rarely happen.
            while (movementVector.X != 0 && (c.Right + movementVector.X > mapBounds.Right || c.X + movementVector.X < mapBounds.X)) movementVector.X = Maths.AbsSubtract(movementVector.X);
            while (movementVector.Y != 0 && (c.Bottom + movementVector.Y > mapBounds.Bottom || c.Y + movementVector.Y < mapBounds.Y)) movementVector.Y = Maths.AbsSubtract(movementVector.Y);

            // Fast exit if the above checks killed the velocity.
            if (movementVector == Vector2.Zero) return Vector2.Zero;

            float lowestXMovement = movementVector.X;
            float lowestYMovement = movementVector.Y;

            var meBounds = new Rectangle(c.X, c.Y, c.Width, c.Height);

            foreach (Rectangle bound in collisions)
            {
                float curXVel = movementVector.X;
                float curYVel = movementVector.Y;

                meBounds.X = c.X + curXVel;
                while (curXVel != 0 && bound.Intersects(ref meBounds))
                {
                    curXVel = Maths.AbsSubtract(curXVel);
                    meBounds.X = c.X + curXVel;
                }

                meBounds.X = c.X;

                meBounds.Y = c.Y + curYVel;
                while (curYVel != 0 && bound.Intersects(ref meBounds))
                {
                    curYVel = Maths.AbsSubtract(curYVel);
                    meBounds.Y = c.Y + curYVel;
                }

                meBounds.Y = c.Y;

                if (MathF.Abs(curXVel) < MathF.Abs(lowestXMovement)) lowestXMovement = curXVel;
                if (MathF.Abs(curYVel) < MathF.Abs(lowestYMovement)) lowestYMovement = curYVel;

                if (lowestXMovement == 0.0f && lowestYMovement == 0.0f) break;
            }

            return new Vector2(lowestXMovement, lowestYMovement);
        }

        /// <summary>
        /// Runs GenericSegmentCollision in increments of one unit.
        /// Produces most accurate collision results.
        /// </summary>
        public static CollisionResult<T> IncrementalGenericSegmentCollision<T>(Vector2 movementVector, IShape colBound, IEnumerable<CollisionNode<T>> collisionProvider)
        {
            var colResult = new CollisionResult<T>
            {
                UnobstructedMovement = movementVector
            };

            float moveX = movementVector.X;
            float moveY = movementVector.Y;
            Vector2 totalMoved = Vector2.Zero;
            while (moveX != 0 || moveY != 0)
            {
                float moveAmountX;
                if (MathF.Abs(moveX) >= 1.0f)
                {
                    moveAmountX = 1.0f * MathF.Sign(moveX);
                    moveX = Maths.AbsSubtract(moveX);
                }
                else
                {
                    moveAmountX = moveX;
                    moveX = 0;
                }

                float moveAmountY;
                if (MathF.Abs(moveY) > 1.0f)
                {
                    moveAmountY = 1.0f * MathF.Sign(moveY);
                    moveY = Maths.AbsSubtract(moveY);
                }
                else
                {
                    moveAmountY = moveY;
                    moveY = 0.0f;
                }

                // ReSharper disable once PossibleMultipleEnumeration
                colResult = GenericSegmentCollision(new Vector2(moveAmountX + totalMoved.X, moveAmountY + totalMoved.Y), colBound, collisionProvider);
                Vector2 move = colResult.UnobstructedMovement;
                if (move == Vector2.Zero) break;
                totalMoved = move;
            }

            colResult.UnobstructedMovement = totalMoved;
            return colResult;
        }

        /// <summary>
        /// A generic segment and rectangle/circle collision with vector projection.
        /// Circle is better for sliding off corners, but your collision will be as tall as it is wide.
        /// Would be best if could work with an ellipse.
        /// </summary>
        /// <returns></returns>
        public static CollisionResult<T> GenericSegmentCollision<T>(Vector2 movementVector, IShape originalBound, IEnumerable<CollisionNode<T>> collisionProvider)
        {
            var r = new CollisionResult<T>
            {
                UnobstructedMovement = movementVector
            };

            if (movementVector == Vector2.Zero) return r;

#if DEBUG
            LastCollision.Clear();
#endif

            // Colliding with multiple walls in one movement. This most likely means that the collision
            // from one wall pushed you into colliding with another one.
            // Such collisions are not trivially resolvable and just returning 0,0 solves most cases and prevents
            // bouncing and clipping in both convex and concave corners.
            var depth = 0;
            while (depth < 2)
            {
                IShape colShape = originalBound.CloneShape();
                colShape.Center += movementVector;

                // ReSharper disable PossibleMultipleEnumeration
                CollisionNode<T> current;
                if (colShape is Rectangle rShape)
                    current = GetIntersectionCollisionRectangle(collisionProvider, rShape);
                else
                    current = GetIntersectionCollision(collisionProvider, colShape, movementVector);
                // ReSharper enable PossibleMultipleEnumeration

                // No collision, return current movement vector.
                if (current == null)
                {
                    r.UnobstructedMovement = movementVector;
                    return r;
                }

                // Find collided surface normal.
                ref LineSegment surface = ref current.Surface;
                bool? leftSideNormal = surface.IsPointLeftOf(colShape.Center);
                Vector2 normal = surface.GetNormal(!(leftSideNormal ?? true));

                // Disgusting hack to catch rectangles getting caught on acute corners when moving in the direction perpendicular to its shorter side.
                // In these cases the rectangle's bounds intersect with the two segments which form a corner, and the one with the largest weight (surface overlap)
                // might be the non-sloped one - aka the one whose normal is either in the same direction as the movement, or perpendicular to it.
                // These cases are kind of wtf as you are colliding with something's back wall first or hitting the side of a wall at the corners edge.
                //
                // The fix is to enlarge the rectangle in these cases and trust the overlap weight to take care of it (as it should now overlap more with the slope).
                // In any case, this *shouldn't* break anything.
                //
                // Note: This does cause the collision depth to be exceeded in many cases, such as right angled corners, but doesn't break anything.
                if (colShape is Rectangle && Vector2.Dot(normal, movementVector) == 0 || Vector2.Dot(normal, movementVector) == -1)
                {
                    var inflatedRect = (Rectangle)colShape.CloneShape();
                    inflatedRect.Inflate(1, 1);
                    current = GetIntersectionCollisionRectangle(collisionProvider, inflatedRect);
                    if (current != null && current.Surface != surface)
                    {
                        surface = ref current.Surface;
                        leftSideNormal = surface.IsPointLeftOf(colShape.Center);
                        normal = surface.GetNormal(!(leftSideNormal ?? true));
                    }
                }

                r.Collided = true;
                r.CollidedSurfaceNormal = normal;
                if (current != null) r.Entity = current.Entity;
                movementVector -= normal * Vector2.Dot(movementVector, normal);
                depth++;

#if DEBUG
                for (var i = 0; i < LastCollision.Count; i++)
                {
                    CollisionDebugData col = LastCollision[i];
                    if (col == null) continue;
                    if (col.Line != surface) continue;
                    col.Movement = movementVector;
                    break;
                }
#endif
            }

            // Collision depth exceeded. Assuming the subject is not inside any collision, and to prevent
            // that from happening just report no movement.
            r.UnobstructedMovement = Vector2.Zero;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CollisionNode<T> GetIntersectionCollisionRectangle<T>(IEnumerable<CollisionNode<T>> collisionProvider, Rectangle movedBound)
        {
#if DEBUG
            LastCollision.Add(null);
#endif

            Span<LineSegment> boundSurfaces = stackalloc LineSegment[4];
            movedBound.GetLineSegments(boundSurfaces);

            CollisionNode<T> closestNode = null;
            var mostOverlap = float.MinValue;
            foreach (CollisionNode<T> current in collisionProvider)
            {
                ref LineSegment surface = ref current.Surface;
                Vector2 intersectionPoint = movedBound.GetIntersectionPoint(ref surface);
                if (intersectionPoint == Vector2.Zero) continue;

                // Project rectangle surfaces on intersecting surfaces and find the intersecting surface with the most overlap.
                // Since rectangles have one shorter side, the distance based weight doesn't work.
                Vector2 lineVector = Vector2.Normalize(surface.Start - surface.End);
                float surfaceOverlapS = Vector2.Dot(lineVector, surface.Start);
                float surfaceOverlapE = Vector2.Dot(lineVector, surface.End);

                float maxSurface = MathF.Max(surfaceOverlapS, surfaceOverlapE);
                float minSurface = MathF.Min(surfaceOverlapS, surfaceOverlapE);

                var max = float.MinValue;
                var min = float.MaxValue;
                for (var i = 0; i < boundSurfaces.Length; i++)
                {
                    float overlapS = Vector2.Dot(lineVector, boundSurfaces[i].Start);
                    float overlapE = Vector2.Dot(lineVector, boundSurfaces[i].End);
                    max = MathF.Max(overlapS, max);
                    max = MathF.Max(overlapE, max);

                    min = MathF.Min(overlapS, min);
                    min = MathF.Min(overlapE, min);
                }

                float overlap = Maths.Get1DIntersectionDepth(min, max, minSurface, maxSurface);

#if DEBUG
                bool? leftSideNormal = surface.IsPointLeftOf(movedBound.Center);
                if (leftSideNormal != null)
                {
                    Vector2 normal = surface.GetNormal(!leftSideNormal.Value);
                    LastCollision.Add(new CollisionDebugData
                    {
                        Line = surface,
                        Weight = overlap,
                        LineNormal = normal,
                        CollisionType = "Overlap-Weighted Intersection"
                    });
                }
#endif

                if (overlap <= mostOverlap) continue;
                mostOverlap = overlap;
                closestNode = new CollisionNode<T> { Surface = surface, Entity = current.Entity };
            }

            return closestNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CollisionNode<T> GetIntersectionCollision<T>(IEnumerable<CollisionNode<T>> collisionProvider, IShape movedBound, Vector2 movementVector)
        {
#if DEBUG
            LastCollision.Add(null);
#endif

            CollisionNode<T> closestNode = null;
            var shortestDist = float.MaxValue;
            foreach (CollisionNode<T> current in collisionProvider)
            {
                ref LineSegment surface = ref current.Surface;
                Vector2 intersectionPoint = movedBound.GetIntersectionPoint(ref surface);
                if (intersectionPoint == Vector2.Zero) continue;
                float intersectionDist = Vector2.Distance(intersectionPoint, movedBound.Center);

#if DEBUG
                bool? leftSideNormal = surface.IsPointLeftOf(movedBound.Center);
                if (leftSideNormal != null)
                {
                    Vector2 normal = surface.GetNormal(!leftSideNormal.Value);
                    LastCollision.Add(new CollisionDebugData
                    {
                        Line = surface,
                        Weight = -intersectionDist,
                        LineNormal = normal,
                        CollisionType = $"Intersection {(intersectionDist == shortestDist ? "Stalemate" : "")}"
                    });
                }
#endif

                if (intersectionDist > shortestDist) continue;

                if (intersectionDist == shortestDist && closestNode != null)
                {
                    // If the two surfaces are at the same distance.
                    // This usually happens on corners, so go along the line and check distance.
                    float lengthBack = movementVector.Length();
                    ref LineSegment oldCloserSurface = ref closestNode.Surface;
                    Vector2 oldCloserIntersectionPoint = movedBound.GetIntersectionPoint(ref surface);
                    Vector2 oldCloserAlongLine = GoBackOnSegment(ref oldCloserSurface, ref oldCloserIntersectionPoint, lengthBack);
                    float oldCloserDistAlongLine = Vector2.Distance(oldCloserAlongLine, movedBound.Center);

                    Vector2 currentAlongLine = GoBackOnSegment(ref surface, ref intersectionPoint, lengthBack);
                    float currentDistAlongLine = Vector2.Distance(currentAlongLine, movedBound.Center);

                    if (currentDistAlongLine >= oldCloserDistAlongLine) continue;
#if DEBUG
                    LastCollision[^1].CollisionType += " WON";
#endif
                }

                shortestDist = intersectionDist;
                closestNode = new CollisionNode<T> { Surface = surface, Entity = current.Entity };
            }

            return closestNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 GoBackOnSegment(ref LineSegment segment, ref Vector2 point, float lengthBack)
        {
            if (point == segment.Start) return segment.PointOnLineAtDistance(lengthBack);
            return point == segment.End ? segment.PointOnLineAtDistance(segment.Length() - lengthBack) : point;
        }
    }
}