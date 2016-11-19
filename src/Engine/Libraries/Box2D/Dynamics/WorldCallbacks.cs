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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Box2D.XNA
{
    /// Called for each fixture found in the query. You control how the ray cast
    /// proceeds by returning a float:
    /// return -1: ignore this fixture and continue
    /// return 0: terminate the ray cast
    /// return fraction: clip the ray to this point
    /// return 1: don't clip the ray and continue
    /// @param fixture the fixture hit by the ray
    /// @param point the point of initial intersection
    /// @param normal the normal vector at the point of intersection
    /// @return -1 to filter, 0 to terminate, fraction to clip the ray for
    /// closest hit, 1 to continue
    public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);

    public interface IDestructionListener
    {
        void SayGoodbye(Joint joint);
        void SayGoodbye(Fixture fixture);
    }

    public interface IContactFilter
    {
        bool ShouldCollide(Fixture fixtureA, Fixture fixtureB);
    }

    public class DefaultContactFilter : IContactFilter
    {
        public bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            Filter filterA;
            fixtureA.GetFilterData(out filterA);

            Filter filterB;
            fixtureB.GetFilterData(out filterB);

	        if (filterA.groupIndex == filterB.groupIndex && filterA.groupIndex != 0)
	        {
		        return filterA.groupIndex > 0;
	        }

	        bool collide = (filterA.maskBits & filterB.categoryBits) != 0 && (filterA.categoryBits & filterB.maskBits) != 0;
	        
            return collide;
        }
    }

    public struct ContactImpulse
    {
        public FixedArray2<float> normalImpulses;
        public FixedArray2<float> tangentImpulses;
    }

    public interface IContactListener
    {
        void BeginContact(Contact contact);
        void EndContact(Contact contact);
        void PreSolve(Contact contact, ref Manifold oldManifold);
        void PostSolve(Contact contact, ref ContactImpulse impulse);
    }

    public class DefaultContactListener : IContactListener
    {
        public void BeginContact(Contact contact) { }
        public void EndContact(Contact contact) { }
        public void PreSolve(Contact contact, ref Manifold oldManifold) { }
        public void PostSolve(Contact contact, ref ContactImpulse impulse) { }
    }

    [Flags]
    public enum DebugDrawFlags
    {
	    Shape			= (1 << 0), ///< draw shapes
	    Joint			= (1 << 1), ///< draw joint connections
	    AABB			= (1 << 2), ///< draw axis aligned bounding boxes
	    Pair			= (1 << 3), ///< draw broad-phase pairs
	    CenterOfMass	= (1 << 4), ///< draw center of mass frame
    };

    /// Implement and register this class with a World to provide debug drawing of physics
    /// entities in your game.
    public abstract class DebugDraw
    {
	    public DebugDrawFlags Flags { get; set; }
    	
	    /// Append flags to the current flags.
	    public void AppendFlags(DebugDrawFlags flags)
        {
            Flags |= flags;
        }

	    /// Clear flags from the current flags.
	    public  void ClearFlags(DebugDrawFlags flags)
        {
            Flags &= ~flags;
        }

	    /// Draw a closed polygon provided in CCW order.
	    public abstract void DrawPolygon(ref FixedArray8<Vector2> vertices, int count, Color color);

	    /// Draw a solid closed polygon provided in CCW order.
        public abstract void DrawSolidPolygon(ref FixedArray8<Vector2> vertices, int count, Color color);

	    /// Draw a circle.
        public abstract void DrawCircle(Vector2 center, float radius, Color color);
    	
	    /// Draw a solid circle.
        public abstract void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color);
    	
	    /// Draw a line segment.
        public abstract void DrawSegment(Vector2 p1, Vector2 p2, Color color);

	    /// Draw a transform. Choose your own length scale.
	    /// @param xf a transform.
        public abstract void DrawTransform(ref Transform xf);
    }
}
