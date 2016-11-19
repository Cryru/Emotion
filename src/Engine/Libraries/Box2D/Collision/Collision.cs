﻿/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Box2D.XNA
{
    internal enum ContactFeatureType : byte
    {
        Vertex = 0,
        Face = 1,
    };

    /// The features that intersect to form the contact point
    /// This must be 4 bytes or less.
    public struct ContactFeature
    {
        internal byte indexA;		///< Feature index on ShapeA
        internal byte indexB;		///< Feature index on ShapeB
        internal byte typeA;		    ///< The feature type on ShapeA
        internal byte typeB;		    ///< The feature type on ShapeB
    };

    /// Contact ids to facilitate warm starting.
    [StructLayout(LayoutKind.Explicit)]
    public struct ContactID
    {
        [FieldOffset(0)]
        public ContactFeature Features;

	    /// The features that intersect to form the contact point
        [FieldOffset(0)]
        public uint Key;					///< Used to quickly compare contact ids.
    };

    /// A manifold point is a contact point belonging to a contact
    /// manifold. It holds details related to the geometry and dynamics
    /// of the contact points.
    /// The local point usage depends on the manifold type:
    /// -ShapeType.Circles: the local center of circleB
    /// -SeparationFunction.FaceA: the local center of cirlceB or the clip point of polygonB
    /// -SeparationFunction.FaceB: the clip point of polygonA
    /// This structure is stored across time steps, so we keep it small.
    /// Note: the impulses are used for internal caching and may not
    /// provide reliable contact forces, especially for high speed collisions.
    public struct ManifoldPoint
    {
        public Vector2 LocalPoint;		///< usage depends on manifold type
        public float NormalImpulse;	///< the non-penetration impulse
        public float TangentImpulse;	///< the friction impulse
        public ContactID Id;			///< uniquely identifies a contact point between two Shapes
    };

    public enum ManifoldType
    {
	    Circles,
	    FaceA,
	    FaceB
    };

    
    public enum EdgeType
    {
        Isolated,
        Concave,
        Flat,
        Convex
    };

    /// A manifold for two touching convex Shapes.
    /// Box2D supports multiple types of contact:
    /// - clip point versus plane with radius
    /// - point versus point with radius (circles)
    /// The local point usage depends on the manifold type:
    /// -ShapeType.Circles: the local center of circleA
    /// -SeparationFunction.FaceA: the center of faceA
    /// -SeparationFunction.FaceB: the center of faceB
    /// Similarly the local normal usage:
    /// -ShapeType.Circles: not used
    /// -SeparationFunction.FaceA: the normal on polygonA
    /// -SeparationFunction.FaceB: the normal on polygonB
    /// We store contacts in this way so that position correction can
    /// account for movement, which is critical for continuous physics.
    /// All contact scenarios must be expressed in one of these types.
    /// This structure is stored across time steps, so we keep it small.
    public struct Manifold
    {
	    public FixedArray2<ManifoldPoint> _points;	        ///< the points of contact
	    public Vector2 _localNormal;						///< not use for Type.SeparationFunction.Points
	    public Vector2 _localPoint;							///< usage depends on manifold type
	    public ManifoldType _type;
	    public int _pointCount;								///< the number of manifold points
    };

    /// This is used to compute the current state of a contact manifold.
    public struct WorldManifold
    {
	    /// Evaluate the manifold with supplied transforms. This assumes
	    /// modest motion from the original state. This does not change the
	    /// point count, impulses, etc. The radii must come from the Shapes
	    /// that generated the manifold.
        public WorldManifold(ref Manifold manifold,
					    ref Transform xfA, float radiusA,
					    ref Transform xfB, float radiusB)
        {
            _points = new FixedArray2<Vector2>();

	        if (manifold._pointCount == 0)
	        {
                _normal = Vector2.UnitY;
		        return;
	        }

	        switch (manifold._type)
	        {
	        case ManifoldType.Circles:
		        {
			        Vector2 pointA = MathUtils.Multiply(ref xfA, manifold._localPoint);
                    Vector2 pointB = MathUtils.Multiply(ref xfB, manifold._points[0].LocalPoint);
                    _normal = new Vector2(1.0f, 0.0f);
			        if (Vector2.DistanceSquared(pointA, pointB) > Settings.b2_epsilon * Settings.b2_epsilon)
			        {
                        _normal = pointB - pointA;
                        _normal.Normalize();
			        }

                    Vector2 cA = pointA + radiusA * _normal;
                    Vector2 cB = pointB - radiusB * _normal;
			        _points[0] = 0.5f * (cA + cB);
		        }
		        break;

	        case ManifoldType.FaceA:
		        {
                    _normal = MathUtils.Multiply(ref xfA.R, manifold._localNormal);
			        Vector2 planePoint = MathUtils.Multiply(ref xfA, manifold._localPoint);

			        for (int i = 0; i < manifold._pointCount; ++i)
			        {
				        Vector2 clipPoint = MathUtils.Multiply(ref xfB, manifold._points[i].LocalPoint);
                        Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, _normal)) * _normal;
                        Vector2 cB = clipPoint - radiusB * _normal;
				        _points[i] = 0.5f * (cA + cB);
			        }
		        }
		        break;

	        case ManifoldType.FaceB:
		        {
                    _normal = MathUtils.Multiply(ref xfB.R, manifold._localNormal);
			        Vector2 planePoint = MathUtils.Multiply(ref xfB, manifold._localPoint);

			        for (int i = 0; i < manifold._pointCount; ++i)
			        {
                        Vector2 clipPoint = MathUtils.Multiply(ref xfA, manifold._points[i].LocalPoint);
                        Vector2 cA = clipPoint - radiusA * _normal;
                        Vector2 cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, _normal)) * _normal;
				        _points[i] = 0.5f * (cA + cB);
			        }
                    // Ensure normal points from A to B.
                    _normal *= -1; 
		        }
		        break;
            default:
                _normal = Vector2.UnitY;
                break;
	        }
        }

	    public Vector2 _normal;						///< world vector pointing from A to B
	    public FixedArray2<Vector2> _points;   	///< world contact point (point of intersection)
    };

    /// This is used for determining the state of contact points.
    public enum PointState
    {
	    Null,		///< point does not exist
	    Add,		///< point was added in the update
	    Persist,	///< point persisted across the update
	    Remove,		///< point was removed in the update
    };

    /// Used for computing contact manifolds.
    public struct ClipVertex
    {
	    public Vector2 v;
	    public ContactID id;
    };

    /// Ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).
    public struct RayCastInput
    {
	    public Vector2 p1, p2;
	    public float maxFraction;
    };

    /// Ray-cast output data.  The ray hits at p1 + fraction * (p2 - p1), where p1 and p2
    /// come from RayCastInput. 
    public struct RayCastOutput
    {
	    public Vector2 normal;
        public float fraction;
    };

    /// An axis aligned bounding box.
    public struct AABB
    {
	    /// Verify that the bounds are sorted.
	    public bool IsValid()
        {
	        Vector2 d = upperBound - lowerBound;
	        bool valid = d.X >= 0.0f && d.Y >= 0.0f;
	        valid = valid && lowerBound.IsValid() && upperBound.IsValid();
	        return valid;
        }

	    /// Get the center of the AABB.
	    public Vector2 GetCenter()
	    {
		    return 0.5f * (lowerBound + upperBound);
	    }

	    /// Get the extents of the AABB (half-widths).
	    public Vector2 GetExtents()
	    {
		    return 0.5f * (upperBound - lowerBound);
	    }

        /// Get the perimeter length
        public float GetPerimeter()
	    {
		    float wx = upperBound.X - lowerBound.X;
		    float wy = upperBound.Y - lowerBound.Y;
		    return 2.0f * (wx + wy);
	    }

	    /// Combine an AABB into this one.
        public void Combine(ref AABB aabb)
	    {
		    lowerBound = Vector2.Min(lowerBound, aabb.lowerBound);
		    upperBound = Vector2.Max(upperBound, aabb.upperBound);
	    }


	    /// Combine two AABBs into this one.
	    public void Combine(ref AABB aabb1, ref AABB aabb2)
	    {
		    lowerBound = Vector2.Min(aabb1.lowerBound, aabb2.lowerBound);
		    upperBound = Vector2.Max(aabb1.upperBound, aabb2.upperBound);
	    }

	    /// Does this aabb contain the provided AABB.
	    public bool Contains(ref AABB aabb)
	    {
		    bool result = true;
		    result = result && lowerBound.X <= aabb.lowerBound.X;
		    result = result && lowerBound.Y <= aabb.lowerBound.Y;
		    result = result && aabb.upperBound.X <= upperBound.X;
		    result = result && aabb.upperBound.Y <= upperBound.Y;
		    return result;
	    }

        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            Vector2 d1, d2;
            d1 = b.lowerBound - a.upperBound;
            d2 = a.lowerBound - b.upperBound;

            if (d1.X > 0.0f || d1.Y > 0.0f)
	            return false;

            if (d2.X > 0.0f || d2.Y > 0.0f)
	            return false;

            return true;
        }

        public static bool TestOverlap( Shape ShapeA, int indexA, 
                                        Shape ShapeB, int indexB, 
                                        ref Transform xfA, ref Transform xfB)
        {
            DistanceInput input = new DistanceInput();
	        input.proxyA.Set(ShapeA, indexA);
	        input.proxyB.Set(ShapeB, indexB);
	        input.transformA = xfA;
	        input.transformB = xfB;
	        input.useRadii = true;

            SimplexCache cache;
	        DistanceOutput output;
	        Distance.ComputeDistance(out output, out cache, ref input);

	        return output.distance < 10.0f * Settings.b2_epsilon;
        }


        // From Real-time Collision Detection, p179.
	    public bool RayCast(out RayCastOutput output, ref RayCastInput input)
        {
            output = new RayCastOutput();

            float tmin = -Settings.b2_maxFloat;
            float tmax = Settings.b2_maxFloat;

	        Vector2 p = input.p1;
	        Vector2 d = input.p2 - input.p1;
	        Vector2 absD = MathUtils.Abs(d);

            Vector2 normal = Vector2.Zero;

	        for (int i = 0; i < 2; ++i)
	        {
                float absD_i = i == 0 ? absD.X : absD.Y;
                float lowerBound_i = i == 0 ? lowerBound.X : lowerBound.Y;
                float upperBound_i = i == 0 ? upperBound.X : upperBound.Y;
                float p_i = i == 0 ? p.X : p.Y;

                if (absD_i < Settings.b2_epsilon)
		        {
			        // Parallel.
                    if (p_i < lowerBound_i || upperBound_i < p_i)
			        {
				        return false;
			        }
		        }
		        else
		        {
                    float d_i = i == 0 ? d.X : d.Y;

                    float inv_d = 1.0f / d_i;
                    float t1 = (lowerBound_i - p_i) * inv_d;
                    float t2 = (upperBound_i - p_i) * inv_d;

			        // Sign of the normal vector.
			        float s = -1.0f;

			        if (t1 > t2)
			        {
				        MathUtils.Swap<float>(ref t1, ref t2);
				        s = 1.0f;
			        }

			        // Push the min up
			        if (t1 > tmin)
			        {
                        if (i == 0)
                        {
                            normal.X = s;
                        }
                        else
                        {
                            normal.Y = s;
                        }

				        tmin = t1;
			        }

			        // Pull the max down
			        tmax = Math.Min(tmax, t2);

			        if (tmin > tmax)
			        {
				        return false;
			        }
		        }
	        }

	        // Does the ray start inside the box?
	        // Does the ray intersect beyond the max fraction?
	        if (tmin < 0.0f || input.maxFraction < tmin)
	        {
		        return false;
	        }

	        // Intersection.
	        output.fraction = tmin;
	        output.normal = normal;
            return true;
        }

	    public Vector2 lowerBound;	///< the lower vertex
	    public Vector2 upperBound;	///< the upper vertex
    };

    public static class Collision
    {
        public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2,
					          ref Manifold manifold1, ref Manifold manifold2)
        {
            state1 = new FixedArray2<PointState>();
            state2 = new FixedArray2<PointState>();

	        // Detect persists and removes.
	        for (int i = 0; i < manifold1._pointCount; ++i)
	        {
		        ContactID id = manifold1._points[i].Id;

                state1[i] = PointState.Remove;

		        for (int j = 0; j < manifold2._pointCount; ++j)
		        {
			        if (manifold2._points[j].Id.Key == id.Key)
			        {
                        state1[i] = PointState.Persist;
				        break;
			        }
		        }
	        }

	        // Detect persists and adds.
	        for (int i = 0; i < manifold2._pointCount; ++i)
	        {
		        ContactID id = manifold2._points[i].Id;

                state2[i] = PointState.Add;

		        for (int j = 0; j < manifold1._pointCount; ++j)
		        {
			        if (manifold1._points[j].Id.Key == id.Key)
			        {
                        state2[i] = PointState.Persist;
				        break;
			        }
		        }
	        }
        }


        /// Compute the collision manifold between two circles.
        public static void CollideCircles(ref Manifold manifold,
					          CircleShape circleA, ref Transform xfA,
					          CircleShape circleB, ref Transform xfB)
        {
	        manifold._pointCount = 0;

	        Vector2 pA = MathUtils.Multiply(ref xfA, circleA._p);
	        Vector2 pB = MathUtils.Multiply(ref xfB, circleB._p);

	        Vector2 d = pB - pA;
	        float distSqr = Vector2.Dot(d, d);
            float rA = circleA._radius;
            float rB = circleB._radius;
            float radius = rA + rB;
	        if (distSqr > radius * radius)
	        {
		        return;
	        }

	        manifold._type = ManifoldType.Circles;
	        manifold._localPoint = circleA._p;
	        manifold._localNormal = Vector2.Zero;
	        manifold._pointCount = 1;

            var p0 = manifold._points[0];

            p0.LocalPoint = circleB._p;
            p0.Id.Key = 0;

            manifold._points[0] = p0;
        }

        /// Compute the collision manifold between a polygon and a circle.
        public static void CollidePolygonAndCircle(ref Manifold manifold,
							           PolygonShape polygonA, ref Transform xfA,
							           CircleShape circleB, ref Transform xfB)
        {
	        manifold._pointCount = 0;

	        // Compute circle position in the frame of the polygon.
	        Vector2 c = MathUtils.Multiply(ref xfB, circleB._p);
	        Vector2 cLocal = MathUtils.MultiplyT(ref xfA, c);

	        // Find the min separating edge.
	        int normalIndex = 0;
	        float separation = -Settings.b2_maxFloat;
	        float radius = polygonA._radius + circleB._radius;
	        int vertexCount = polygonA._vertexCount;

	        for (int i = 0; i < vertexCount; ++i)
	        {
                float s = Vector2.Dot(polygonA._normals[i], cLocal - polygonA._vertices[i]);

		        if (s > radius)
		        {
			        // Early out.
			        return;
		        }

		        if (s > separation)
		        {
			        separation = s;
			        normalIndex = i;
		        }
	        }

	        // Vertices that subtend the incident face.
	        int vertIndex1 = normalIndex;
	        int vertIndex2 = vertIndex1 + 1 < vertexCount ? vertIndex1 + 1 : 0;
            Vector2 v1 = polygonA._vertices[vertIndex1];
            Vector2 v2 = polygonA._vertices[vertIndex2];

	        // If the center is inside the polygon ...
	        if (separation < Settings.b2_epsilon)
	        {
		        manifold._pointCount = 1;
		        manifold._type = ManifoldType.FaceA;
                manifold._localNormal = polygonA._normals[normalIndex];
		        manifold._localPoint = 0.5f * (v1 + v2);

                var p0 = manifold._points[0];

                p0.LocalPoint = circleB._p;
                p0.Id.Key = 0;

                manifold._points[0] = p0;

		        return;
	        }

	        // Compute barycentric coordinates
	        float u1 = Vector2.Dot(cLocal - v1, v2 - v1);
	        float u2 = Vector2.Dot(cLocal - v2, v1 - v2);
	        if (u1 <= 0.0f)
	        {
		        if (Vector2.DistanceSquared(cLocal, v1) > radius * radius)
		        {
			        return;
		        }

		        manifold._pointCount = 1;
		        manifold._type = ManifoldType.FaceA;
		        manifold._localNormal = cLocal - v1;
		        manifold._localNormal.Normalize();
		        manifold._localPoint = v1;

                var p0b = manifold._points[0];

                p0b.LocalPoint = circleB._p;
                p0b.Id.Key = 0;

                manifold._points[0] = p0b;

	        }
	        else if (u2 <= 0.0f)
	        {
		        if (Vector2.DistanceSquared(cLocal, v2) > radius * radius)
		        {
			        return;
		        }

		        manifold._pointCount = 1;
		        manifold._type = ManifoldType.FaceA;
		        manifold._localNormal = cLocal - v2;
		        manifold._localNormal.Normalize();
		        manifold._localPoint = v2;

                var p0c = manifold._points[0];

                p0c.LocalPoint = circleB._p;
                p0c.Id.Key = 0;

                manifold._points[0] = p0c;
	        }
	        else
	        {
		        Vector2 faceCenter = 0.5f * (v1 + v2);
                float separation2 = Vector2.Dot(cLocal - faceCenter, polygonA._normals[vertIndex1]);
		        if (separation2 > radius)
		        {
			        return;
		        }

		        manifold._pointCount = 1;
		        manifold._type = ManifoldType.FaceA;
                manifold._localNormal = polygonA._normals[vertIndex1];
		        manifold._localPoint = faceCenter;

                var p0d = manifold._points[0];

                p0d.LocalPoint = circleB._p;
                p0d.Id.Key = 0;

                manifold._points[0] = p0d;
	        }
        }

        /// Compute the collision manifold between two polygons.
        public static void CollidePolygons(ref Manifold manifold,
                               PolygonShape polyA, ref Transform xfA,
                               PolygonShape polyB, ref Transform xfB)
        {
	        manifold._pointCount = 0;
	        float totalRadius = polyA._radius + polyB._radius;

	        int edgeA = 0;
	        float separationA = FindMaxSeparation(out edgeA, polyA, ref xfA, polyB, ref xfB);
	        if (separationA > totalRadius)
		        return;

	        int edgeB = 0;
	        float separationB = FindMaxSeparation(out edgeB, polyB, ref xfB, polyA, ref xfA);
	        if (separationB > totalRadius)
		        return;

	        PolygonShape poly1;	// reference polygon
	        PolygonShape poly2;	// incident polygon
	        Transform xf1, xf2;
	        int edge1;		// reference edge
	        bool flip;
	        float k_relativeTol = 0.98f;
	        float k_absoluteTol = 0.001f;

	        if (separationB > k_relativeTol * separationA + k_absoluteTol)
	        {
		        poly1 = polyB;
		        poly2 = polyA;
		        xf1 = xfB;
		        xf2 = xfA;
		        edge1 = edgeB;
		        manifold._type = ManifoldType.FaceB;
		        flip = true;
	        }
	        else
	        {
		        poly1 = polyA;
		        poly2 = polyB;
		        xf1 = xfA;
		        xf2 = xfB;
		        edge1 = edgeA;
		        manifold._type = ManifoldType.FaceA;
		        flip = false;
	        }

	        FixedArray2<ClipVertex> incidentEdge;
	        FindIncidentEdge(out incidentEdge, poly1, ref xf1, edge1, poly2, ref xf2);

	        int count1 = poly1._vertexCount;

            int iv1 = edge1;
            int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

            Vector2 v11 = poly1._vertices[iv1];
            Vector2 v12 = poly1._vertices[iv2];

	        Vector2 localTangent = v12 - v11;
            localTangent.Normalize();

            Vector2 localNormal = MathUtils.Cross(localTangent, 1.0f);
	        Vector2 planePoint = 0.5f * (v11 + v12);

            Vector2 tangent = MathUtils.Multiply(ref xf1.R, localTangent);
	        Vector2 normal = MathUtils.Cross(tangent, 1.0f);
        	
	        v11 = MathUtils.Multiply(ref xf1, v11);
	        v12 = MathUtils.Multiply(ref xf1, v12);

            // Face offset.
	        float frontOffset = Vector2.Dot(normal, v11);

            // Side offsets, extended by polytope skin thickness.
            float sideOffset1 = -Vector2.Dot(tangent, v11) + totalRadius;
            float sideOffset2 = Vector2.Dot(tangent, v12) + totalRadius;

	        // Clip incident edge against extruded edge1 side edges.
	        FixedArray2<ClipVertex> clipPoints1;
	        FixedArray2<ClipVertex> clipPoints2;
	        int np;

	        // Clip to box side 1
            np = ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1, iv1);

	        if (np < 2)
		        return;

	        // Clip to negative box side 1
            np = ClipSegmentToLine(out clipPoints2, ref clipPoints1, tangent, sideOffset2, iv2);

	        if (np < 2)
	        {
		        return;
	        }

	        // Now clipPoints2 contains the clipped points.
	        manifold._localNormal = localNormal;
	        manifold._localPoint = planePoint;

	        int pointCount = 0;
	        for (int i = 0; i < Settings.b2_maxManifoldPoints; ++i)
	        {
		        float separation = Vector2.Dot(normal, clipPoints2[i].v) - frontOffset;

		        if (separation <= totalRadius)
		        {
			        ManifoldPoint cp = manifold._points[pointCount];
			        cp.LocalPoint = MathUtils.MultiplyT(ref xf2, clipPoints2[i].v);
			        cp.Id = clipPoints2[i].id;

                    if (flip)
                    {
                        // Swap features
                        ContactFeature cf = cp.Id.Features;
                        cp.Id.Features.indexA = cf.indexB;
                        cp.Id.Features.indexB = cf.indexA;
                        cp.Id.Features.typeA = cf.typeB;
                        cp.Id.Features.typeB = cf.typeA;
                    }

                    manifold._points[pointCount] = cp;

			        ++pointCount;
		        }
	        }

	        manifold._pointCount = pointCount;
        }

        // Compute contact points for edge versus circle.
        // This accounts for edge connectivity.
        public static void CollideEdgeAndCircle(ref Manifold manifold,
					           EdgeShape edgeA, ref Transform xfA,
					           CircleShape circleB, ref Transform xfB)
        {
	        manifold._pointCount = 0;

	        // Compute circle in frame of edge
	        Vector2 Q = MathUtils.MultiplyT(ref xfA, MathUtils.Multiply(ref xfB, circleB._p));

	        Vector2 A = edgeA._vertex1, B = edgeA._vertex2;
	        Vector2 e = B - A;

	        // Barycentric coordinates
	        float u = Vector2.Dot(e, B - Q);
	        float v = Vector2.Dot(e, Q - A);

	        float radius = edgeA._radius + circleB._radius;

	        ContactFeature cf;
	        cf.indexB = 0;
	        cf.typeB = (byte)ContactFeatureType.Vertex;
            
            Vector2 P, d;

	        // Region A
	        if (v <= 0.0f)
	        {
		        P = A;
		        d = Q - P;
		        float dd = Vector2.Dot(d, d);
		        if (dd > radius * radius)
		        {
			        return;
		        }

		        // Is there an edge connected to A?
		        if (edgeA._hasVertex0)
		        {
			        Vector2 A1 = edgeA._vertex0;
			        Vector2 B1 = A;
			        Vector2 e1 = B1 - A1;
			        float u1 = Vector2.Dot(e1, B1 - Q);

			        // Is the circle in Region AB of the previous edge?
			        if (u1 > 0.0f)
			        {
				        return;
			        }
		        }

		        cf.indexA = 0;
		        cf.typeA = (byte)ContactFeatureType.Vertex;
		        manifold._pointCount = 1;
		        manifold._type = ManifoldType.Circles;
		        manifold._localNormal = Vector2.Zero;
		        manifold._localPoint = P;
                var mp = new ManifoldPoint();
		        mp.Id.Key = 0;
		        mp.Id.Features = cf;
		        mp.LocalPoint = circleB._p;
                manifold._points[0] = mp;
		        return;
	        }

	        // Region B
	        if (u <= 0.0f)
	        {
		        P = B;
		        d = Q - P;
		        float dd = Vector2.Dot(d, d);
		        if (dd > radius * radius)
		        {
			        return;
		        }

		        // Is there an edge connected to B?
		        if (edgeA._hasVertex3)
		        {
			        Vector2 B2 = edgeA._vertex3;
			        Vector2 A2 = B;
			        Vector2 e2 = B2 - A2;
			        float v2 = Vector2.Dot(e2, Q - A2);

			        // Is the circle in Region AB of the next edge?
			        if (v2 > 0.0f)
			        {
				        return;
			        }
		        }

		        cf.indexA = 1;
		        cf.typeA = (byte)ContactFeatureType.Vertex;
		        manifold._pointCount = 1;
		        manifold._type = ManifoldType.Circles;
		        manifold._localNormal = Vector2.Zero;
		        manifold._localPoint = P;
	            var mp = new ManifoldPoint();
		        mp.Id.Key = 0;
		        mp.Id.Features = cf;
		        mp.LocalPoint = circleB._p;
                manifold._points[0] = mp;
		        return;
	        }

	        // Region AB
	        float den = Vector2.Dot(e, e);
	        Debug.Assert(den > 0.0f);
	        P = (1.0f / den) * (u * A + v * B);
	        d = Q - P;
	        float dd2 = Vector2.Dot(d, d);
	        if (dd2 > radius * radius)
	        {
		        return;
	        }

	        Vector2 n = new Vector2(-e.Y, e.X);
	        if (Vector2.Dot(n, Q - A) < 0.0f)
	        {
		        n = new Vector2(-n.X, -n.Y);
	        }
	        n.Normalize();

	        cf.indexA = 0;
	        cf.typeA = (byte)ContactFeatureType.Face;
	        manifold._pointCount = 1;
	        manifold._type = ManifoldType.FaceA;
	        manifold._localNormal = n;
	        manifold._localPoint = A;
            var mp2 = new ManifoldPoint();
	        mp2.Id.Key = 0;
	        mp2.Id.Features = cf;
            mp2.LocalPoint = circleB._p;
            manifold._points[0] = mp2;
        }

    	public enum EPAxisType
        {
	        Unknown,
	        EdgeA,
	        EdgeB,
        };

        public struct EPAxis
        {
	        public EPAxisType type;
	        public int index;
	        public float separation;
        };

        static EPAxis ComputeEdgeSeperation(Vector2 v1, Vector2 v2, Vector2 n, PolygonShape polygonB, float radius)
        {
	        // EdgeA separation
	        EPAxis axis;
	        axis.type = EPAxisType.EdgeA;
	        axis.index = 0;
	        axis.separation = Vector2.Dot(n, polygonB._vertices[0] - v1);
	        for (int i = 1; i < polygonB._vertexCount; ++i)
	        {
		        float s = Vector2.Dot(n, polygonB._vertices[i] - v1);
		        if (s < axis.separation)
		        {
			        axis.separation = s;
		        }
	        }

	        return axis;
        }

        static EPAxis ComputePolygonSeperation(Vector2 v1, Vector2 v2, Vector2 n, PolygonShape polygonB, float radius)
        {
	        // PolygonB separation
	        EPAxis axis;
	        axis.type = EPAxisType.EdgeB;
	        axis.index = 0;
	        axis.separation = float.MinValue;
	        for (int i = 0; i < polygonB._vertexCount; ++i)
	        {
		        float s1 = Vector2.Dot(polygonB._normals[i], v1 - polygonB._vertices[i]);	
		        float s2 = Vector2.Dot(polygonB._normals[i], v2 - polygonB._vertices[i]);
		        float s = Math.Min(s1, s2);
		        if (s > axis.separation)
		        {
			        axis.index = i;
			        axis.separation = s;
			        if (s > radius)
			        {
				        return axis;
			        }
		        }
	        }

	        return axis;
        }

        static void FindIncidentEdge(ref FixedArray2<ClipVertex> c, PolygonShape poly1, int edge1, PolygonShape poly2)
        {
	        int count1 = poly1._vertexCount;
	        int count2 = poly2._vertexCount;

	        Debug.Assert(0 <= edge1 && edge1 < count1);

	        // Get the normal of the reference edge in poly2's frame.
            Vector2 normal1 = poly1._normals[edge1];

	        // Find the incident edge on poly2.
	        int index = 0;
	        float minDot = float.MaxValue;
	        for (int i = 0; i < count2; ++i)
	        {
                float dot = Vector2.Dot(normal1, poly2._normals[i]);
		        if (dot < minDot)
		        {
			        minDot = dot;
			        index = i;
		        }
	        }

	        // Build the clip vertices for the incident edge.
	        int i1 = index;
	        int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            ClipVertex ctemp = new ClipVertex();
            ctemp.v = poly2._vertices[i1];
            ctemp.id.Features.indexA = (byte)edge1;
            ctemp.id.Features.indexB = (byte)i1;
            ctemp.id.Features.typeA = (byte)ContactFeatureType.Face;
            ctemp.id.Features.typeB = (byte)ContactFeatureType.Vertex;
            c[0] = ctemp;

            ctemp.v = poly2._vertices[i2];
            ctemp.id.Features.indexA = (byte)edge1;
            ctemp.id.Features.indexB = (byte)i2;
            ctemp.id.Features.typeA = (byte)ContactFeatureType.Face;
            ctemp.id.Features.typeB = (byte)ContactFeatureType.Vertex;
            c[1] = ctemp;
        }

        // Collide and edge and polygon. This uses the SAT and clipping to produce up to 2 contact points.
        // Edge adjacency is handle to produce locally valid contact points and normals. This is intended
        // to allow the polygon to slide smoothly over an edge chain.
        //
        // Algorithm
        // 1. Classify front-side or back-side collision with edge.
        // 2. Compute separation
        // 3. Process adjacent edges
        // 4. Classify adjacent edge as convex, flat, null, or concave
        // 5. Skip null or concave edges. Concave edges get a separate manifold.
        // 6. If the edge is flat, compute contact points as normal. Discard boundary points.
        // 7. If the edge is convex, compute it's separation.
        // 8. Use the minimum separation of up to three edges. If the minimum separation
        //    is not the primary edge, return.
        // 9. If the minimum separation is the primary edge, compute the contact points and return.

        static PolygonShape s_polygonA = new PolygonShape();
        static PolygonShape s_polygonB = new PolygonShape();
        public static void CollideEdgeAndPolygon(ref Manifold manifold,
					           EdgeShape edgeA, ref Transform xfA,
					           PolygonShape polygonB_in, ref Transform xfB)
        {
	        manifold._pointCount = 0;

            Transform xf;
            MathUtils.MultiplyT(ref xfA, ref xfB, out xf);

	        // Create a polygon for edge shape A
            s_polygonA.SetAsEdge(edgeA._vertex1, edgeA._vertex2);

	        // Build polygonB in frame A
            s_polygonB._radius = polygonB_in._radius;
            s_polygonB._vertexCount = polygonB_in._vertexCount;
            s_polygonB._centroid = MathUtils.Multiply(ref xf, polygonB_in._centroid);
            for (int i = 0; i < s_polygonB._vertexCount; ++i)
	        {
                s_polygonB._vertices[i] = MathUtils.Multiply(ref xf, polygonB_in._vertices[i]);
                s_polygonB._normals[i] = MathUtils.Multiply(ref xf.R, polygonB_in._normals[i]);
	        }

            float totalRadius = s_polygonA._radius + s_polygonB._radius;

	        // Edge geometry
	        Vector2 v1 = edgeA._vertex1;
	        Vector2 v2 = edgeA._vertex2;
	        Vector2 e = v2 - v1;
	        Vector2 edgeNormal = new Vector2(e.Y, -e.X);
	        edgeNormal.Normalize();

	        // Determine side
            bool isFrontSide = Vector2.Dot(edgeNormal, s_polygonB._centroid - v1) >= 0.0f;
	        if (isFrontSide == false)
	        {
		        edgeNormal = -edgeNormal;
	        }

	        // Compute primary separating axis
            EPAxis edgeAxis = ComputeEdgeSeperation(v1, v2, edgeNormal, s_polygonB, totalRadius);
	        if (edgeAxis.separation > totalRadius)
	        {
		        // Shapes are separated
		        return;
	        }

	        // Classify adjacent edges
            FixedArray2<EdgeType> types = new FixedArray2<EdgeType>();
            //types[0] = EdgeType.Isolated;
            //types[1] = EdgeType.Isolated;
	        if (edgeA._hasVertex0)
	        {
		        Vector2 v0 = edgeA._vertex0;
		        float s = Vector2.Dot(edgeNormal, v0 - v1);

		        if (s > 0.1f * Settings.b2_linearSlop)
		        {
			        types[0] = EdgeType.Concave;
		        }
		        else if (s >= -0.1f * Settings.b2_linearSlop)
		        {
			        types[0] = EdgeType.Flat;
		        }
		        else
		        {
			        types[0] = EdgeType.Convex;
		        }
	        }

	        if (edgeA._hasVertex3)
	        {
		        Vector2 v3 = edgeA._vertex3;
		        float s = Vector2.Dot(edgeNormal, v3 - v2);
		        if (s > 0.1f * Settings.b2_linearSlop)
		        {
			        types[1] = EdgeType.Concave;
		        }
		        else if (s >= -0.1f * Settings.b2_linearSlop)
		        {
			        types[1] = EdgeType.Flat;
		        }
		        else
		        {
			        types[1] = EdgeType.Convex;
		        }
	        }

	        if (types[0] == EdgeType.Convex)
	        {
		        // Check separation on previous edge.
		        Vector2 v0 = edgeA._vertex0;
		        Vector2 e0 = v1 - v0;

		        Vector2 n0 = new Vector2(e0.Y, -e0.X);
		        n0.Normalize();
		        if (isFrontSide == false)
		        {
			        n0 = -n0;
		        }

                EPAxis axis1 = ComputeEdgeSeperation(v0, v1, n0, s_polygonB, totalRadius);
		        if (axis1.separation > edgeAxis.separation)
		        {
			        // The polygon should collide with previous edge
			        return;
		        }
	        }

	        if (types[1] == EdgeType.Convex)
	        {
		        // Check separation on next edge.
		        Vector2 v3 = edgeA._vertex3;
		        Vector2 e2 = v3 - v2;

		        Vector2 n2 = new Vector2(e2.Y, -e2.X);
		        n2.Normalize();
		        if (isFrontSide == false)
		        {
			        n2 = -n2;
		        }

                EPAxis axis2 = ComputeEdgeSeperation(v2, v3, n2, s_polygonB, totalRadius);
		        if (axis2.separation > edgeAxis.separation)
		        {
			        // The polygon should collide with the next edge
			        return;
		        }
	        }

            EPAxis polygonAxis = ComputePolygonSeperation(v1, v2, edgeNormal, s_polygonB, totalRadius);
	        if (polygonAxis.separation > totalRadius)
	        {
		        return;
	        }

	        // Use hysteresis for jitter reduction.
	        float k_relativeTol = 0.98f;
	        float k_absoluteTol = 0.001f;

	        EPAxis primaryAxis;
	        if (polygonAxis.separation > k_relativeTol * edgeAxis.separation + k_absoluteTol)
	        {
		        primaryAxis = polygonAxis;
	        }
	        else
	        {
		        primaryAxis = edgeAxis;
	        }

	        PolygonShape poly1;
	        PolygonShape poly2;
	        if (primaryAxis.type == EPAxisType.EdgeA)
	        {
                poly1 = s_polygonA;
                poly2 = s_polygonB;
		        if (isFrontSide == false)
		        {
			        primaryAxis.index = 1;
		        }
		        manifold._type = ManifoldType.FaceA;
	        }
	        else
	        {
                poly1 = s_polygonB;
                poly2 = s_polygonA;
		        manifold._type = ManifoldType.FaceB;
	        }

	        int edge1 = primaryAxis.index;

            FixedArray2<ClipVertex> incidentEdge = new FixedArray2<ClipVertex>();
	        FindIncidentEdge(ref incidentEdge, poly1, primaryAxis.index, poly2);
	        int count1 = poly1._vertexCount;
	        int iv1 = edge1;
	        int iv2 = edge1 + 1 < count1 ? edge1 + 1 : 0;

	        Vector2 v11 = poly1._vertices[iv1];
	        Vector2 v12 = poly1._vertices[iv2];

	        Vector2 tangent = v12 - v11;
	        tangent.Normalize();
        	
	        Vector2 normal = MathUtils.Cross(tangent, 1.0f);
	        Vector2 planePoint = 0.5f * (v11 + v12);

	        // Face offset.
	        float frontOffset = Vector2.Dot(normal, v11);

	        // Side offsets, extended by polytope skin thickness.
	        float sideOffset1 = -Vector2.Dot(tangent, v11) + totalRadius;
	        float sideOffset2 = Vector2.Dot(tangent, v12) + totalRadius;

	        // Clip incident edge against extruded edge1 side edges.
	        FixedArray2<ClipVertex> clipPoints1;
	        FixedArray2<ClipVertex> clipPoints2;
	        int np;

	        // Clip to box side 1
	        np = ClipSegmentToLine(out clipPoints1, ref incidentEdge, -tangent, sideOffset1, iv1);

	        if (np < Settings.b2_maxManifoldPoints)
	        {
		        return;
	        }

	        // Clip to negative box side 1
	        np = ClipSegmentToLine(out clipPoints2, ref clipPoints1,  tangent, sideOffset2, iv2);

            if (np < Settings.b2_maxManifoldPoints)
	        {
		        return;
	        }

	        // Now clipPoints2 contains the clipped points.
	        if (primaryAxis.type == EPAxisType.EdgeA)
	        {
		        manifold._localNormal = normal;
		        manifold._localPoint = planePoint;
	        }
	        else
	        {
		        manifold._localNormal = MathUtils.MultiplyT(ref xf.R, normal);
		        manifold._localPoint = MathUtils.MultiplyT(ref xf, planePoint);
	        }

	        int pointCount = 0;
	        for (int i = 0; i < Settings.b2_maxManifoldPoints; ++i)
	        {
		        float separation;
        		
		        separation = Vector2.Dot(normal, clipPoints2[i].v) - frontOffset;

		        if (separation <= totalRadius)
		        {
			        ManifoldPoint cp = manifold._points[pointCount];

			        if (primaryAxis.type == EPAxisType.EdgeA)
			        {
				        cp.LocalPoint = MathUtils.MultiplyT(ref xf, clipPoints2[i].v);
                        cp.Id = clipPoints2[i].id;
			        }
			        else
			        {
				        cp.LocalPoint = clipPoints2[i].v;
                        cp.Id.Features.typeA = clipPoints2[i].id.Features.typeB;
                        cp.Id.Features.typeB = clipPoints2[i].id.Features.typeA;
                        cp.Id.Features.indexA = clipPoints2[i].id.Features.indexB;
                        cp.Id.Features.indexB = clipPoints2[i].id.Features.indexA;
			        }

                    manifold._points[pointCount] = cp;
                    if (cp.Id.Features.typeA == (byte)ContactFeatureType.Vertex && types[cp.Id.Features.indexA] == EdgeType.Flat)
			        {
				        continue;
			        }

			        ++pointCount;
		        }
	        }

	        manifold._pointCount = pointCount;
        }

        /// Clipping for contact manifolds.
        public static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn,
							        Vector2 normal, float offset, int vertexIndexA)
        {
            vOut = new FixedArray2<ClipVertex>();

	        // Start with no output points
	        int numOut = 0;

	        // Calculate the distance of end points to the line
	        float distance0 = Vector2.Dot(normal, vIn[0].v) - offset;
	        float distance1 = Vector2.Dot(normal, vIn[1].v) - offset;

	        // If the points are behind the plane
	        if (distance0 <= 0.0f) vOut[numOut++] = vIn[0];
	        if (distance1 <= 0.0f) vOut[numOut++] = vIn[1];

	        // If the points are on different sides of the plane
	        if (distance0 * distance1 < 0.0f)
	        {
		        // Find intersection point of edge and plane
		        float interp = distance0 / (distance0 - distance1);

                var cv = vOut[numOut];

                cv.v = vIn[0].v + interp * (vIn[1].v - vIn[0].v);
		        
                // VertexA is hitting edgeB.
		        cv.id.Features.indexA = (byte)vertexIndexA;
                cv.id.Features.indexB = vIn[0].id.Features.indexB;
                cv.id.Features.typeA = (byte)ContactFeatureType.Vertex;
                cv.id.Features.typeB = (byte)ContactFeatureType.Face;

                vOut[numOut] = cv;

		        ++numOut;
	        }

	        return numOut;
        }

        // Find the separation between poly1 and poly2 for a give edge normal on poly1.
        static float EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1,
							        PolygonShape poly2, ref Transform xf2)
        {
	        int count1 = poly1._vertexCount;
	        int count2 = poly2._vertexCount;

	        Debug.Assert(0 <= edge1 && edge1 < count1);

	        // Convert normal from poly1's frame into poly2's frame.
#if MATH_OVERLOADS
            Vector2 normal1World = MathUtils.Multiply(ref xf1.R, poly1._normals[edge1]);
            Vector2 normal1 = MathUtils.MultiplyT(ref xf2.R, normal1World);
#else
            Vector2 p1n = poly1._normals[edge1];
            Vector2 normal1World = new Vector2(xf1.R.col1.X * p1n.X + xf1.R.col2.X * p1n.Y, xf1.R.col1.Y * p1n.X + xf1.R.col2.Y * p1n.Y);            
            Vector2 normal1 = new Vector2(normal1World.X * xf2.R.col1.X + normal1World.Y * xf2.R.col1.Y, normal1World.X * xf2.R.col2.X + normal1World.Y * xf2.R.col2.Y); 
#endif
	        // Find support vertex on poly2 for -normal.
	        int index = 0;
	        float minDot = Settings.b2_maxFloat;

	        for (int i = 0; i < count2; ++i)
	        {
#if !MATH_OVERLOADS // inlining this made it 1ms slower
		        float dot = Vector2.Dot(poly2._vertices[i], normal1);
#else
                Vector2 p2vi = poly2._vertices[i];
                float dot = p2vi.X * normal1.X + p2vi.Y * normal1.Y;
#endif
		        if (dot < minDot)
		        {
			        minDot = dot;
			        index = i;
		        }
	        }

#if MATH_OVERLOADS
	        Vector2 v1 = MathUtils.Multiply(ref xf1, poly1._vertices[edge1]);
	        Vector2 v2 = MathUtils.Multiply(ref xf2, poly2._vertices[index]);
#else
            Vector2 p1ve = poly1._vertices[edge1];
            Vector2 p2vi = poly2._vertices[index];
            Vector2 v1 = new Vector2(xf1.Position.X + xf1.R.col1.X * p1ve.X + xf1.R.col2.X * p1ve.Y,
                                     xf1.Position.Y + xf1.R.col1.Y * p1ve.X + xf1.R.col2.Y * p1ve.Y);
            Vector2 v2 = new Vector2(xf2.Position.X + xf2.R.col1.X * p2vi.X + xf2.R.col2.X * p2vi.Y,
                                     xf2.Position.Y + xf2.R.col1.Y * p2vi.X + xf2.R.col2.Y * p2vi.Y);
#endif

#if !MATH_OVERLOADS // inlining is 1ms slower
            float separation = Vector2.Dot(v2 - v1, normal1World);
#else
            Vector2 v2subv1 = new Vector2(v2.X - v1.X, v2.Y - v1.Y);
            float separation = v2subv1.X * normal1World.X + v2subv1.Y * normal1World.Y;
#endif
            return separation;
        }

         // Find the max separation between poly1 and poly2 using edge normals from poly1.
        static float FindMaxSeparation( out int edgeIndex,
								        PolygonShape poly1, ref Transform xf1,
								        PolygonShape poly2, ref Transform xf2)
        {
            edgeIndex = -1;
	        int count1 = poly1._vertexCount;

	        // Vector pointing from the centroid of poly1 to the centroid of poly2.
	        Vector2 d = MathUtils.Multiply(ref xf2, poly2._centroid) - MathUtils.Multiply(ref xf1, poly1._centroid);
	        Vector2 dLocal1 = MathUtils.MultiplyT(ref xf1.R, d);

	        // Find edge normal on poly1 that has the largest projection onto d.
	        int edge = 0;
	        float maxDot = -Settings.b2_maxFloat;
	        for (int i = 0; i < count1; ++i)
	        {
		        float dot = Vector2.Dot(poly1._normals[i], dLocal1);
		        if (dot > maxDot)
		        {
			        maxDot = dot;
			        edge = i;
		        }
	        }

	        // Get the separation for the edge normal.
	        float s = EdgeSeparation(poly1, ref xf1, edge, poly2, ref xf2);

	        // Check the separation for the previous edge normal.
	        int prevEdge = edge - 1 >= 0 ? edge - 1 : count1 - 1;
	        float sPrev = EdgeSeparation(poly1, ref xf1, prevEdge, poly2, ref xf2);

	        // Check the separation for the next edge normal.
	        int nextEdge = edge + 1 < count1 ? edge + 1 : 0;
	        float sNext = EdgeSeparation(poly1, ref xf1, nextEdge, poly2, ref xf2);

	        // Find the best edge and the search direction.
	        int bestEdge;
	        float bestSeparation;
	        int increment;
	        if (sPrev > s && sPrev > sNext)
	        {
		        increment = -1;
		        bestEdge = prevEdge;
		        bestSeparation = sPrev;
	        }
	        else if (sNext > s)
	        {
		        increment = 1;
		        bestEdge = nextEdge;
		        bestSeparation = sNext;
	        }
	        else
	        {
		        edgeIndex = edge;
		        return s;
	        }

	        // Perform a local search for the best edge normal.
	        for ( ; ; )
	        {
		        if (increment == -1)
			        edge = bestEdge - 1 >= 0 ? bestEdge - 1 : count1 - 1;
		        else
			        edge = bestEdge + 1 < count1 ? bestEdge + 1 : 0;

		        s = EdgeSeparation(poly1, ref xf1, edge, poly2, ref xf2);

		        if (s > bestSeparation)
		        {
			        bestEdge = edge;
			        bestSeparation = s;
		        }
		        else
		        {
			        break;
		        }
	        }

	        edgeIndex = bestEdge;
	        return bestSeparation;
        }

        static void FindIncidentEdge(out FixedArray2<ClipVertex> c,
							         PolygonShape poly1, ref Transform xf1, int edge1,
							         PolygonShape poly2, ref Transform xf2)
        {
            c = new FixedArray2<ClipVertex>();

	        int count1 = poly1._vertexCount;
	        int count2 = poly2._vertexCount;

	        Debug.Assert(0 <= edge1 && edge1 < count1);

	        // Get the normal of the reference edge in poly2's frame.
	        Vector2 normal1 = MathUtils.MultiplyT(ref xf2.R, MathUtils.Multiply(ref xf1.R, poly1._normals[edge1]));

	        // Find the incident edge on poly2.
	        int index = 0;
	        float minDot = Settings.b2_maxFloat;
	        for (int i = 0; i < count2; ++i)
	        {
		        float dot = Vector2.Dot(normal1, poly2._normals[i]);
		        if (dot < minDot)
		        {
			        minDot = dot;
			        index = i;
		        }
	        }

	        // Build the clip vertices for the incident edge.
	        int i1 = index;
	        int i2 = i1 + 1 < count2 ? i1 + 1 : 0;

            var cv0 = c[0];

            cv0.v = MathUtils.Multiply(ref xf2, poly2._vertices[i1]);
            cv0.id.Features.indexA = (byte)edge1;
            cv0.id.Features.indexB = (byte)i1;
            cv0.id.Features.typeA = (byte)ContactFeatureType.Face;
            cv0.id.Features.typeB = (byte)ContactFeatureType.Vertex;

            c[0] = cv0;

            var cv1 = c[1];
            cv1.v = MathUtils.Multiply(ref xf2, poly2._vertices[i2]);
            cv1.id.Features.indexA = (byte)edge1;
            cv1.id.Features.indexB = (byte)i2;
            cv1.id.Features.typeA = (byte)ContactFeatureType.Face;
            cv1.id.Features.typeB = (byte)ContactFeatureType.Vertex;

            c[1] = cv1;
        }
    }
}

