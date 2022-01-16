#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Shape;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    public class ContactSolver
    {
        public class VelocityConstraintPoint
        {
            public float NormalImpulse;
            public float NormalMass;
            public Vector2 RA;
            public Vector2 RB;
            public float TangentImpulse;
            public float TangentMass;
            public float VelocityBias;
        }

        public class ContactVelocityConstraint
        {
            public int ContactIndex;
            public float Friction;
            public int IndexA;
            public int IndexB;
            public float InvIa, InvIb;
            public float InvMassA, InvMassB;
            public Matrix2x2 K;
            public Vector2 Normal;
            public Matrix2x2 NormalMass;
            public int PointCount;
            public FixedArray2<VelocityConstraintPoint> Points;
            public float Restitution;
            public float Threshold;
            public float TangentSpeed;

            public ContactVelocityConstraint()
            {
                Points[0] = new VelocityConstraintPoint();
                Points[1] = new VelocityConstraintPoint();
            }
        }

        public class ContactPositionConstraint
        {
            public int IndexA;
            public int IndexB;
            public float InvIa, InvIb;
            public float InvMassA, InvMassB;
            public Vector2 LocalCenterA, LocalCenterB;
            public Vector2 LocalNormal;
            public Vector2 LocalPoint;
            public FixedArray2<Vector2> LocalPoints;
            public int PointCount;
            public float RadiusA, RadiusB;
            public ManifoldType Type;
        }

        private CollisionContact[] _contacts;
        private int _count;
        private ContactPositionConstraint[] _positionConstraints;
        private PhysicsIsland.Position[] _positions;
        private PhysicsTimeStepData _step;
        private PhysicsIsland.Velocity[] _velocities;
        public ContactVelocityConstraint[] VelocityConstraints;

        /// <summary>
        /// Evaluate the manifold with supplied transforms. This assumes modest motion from the original state. This does
        /// not change the point count, impulses, etc. The radii must come from the Shapes that generated the manifold.
        /// </summary>
        public static void InitializeWorldManifold(
            ref Manifold manifold,
            PhysicsTransform xfA,
            float radiusA,
            PhysicsTransform xfB,
            float radiusB,
            out Vector2 normal,
            out FixedArray2<Vector2> points,
            out FixedArray2<float> separations)
        {
            normal = Vector2.Zero;
            points = new FixedArray2<Vector2>();
            separations = new FixedArray2<float>();

            if (manifold.PointCount == 0)
                return;

            switch (manifold.Type)
            {
                case ManifoldType.FaceA:
                {
                    normal = xfA.RotateVector(manifold.LocalNormal);
                    Vector2 planePoint = xfA.TransformVector(manifold.LocalPoint);

                    for (var i = 0; i < manifold.PointCount; ++i)
                    {
                        Vector2 clipPoint = xfB.TransformVector(manifold.Points[i].LocalPoint);
                        Vector2 cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
                        Vector2 cB = clipPoint - radiusB * normal;
                        points[i] = 0.5f * (cA + cB);
                        separations[i] = Vector2.Dot(cB - cA, normal);
                    }
                }
                    break;

                case ManifoldType.FaceB:
                {
                    normal = xfB.RotateVector(manifold.LocalNormal);
                    Vector2 planePoint = xfB.TransformVector(manifold.LocalPoint);

                    for (var i = 0; i < manifold.PointCount; ++i)
                    {
                        Vector2 clipPoint = xfA.TransformVector(manifold.Points[i].LocalPoint);
                        Vector2 cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
                        Vector2 cA = clipPoint - radiusA * normal;
                        points[i] = 0.5f * (cA + cB);
                        separations[i] = Vector2.Dot(cA - cB, normal);
                    }

                    // Ensure normal points from A to B.
                    normal = -normal;
                }
                    break;
            }
        }

        public static void InitializePositionManifold(
            ContactPositionConstraint pc,
            PhysicsTransform xfA, PhysicsTransform xfB,
            int index,
            out Vector2 normal,
            out Vector2 point,
            out float separation)
        {
            Debug.Assert(pc.PointCount > 0);

            switch (pc.Type)
            {
                case ManifoldType.FaceA:
                {
                    normal = xfA.RotateVector(pc.LocalNormal);
                    Vector2 planePoint = xfA.TransformVector(pc.LocalPoint);

                    Vector2 clipPoint = xfB.TransformVector(pc.LocalPoints[index]);
                    separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.RadiusA - pc.RadiusB;
                    point = clipPoint;
                }
                    break;

                case ManifoldType.FaceB:
                {
                    normal = xfB.RotateVector(pc.LocalNormal);
                    Vector2 planePoint = xfB.TransformVector(pc.LocalPoint);

                    Vector2 clipPoint = xfA.TransformVector(pc.LocalPoints[index]);
                    separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.RadiusA - pc.RadiusB;
                    point = clipPoint;

                    // Ensure normal points from A to B
                    normal = -normal;
                }
                    break;
                default:
                    normal = Vector2.Zero;
                    point = Vector2.Zero;
                    separation = 0;
                    break;
            }
        }

        public void Reset(PhysicsTimeStepData step, int count, CollisionContact[] contacts, PhysicsIsland.Position[] positions, PhysicsIsland.Velocity[] velocities, bool warmStart = true)
        {
            _step = step;
            _count = count;
            _positions = positions;
            _velocities = velocities;
            _contacts = contacts;

            // grow the array
            if (VelocityConstraints == null || VelocityConstraints.Length < count)
            {
                VelocityConstraints = new ContactVelocityConstraint[count * 2];
                _positionConstraints = new ContactPositionConstraint[count * 2];

                for (var i = 0; i < VelocityConstraints.Length; i++)
                {
                    VelocityConstraints[i] = new ContactVelocityConstraint();
                }

                for (var i = 0; i < _positionConstraints.Length; i++)
                {
                    _positionConstraints[i] = new ContactPositionConstraint();
                }
            }

            // Initialize position independent portions of the constraints.
            for (var i = 0; i < _count; ++i)
            {
                CollisionContact contact = contacts[i];

                PhysicsLink fixtureA = contact.LinkA;
                PhysicsLink fixtureB = contact.LinkB;
                ShapeBase shapeA = fixtureA.Shape;
                ShapeBase shapeB = fixtureB.Shape;
                float radiusA = shapeA.Radius;
                float radiusB = shapeB.Radius;
                PhysicsBody bodyA = fixtureA.Body;
                PhysicsBody bodyB = fixtureB.Body;
                Manifold manifold = contact.Manifold;

                int pointCount = manifold.PointCount;
                Debug.Assert(pointCount > 0);

                ContactVelocityConstraint vc = VelocityConstraints[i];
                vc.Friction = contact.Friction;
                vc.Restitution = contact.Restitution;
                vc.Threshold = contact.RestitutionThreshold;
                vc.TangentSpeed = contact.TangentSpeed;
                vc.IndexA = bodyA.IslandIndex;
                vc.IndexB = bodyB.IslandIndex;
                vc.InvMassA = bodyA.InvMass;
                vc.InvMassB = bodyB.InvMass;
                vc.InvIa = bodyA.InvInertia;
                vc.InvIb = bodyB.InvInertia;
                vc.ContactIndex = i;
                vc.PointCount = pointCount;
                vc.K.SetZero();
                vc.NormalMass.SetZero();

                ContactPositionConstraint pc = _positionConstraints[i];
                pc.IndexA = bodyA.IslandIndex;
                pc.IndexB = bodyB.IslandIndex;
                pc.InvMassA = bodyA.InvMass;
                pc.InvMassB = bodyB.InvMass;
                pc.LocalCenterA = bodyA.Sweep.LocalCenter;
                pc.LocalCenterB = bodyB.Sweep.LocalCenter;
                pc.InvIa = bodyA.InvInertia;
                pc.InvIb = bodyB.InvInertia;
                pc.LocalNormal = manifold.LocalNormal;
                pc.LocalPoint = manifold.LocalPoint;
                pc.PointCount = pointCount;
                pc.RadiusA = radiusA;
                pc.RadiusB = radiusB;
                pc.Type = manifold.Type;

                for (var j = 0; j < pointCount; ++j)
                {
                    ManifoldPoint cp = manifold.Points[j];
                    VelocityConstraintPoint vcp = vc.Points[j];

                    if (warmStart)
                    {
                        vcp.NormalImpulse = _step.DeltaTimeRatio * cp.NormalImpulse;
                        vcp.TangentImpulse = _step.DeltaTimeRatio * cp.TangentImpulse;
                    }
                    else
                    {
                        vcp.NormalImpulse = 0.0f;
                        vcp.TangentImpulse = 0.0f;
                    }

                    vcp.RA = Vector2.Zero;
                    vcp.RB = Vector2.Zero;
                    vcp.NormalMass = 0.0f;
                    vcp.TangentMass = 0.0f;
                    vcp.VelocityBias = 0.0f;

                    pc.LocalPoints[j] = cp.LocalPoint;
                }
            }
        }

        /// <summary>Initialize position dependent portions of the velocity constraints.</summary>
        public void InitializeVelocityConstraints()
        {
            for (var i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];
                ContactPositionConstraint pc = _positionConstraints[i];

                float radiusA = pc.RadiusA;
                float radiusB = pc.RadiusB;
                Manifold manifold = _contacts[vc.ContactIndex].Manifold;

                int indexA = vc.IndexA;
                int indexB = vc.IndexB;

                float mA = vc.InvMassA;
                float mB = vc.InvMassB;
                float iA = vc.InvIa;
                float iB = vc.InvIb;
                Vector2 localCenterA = pc.LocalCenterA;
                Vector2 localCenterB = pc.LocalCenterB;

                Vector2 cA = _positions[indexA].C;
                float aA = _positions[indexA].A;
                Vector2 vA = _velocities[indexA].V;
                float wA = _velocities[indexA].W;

                Vector2 cB = _positions[indexB].C;
                float aB = _positions[indexB].A;
                Vector2 vB = _velocities[indexB].V;
                float wB = _velocities[indexB].W;

                Debug.Assert(manifold.PointCount > 0);

                var xfA = new PhysicsTransform();
                var xfB = new PhysicsTransform();
                xfA.Rotation.SetAngle(aA);
                xfB.Rotation.SetAngle(aB);
                xfA.Position = cA - xfA.RotateVector(localCenterA);
                xfB.Position = cB - xfB.RotateVector(localCenterB);

                InitializeWorldManifold(ref manifold, xfA, radiusA, xfB, radiusB, out Vector2 normal, out FixedArray2<Vector2> points, out _);

                vc.Normal = normal;

                int pointCount = vc.PointCount;
                for (var j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.Points[j];

                    vcp.RA = points[j] - cA;
                    vcp.RB = points[j] - cB;

                    float rnA = Maths.Cross2D(vcp.RA, vc.Normal);
                    float rnB = Maths.Cross2D(vcp.RB, vc.Normal);

                    float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    vcp.NormalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;

                    Vector2 tangent = Maths.Cross2D(vc.Normal, 1.0f);

                    float rtA = Maths.Cross2D(vcp.RA, tangent);
                    float rtB = Maths.Cross2D(vcp.RB, tangent);

                    float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

                    vcp.TangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

                    // Setup a velocity bias for restitution.
                    vcp.VelocityBias = 0.0f;
                    float vRel = Vector2.Dot(vc.Normal, vB + Maths.Cross2D(wB, vcp.RB) - vA - Maths.Cross2D(wA, vcp.RA));
                    if (vRel < -vc.Threshold)
                        vcp.VelocityBias = -vc.Restitution * vRel;
                }

                // If we have two points, then prepare the block solver.
                if (vc.PointCount == 2)
                {
                    VelocityConstraintPoint vcp1 = vc.Points[0];
                    VelocityConstraintPoint vcp2 = vc.Points[1];

                    float rn1A = Maths.Cross2D(vcp1.RA, vc.Normal);
                    float rn1B = Maths.Cross2D(vcp1.RB, vc.Normal);
                    float rn2A = Maths.Cross2D(vcp2.RA, vc.Normal);
                    float rn2B = Maths.Cross2D(vcp2.RB, vc.Normal);

                    float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
                    float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
                    float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

                    // Ensure a reasonable condition number.
                    const float kMaxConditionNumber = 1000.0f;
                    if (k11 * k11 < kMaxConditionNumber * (k11 * k22 - k12 * k12))
                    {
                        // K is safe to invert.
                        vc.K.Ex = new Vector2(k11, k12);
                        vc.K.Ey = new Vector2(k12, k22);
                        vc.NormalMass = vc.K.Inverse;
                    }
                    else
                    {
                        // The constraints are redundant, just use one.
                        // TODO_ERIN use deepest?
                        vc.PointCount = 1;
                    }
                }
            }
        }

        public void WarmStart()
        {
            // Warm start.
            for (var i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];

                int indexA = vc.IndexA;
                int indexB = vc.IndexB;
                float mA = vc.InvMassA;
                float iA = vc.InvIa;
                float mB = vc.InvMassB;
                float iB = vc.InvIb;
                int pointCount = vc.PointCount;

                Vector2 vA = _velocities[indexA].V;
                float wA = _velocities[indexA].W;
                Vector2 vB = _velocities[indexB].V;
                float wB = _velocities[indexB].W;

                Vector2 normal = vc.Normal;
                Vector2 tangent = Maths.Cross2D(normal, 1.0f);

                for (var j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.Points[j];
                    Vector2 p = vcp.NormalImpulse * normal + vcp.TangentImpulse * tangent;
                    wA -= iA * Maths.Cross2D(vcp.RA, p);
                    vA -= mA * p;
                    wB += iB * Maths.Cross2D(vcp.RB, p);
                    vB += mB * p;
                }

                _velocities[indexA].V = vA;
                _velocities[indexA].W = wA;
                _velocities[indexB].V = vB;
                _velocities[indexB].W = wB;
            }
        }

        public void SolveVelocityConstraints()
        {
            for (var i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];

                int indexA = vc.IndexA;
                int indexB = vc.IndexB;
                float mA = vc.InvMassA;
                float iA = vc.InvIa;
                float mB = vc.InvMassB;
                float iB = vc.InvIb;
                int pointCount = vc.PointCount;

                Vector2 vA = _velocities[indexA].V;
                float wA = _velocities[indexA].W;
                Vector2 vB = _velocities[indexB].V;
                float wB = _velocities[indexB].W;

                Vector2 normal = vc.Normal;
                Vector2 tangent = Maths.Cross2D(normal, 1.0f);
                float friction = vc.Friction;

                Debug.Assert(pointCount == 1 || pointCount == 2);

                // Solve tangent constraints first because non-penetration is more important
                // than friction.
                for (var j = 0; j < pointCount; ++j)
                {
                    VelocityConstraintPoint vcp = vc.Points[j];

                    // Relative velocity at contact
                    Vector2 dv = vB + Maths.Cross2D(wB, vcp.RB) - vA - Maths.Cross2D(wA, vcp.RA);

                    // Compute tangent force
                    float vt = Vector2.Dot(dv, tangent) - vc.TangentSpeed;
                    float lambda = vcp.TangentMass * -vt;

                    // b2Clamp the accumulated force
                    float maxFriction = friction * vcp.NormalImpulse;
                    float newImpulse = Maths.Clamp(vcp.TangentImpulse + lambda, -maxFriction, maxFriction);
                    lambda = newImpulse - vcp.TangentImpulse;
                    vcp.TangentImpulse = newImpulse;

                    // Apply contact impulse
                    Vector2 p = lambda * tangent;

                    vA -= mA * p;
                    wA -= iA * Maths.Cross2D(vcp.RA, p);

                    vB += mB * p;
                    wB += iB * Maths.Cross2D(vcp.RB, p);
                }

                // Solve normal constraints
                if (pointCount == 1)
                {
                    for (var j = 0; j < pointCount; ++j)
                    {
                        VelocityConstraintPoint vcp = vc.Points[j];

                        // Relative velocity at contact
                        Vector2 dv = vB + Maths.Cross2D(wB, vcp.RB) - vA - Maths.Cross2D(wA, vcp.RA);

                        // Compute normal impulse
                        float vn = Vector2.Dot(dv, normal);
                        float lambda = -vcp.NormalMass * (vn - vcp.VelocityBias);

                        // b2Clamp the accumulated impulse
                        float newImpulse = Math.Max(vcp.NormalImpulse + lambda, 0.0f);
                        lambda = newImpulse - vcp.NormalImpulse;
                        vcp.NormalImpulse = newImpulse;

                        // Apply contact impulse
                        Vector2 p = lambda * normal;
                        vA -= mA * p;
                        wA -= iA * Maths.Cross2D(vcp.RA, p);

                        vB += mB * p;
                        wB += iB * Maths.Cross2D(vcp.RB, p);
                    }
                }
                else
                {
                    // Block solver developed in collaboration with Dirk Gregorius (back in 01/07 on Box2D_Lite).
                    // Build the mini LCP for this contact patch
                    //
                    // vn = A * x + b, vn >= 0, x >= 0 and vn_i * x_i = 0 with i = 1..2
                    //
                    // A = J * W * JT and J = ( -n, -r1 x n, n, r2 x n )
                    // b = vn0 - velocityBias
                    //
                    // The system is solved using the "Total enumeration method" (s. Murty). The complementary constraint vn_i * x_i
                    // implies that we must have in any solution either vn_i = 0 or x_i = 0. So for the 2D contact problem the cases
                    // vn1 = 0 and vn2 = 0, x1 = 0 and x2 = 0, x1 = 0 and vn2 = 0, x2 = 0 and vn1 = 0 need to be tested. The first valid
                    // solution that satisfies the problem is chosen.
                    // 
                    // In order to account of the accumulated impulse 'a' (because of the iterative nature of the solver which only requires
                    // that the accumulated impulse is clamped and not the incremental impulse) we change the impulse variable (x_i).
                    //
                    // Substitute:
                    // 
                    // x = a + d
                    // 
                    // a := old total impulse
                    // x := new total impulse
                    // d := incremental impulse 
                    //
                    // For the current iteration we extend the formula for the incremental impulse
                    // to compute the new total impulse:
                    //
                    // vn = A * d + b
                    //    = A * (x - a) + b
                    //    = A * x + b - A * a
                    //    = A * x + b'
                    // b' = b - A * a;

                    VelocityConstraintPoint cp1 = vc.Points[0];
                    VelocityConstraintPoint cp2 = vc.Points[1];

                    var a = new Vector2(cp1.NormalImpulse, cp2.NormalImpulse);
                    Debug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

                    // Relative velocity at contact
                    Vector2 dv1 = vB + Maths.Cross2D(wB, cp1.RB) - vA - Maths.Cross2D(wA, cp1.RA);
                    Vector2 dv2 = vB + Maths.Cross2D(wB, cp2.RB) - vA - Maths.Cross2D(wA, cp2.RA);

                    // Compute normal velocity
                    float vn1 = Vector2.Dot(dv1, normal);
                    float vn2 = Vector2.Dot(dv2, normal);

                    Vector2 b = Vector2.Zero;
                    b.X = vn1 - cp1.VelocityBias;
                    b.Y = vn2 - cp2.VelocityBias;

                    // Compute b'
                    b -= vc.K.Transform(a);

                    for (;;)
                    {
                        //
                        // Case 1: vn = 0
                        //
                        // 0 = A * x + b'
                        //
                        // Solve for x:
                        //
                        // x = - inv(A) * b'
                        //
                        Vector2 x = -vc.NormalMass.Transform(b);

                        if (x.X >= 0.0f && x.Y >= 0.0f)
                        {
                            // Get the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 p1 = d.X * normal;
                            Vector2 p2 = d.Y * normal;
                            vA -= mA * (p1 + p2);
                            wA -= iA * (Maths.Cross2D(cp1.RA, p1) + Maths.Cross2D(cp2.RA, p2));

                            vB += mB * (p1 + p2);
                            wB += iB * (Maths.Cross2D(cp1.RB, p1) + Maths.Cross2D(cp2.RB, p2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;
                            break;
                        }

                        //
                        // Case 2: vn1 = 0 and x2 = 0
                        //
                        //   0 = a11 * x1 + a12 * 0 + b1' 
                        // vn2 = a21 * x1 + a22 * 0 + b2'
                        //
                        x.X = -cp1.NormalMass * b.X;
                        x.Y = 0.0f;
                        vn1 = 0.0f;
                        vn2 = vc.K.Ex.Y * x.X + b.Y;

                        if (x.X >= 0.0f && vn2 >= 0.0f)
                        {
                            // Get the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 p1 = d.X * normal;
                            Vector2 p2 = d.Y * normal;
                            vA -= mA * (p1 + p2);
                            wA -= iA * (Maths.Cross2D(cp1.RA, p1) + Maths.Cross2D(cp2.RA, p2));

                            vB += mB * (p1 + p2);
                            wB += iB * (Maths.Cross2D(cp1.RB, p1) + Maths.Cross2D(cp2.RB, p2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;
                            break;
                        }

                        //
                        // Case 3: vn2 = 0 and x1 = 0
                        //
                        // vn1 = a11 * 0 + a12 * x2 + b1' 
                        //   0 = a21 * 0 + a22 * x2 + b2'
                        //
                        x.X = 0.0f;
                        x.Y = -cp2.NormalMass * b.Y;
                        vn1 = vc.K.Ey.X * x.Y + b.X;
                        vn2 = 0.0f;

                        if (x.Y >= 0.0f && vn1 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 p1 = d.X * normal;
                            Vector2 p2 = d.Y * normal;
                            vA -= mA * (p1 + p2);
                            wA -= iA * (Maths.Cross2D(cp1.RA, p1) + Maths.Cross2D(cp2.RA, p2));

                            vB += mB * (p1 + p2);
                            wB += iB * (Maths.Cross2D(cp1.RB, p1) + Maths.Cross2D(cp2.RB, p2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;
                            break;
                        }

                        //
                        // Case 4: x1 = 0 and x2 = 0
                        // 
                        // vn1 = b1
                        // vn2 = b2;
                        x.X = 0.0f;
                        x.Y = 0.0f;
                        vn1 = b.X;
                        vn2 = b.Y;

                        if (vn1 >= 0.0f && vn2 >= 0.0f)
                        {
                            // Resubstitute for the incremental impulse
                            Vector2 d = x - a;

                            // Apply incremental impulse
                            Vector2 p1 = d.X * normal;
                            Vector2 p2 = d.Y * normal;
                            vA -= mA * (p1 + p2);
                            wA -= iA * (Maths.Cross2D(cp1.RA, p1) + Maths.Cross2D(cp2.RA, p2));

                            vB += mB * (p1 + p2);
                            wB += iB * (Maths.Cross2D(cp1.RB, p1) + Maths.Cross2D(cp2.RB, p2));

                            // Accumulate
                            cp1.NormalImpulse = x.X;
                            cp2.NormalImpulse = x.Y;
                        }

                        // No solution, give up. This is hit sometimes, but it doesn't seem to matter.
                        break;
                    }
                }

                _velocities[indexA].V = vA;
                _velocities[indexA].W = wA;
                _velocities[indexB].V = vB;
                _velocities[indexB].W = wB;
            }
        }

        public void StoreImpulses()
        {
            for (var i = 0; i < _count; ++i)
            {
                ContactVelocityConstraint vc = VelocityConstraints[i];
                Manifold manifold = _contacts[vc.ContactIndex].Manifold;

                for (var j = 0; j < vc.PointCount; ++j)
                {
                    ManifoldPoint point = manifold.Points[j];
                    point.NormalImpulse = vc.Points[j].NormalImpulse;
                    point.TangentImpulse = vc.Points[j].TangentImpulse;
                    manifold.Points[j] = point;
                }

                _contacts[vc.ContactIndex].Manifold = manifold;
            }
        }

        public bool SolvePositionConstraints()
        {
            var minSeparation = 0.0f;

            for (var i = 0; i < _count; ++i)
            {
                ContactPositionConstraint pc = _positionConstraints[i];

                int indexA = pc.IndexA;
                int indexB = pc.IndexB;
                Vector2 localCenterA = pc.LocalCenterA;
                float mA = pc.InvMassA;
                float iA = pc.InvIa;
                Vector2 localCenterB = pc.LocalCenterB;
                float mB = pc.InvMassB;
                float iB = pc.InvIb;
                int pointCount = pc.PointCount;

                Vector2 cA = _positions[indexA].C;
                float aA = _positions[indexA].A;

                Vector2 cB = _positions[indexB].C;
                float aB = _positions[indexB].A;

                // Solve normal constraints
                for (var j = 0; j < pointCount; ++j)
                {
                    var xfA = new PhysicsTransform();
                    var xfB = new PhysicsTransform();
                    xfA.Rotation.SetAngle(aA);
                    xfB.Rotation.SetAngle(aB);
                    xfA.Position = cA - xfA.RotateVector(localCenterA);
                    xfB.Position = cB - xfB.RotateVector(localCenterB);

                    InitializePositionManifold(pc, xfA, xfB, j, out Vector2 normal, out Vector2 point, out float separation);

                    Vector2 rA = point - cA;
                    Vector2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float c = Maths.Clamp(PhysicsConfig.Baumgarte * (separation + PhysicsConfig.LinearSlop), -PhysicsConfig.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = Maths.Cross2D(rA, normal);
                    float rnB = Maths.Cross2D(rB, normal);
                    float k = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = k > 0.0f ? -c / k : 0.0f;

                    Vector2 p = impulse * normal;

                    cA -= mA * p;
                    aA -= iA * Maths.Cross2D(rA, p);

                    cB += mB * p;
                    aB += iB * Maths.Cross2D(rB, p);
                }

                _positions[indexA].C = cA;
                _positions[indexA].A = aA;

                _positions[indexB].C = cB;
                _positions[indexB].A = aB;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -3.0f * PhysicsConfig.LinearSlop;
        }

        // Sequential position solver for position constraints.
        public bool SolveToIPositionConstraints(int toiIndexA, int toiIndexB)
        {
            var minSeparation = 0.0f;

            for (var i = 0; i < _count; ++i)
            {
                ContactPositionConstraint pc = _positionConstraints[i];

                int indexA = pc.IndexA;
                int indexB = pc.IndexB;
                Vector2 localCenterA = pc.LocalCenterA;
                Vector2 localCenterB = pc.LocalCenterB;
                int pointCount = pc.PointCount;

                var mA = 0.0f;
                var iA = 0.0f;
                if (indexA == toiIndexA || indexA == toiIndexB)
                {
                    mA = pc.InvMassA;
                    iA = pc.InvIa;
                }

                var mB = 0.0f;
                var iB = 0.0f;
                if (indexB == toiIndexA || indexB == toiIndexB)
                {
                    mB = pc.InvMassB;
                    iB = pc.InvIb;
                }

                Vector2 cA = _positions[indexA].C;
                float aA = _positions[indexA].A;

                Vector2 cB = _positions[indexB].C;
                float aB = _positions[indexB].A;

                // Solve normal constraints
                for (var j = 0; j < pointCount; ++j)
                {
                    var xfA = new PhysicsTransform();
                    var xfB = new PhysicsTransform();
                    xfA.Rotation.SetAngle(aA);
                    xfB.Rotation.SetAngle(aB);
                    xfA.Position = cA - xfA.RotateVector(localCenterA);
                    xfB.Position = cB - xfB.RotateVector(localCenterB);

                    InitializePositionManifold(pc, xfA, xfB, j, out Vector2 normal, out Vector2 point, out float separation);

                    Vector2 rA = point - cA;
                    Vector2 rB = point - cB;

                    // Track max constraint error.
                    minSeparation = Math.Min(minSeparation, separation);

                    // Prevent large corrections and allow slop.
                    float c = Maths.Clamp(PhysicsConfig.TimeOfImpactBaumgarte * (separation + PhysicsConfig.LinearSlop), -PhysicsConfig.MaxLinearCorrection, 0.0f);

                    // Compute the effective mass.
                    float rnA = Maths.Cross2D(rA, normal);
                    float rnB = Maths.Cross2D(rB, normal);
                    float k = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

                    // Compute normal impulse
                    float impulse = k > 0.0f ? -c / k : 0.0f;

                    Vector2 p = impulse * normal;

                    cA -= mA * p;
                    aA -= iA * Maths.Cross2D(rA, p);

                    cB += mB * p;
                    aB += iB * Maths.Cross2D(rB, p);
                }

                _positions[indexA].C = cA;
                _positions[indexA].A = aA;

                _positions[indexB].C = cB;
                _positions[indexB].A = aB;
            }

            // We can't expect minSpeparation >= -b2_linearSlop because we don't
            // push the separation above -b2_linearSlop.
            return minSeparation >= -1.5f * PhysicsConfig.LinearSlop;
        }
    }
}