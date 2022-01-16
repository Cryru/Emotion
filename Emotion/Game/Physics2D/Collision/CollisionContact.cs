#region Using

using System;
using Emotion.Common;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Shape;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    [Flags]
    public enum ContactFlags : byte
    {
        Unknown = 0,

        /// <summary>
        /// Used when crawling the contact graph when forming islands.
        /// </summary>
        Island = 1,

        /// <summary>
        /// Set when the shapes are touching.
        /// </summary>
        Touching = 2 << 0,

        /// <summary>
        /// This contact can be disabled (by user)
        /// </summary>
        Enabled = 2 << 1,

        /// <summary>
        /// This contact needs filtering because a fixture filter was changed.
        /// </summary>
        FilterNeeded = 2 << 2,

        /// <summary>
        /// This bullet contact had a TOI event
        /// </summary>
        BulletHit = 2 << 3,

        /// <summary>
        /// This contact has a valid TOI
        /// </summary>
        TimeOfImpact = 2 << 4
    }

    public enum ContactType : byte
    {
        NotSupported,
        Polygon,
    }

    public class CollisionContact
    {
        public ContactFlags Flags;
        public ContactType ContactType;

        public PhysicsLink LinkA;
        public PhysicsLink LinkB;

        public int TimeOfImpactCount;
        public float TimeOfImpact;

        public Manifold Manifold;

        public float Friction;
        public float Restitution;
        public float RestitutionThreshold;
        public float TangentSpeed;

        /// <summary>
        /// Enable/disable this contact.The contact is only disabled for the current time step (or sub-step in continuous
        /// collisions).
        /// </summary>
        public bool Enabled
        {
            get => (Flags & ContactFlags.Enabled) == ContactFlags.Enabled;
            set
            {
                if (value)
                    Flags |= ContactFlags.Enabled;
                else
                    Flags &= ~ContactFlags.Enabled;
            }
        }

        internal bool IsTouching
        {
            get => (Flags & ContactFlags.Touching) == ContactFlags.Touching;
        }

        internal bool IslandFlag
        {
            get => (Flags & ContactFlags.Island) == ContactFlags.Island;
        }

        internal bool TOIFlag
        {
            get => (Flags & ContactFlags.TimeOfImpact) == ContactFlags.TimeOfImpact;
        }

        internal bool FilterFlag
        {
            get => (Flags & ContactFlags.FilterNeeded) == ContactFlags.FilterNeeded;
        }

        public void Set(PhysicsLink a, PhysicsLink b)
        {
            Flags = ContactFlags.Enabled;
            LinkA = a;
            LinkB = b;

            Manifold.PointCount = 0;

            TimeOfImpactCount = 0;
            TangentSpeed = 0;

            Friction = MixFriction(LinkA.Friction, LinkB.Friction);
            Restitution = MixRestitution(LinkA.Restitution, LinkB.Restitution);
            RestitutionThreshold = MixRestitutionThreshold(LinkA.RestitutionThreshold, LinkB.RestitutionThreshold);
            ContactType = LinkA.Shape.GetContactType(LinkB.Shape);
        }

        /// <summary>
        /// Friction mixing law.
        /// The idea is to allow either fixture to drive the friction to zero. For example, anything slides on ice.
        /// </summary>
        private static float MixFriction(float friction1, float friction2)
        {
            return (float) Math.Sqrt(friction1 * friction2);
        }

        /// <summary>
        /// Restitution mixing law.
        /// The idea is allow for anything to bounce off an inelastic surface. For example, a superball bounces on anything.
        /// </summary>
        private static float MixRestitution(float restitution1, float restitution2)
        {
            return restitution1 > restitution2 ? restitution1 : restitution2;
        }

        /// <summary>Restitution mixing law. This picks the lowest value.</summary>
        private static float MixRestitutionThreshold(float threshold1, float threshold2)
        {
            return threshold1 < threshold2 ? threshold1 : threshold2;
        }

        /// <summary>
        /// Update the contact manifold and touching status.
        /// Note: do not assume the fixture AABBs are overlapping or are valid.
        /// </summary>
        public void Update()
        {
            Manifold oldManifold = Manifold;

            // Re-enable this contact.
            Flags |= ContactFlags.Enabled;

            bool touching;
            bool wasTouching = Flags.HasFlag(ContactFlags.Touching);

            PhysicsBody bodyA = LinkA.Body;
            PhysicsBody bodyB = LinkB.Body;

            // todo: check if trigger (sensor)

            UpdateCollision();
            touching = Manifold.PointCount > 0;

            // Match old contact ids to new contact ids and copy the
            // stored impulses to warm start the solver.
            for (var i = 0; i < Manifold.PointCount; ++i)
            {
                ManifoldPoint mp2 = Manifold.Points[i];
                mp2.NormalImpulse = 0.0f;
                mp2.TangentImpulse = 0.0f;
                ContactId id2 = mp2.Id;

                for (var j = 0; j < oldManifold.PointCount; ++j)
                {
                    ManifoldPoint mp1 = oldManifold.Points[j];
                    if (mp1.Id.Key == id2.Key)
                    {
                        mp2.NormalImpulse = mp1.NormalImpulse;
                        mp2.TangentImpulse = mp1.TangentImpulse;
                        break;
                    }
                }

                Manifold.Points[i] = mp2;
            }

            if (touching != wasTouching)
            {
                bodyA.Awake = true;
                bodyB.Awake = true;
            }

            if (touching)
                Flags |= ContactFlags.Touching;
            else
                Flags &= ~ContactFlags.Touching;

            if (!wasTouching && touching)
            {
                // todo: OnCollision event
            }
            else if (wasTouching && !touching)
            {
                // todo: OnSeparation event
            }
        }

        private void UpdateCollision()
        {
            switch (ContactType)
            {
                case ContactType.Polygon:
                    PolygonCollision.CollidePolygons(ref Manifold, (PolygonShape) LinkA.Shape, LinkA.Body.BodyTransform, (PolygonShape) LinkB.Shape, LinkB.Body.BodyTransform);
                    break;
                default:
                    throw new ArgumentException("You are using an unsupported contact type.");
            }
        }
    }
}