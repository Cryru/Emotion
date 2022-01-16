#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    public class PhysicsIsland
    {
        public struct Position
        {
            public Vector2 C;
            public float A;
        }

        public struct Velocity
        {
            public Vector2 V;
            public float W;
        }

        private static float _maxTranslation = 2.0f;
        private static float _maxRotation = 0.5f * Maths.PI;
        private static float _linTolSqr = PhysicsConfig.LinearSleepTolerance * PhysicsConfig.LinearSleepTolerance;
        private static float _angTolSqr = PhysicsConfig.AngularSleepTolerance * PhysicsConfig.AngularSleepTolerance;
        private ContactSolver _contactSolver = new ContactSolver();

        private CollisionContact[] _contacts;

        internal PhysicsBody[] _bodies;
        internal int _bodyCount;
        internal int _bodyCapacity;
        internal int _contactCapacity;
        internal int _contactCount;

        private Position[] _positions;
        private Velocity[] _velocities;

        public void Reset(int bodyCapacity, int contactCapacity)
        {
            _bodyCapacity = bodyCapacity;
            _contactCapacity = contactCapacity;
            _bodyCount = 0;
            _contactCount = 0;

            if (_bodies == null || _bodies.Length < bodyCapacity)
            {
                _bodies = new PhysicsBody[bodyCapacity];
                _velocities = new Velocity[bodyCapacity];
                _positions = new Position[bodyCapacity];
            }

            if (_contacts == null || _contacts.Length < contactCapacity)
                _contacts = new CollisionContact[contactCapacity * 2];
        }

        public void Clear()
        {
            _bodyCount = 0;
            _contactCount = 0;
        }

        public void Solve(PhysicsTimeStepData step, Vector2 gravity)
        {
            float h = step.DeltaTime;

            // Integrate velocities and apply damping. Initialize the body state.
            for (var i = 0; i < _bodyCount; ++i)
            {
                PhysicsBody b = _bodies[i];

                Vector2 center = b.Sweep.CenterWorld;
                float rot = b.Sweep.Angle;
                Vector2 velocity = b.LinearVelocity;
                float rotVelocity = b.AngularVelocity;

                // Store positions for continuous collision.
                b.Sweep.CenterWorld0 = b.Sweep.CenterWorld;
                b.Sweep.Angle0 = b.Sweep.Angle;

                if (b.BodyType == BodyType.Dynamic)
                {
                    // Integrate velocities.
                    velocity += h * b.InvMass * (b.Mass * gravity + b.ActiveVelocity);
                    rotVelocity += h * b.InvInertia * b.ActiveTorque;

                    // Apply damping.
                    // ODE: dv/dt + c * v = 0
                    // Solution: v(t) = v0 * exp(-c * t)
                    // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
                    // v2 = exp(-c * dt) * v1
                    // Taylor expansion:
                    // v2 = (1.0f - c * dt) * v1
                    velocity *= Maths.Clamp01(1.0f - h * b.LinearDamping);
                    rotVelocity *= Maths.Clamp01(1.0f - h * b.AngularDamping);
                }

                _positions[i].C = center;
                _positions[i].A = rot;
                _velocities[i].V = velocity;
                _velocities[i].W = rotVelocity;
            }

            //Velcro: We reduce the amount of garbage by reusing the contactsolver and only resetting the state
            _contactSolver.Reset(step, _contactCount, _contacts, _positions, _velocities);
            _contactSolver.InitializeVelocityConstraints();
            _contactSolver.WarmStart();

            // Solve velocity constraints.
            for (var i = 0; i < PhysicsConfig.VelocityIterations; ++i)
            {
                _contactSolver.SolveVelocityConstraints();
            }

            // Store impulses for warm starting.
            _contactSolver.StoreImpulses();

            // Integrate positions
            for (var i = 0; i < _bodyCount; ++i)
            {
                Vector2 c = _positions[i].C;
                float a = _positions[i].A;
                Vector2 v = _velocities[i].V;
                float w = _velocities[i].W;

                // Check for large velocities
                Vector2 translation = h * v;
                if (Vector2.Dot(translation, translation) > _maxTranslation * _maxTranslation)
                {
                    float ratio = _maxTranslation / translation.Length();
                    v *= ratio;
                }

                float rotation = h * w;
                if (rotation * rotation > _maxRotation * _maxRotation)
                {
                    float ratio = _maxRotation / Math.Abs(rotation);
                    w *= ratio;
                }

                // Integrate
                c += h * v;
                a += h * w;

                _positions[i].C = c;
                _positions[i].A = a;
                _velocities[i].V = v;
                _velocities[i].W = w;
            }

            // Solve position constraints
            var positionSolved = false;
            for (var i = 0; i < PhysicsConfig.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolvePositionConstraints();
                if (contactsOkay)
                {
                    // Exit early if the position errors are small.
                    positionSolved = true;
                    break;
                }
            }

            // Copy state buffers back to the bodies
            for (var i = 0; i < _bodyCount; ++i)
            {
                PhysicsBody body = _bodies[i];
                body.Sweep.CenterWorld = _positions[i].C;
                body.Sweep.Angle = _positions[i].A;
                body.LinearVelocity = _velocities[i].V;
                body.AngularVelocity = _velocities[i].W;
                body.SynchronizeSweep();
            }

            // Put bodies to sleep.
            var minSleepTime = float.MaxValue;
            for (var i = 0; i < _bodyCount; ++i)
            {
                PhysicsBody b = _bodies[i];

                if (b.BodyType == BodyType.Static)
                    continue;

                if (b.AngularVelocity * b.AngularVelocity > _angTolSqr || Vector2.Dot(b.LinearVelocity, b.LinearVelocity) > _linTolSqr)
                {
                    b.SleepTime = 0.0f;
                    minSleepTime = 0.0f;
                }
                else
                {
                    b.SleepTime += h;
                    minSleepTime = Math.Min(minSleepTime, b.SleepTime);
                }
            }

            if (minSleepTime >= PhysicsConfig.TimeToSleep && positionSolved)
                for (var i = 0; i < _bodyCount; ++i)
                {
                    PhysicsBody b = _bodies[i];
                    b.Awake = false;
                }
        }

        public void SolveTimeOfImpact(PhysicsTimeStepData subStep, int toiIndexA, int toiIndexB)
        {
            Debug.Assert(toiIndexA < _bodyCount);
            Debug.Assert(toiIndexB < _bodyCount);

            // Initialize the body state.
            for (var i = 0; i < _bodyCount; ++i)
            {
                PhysicsBody b = _bodies[i];
                _positions[i].C = b.Sweep.CenterWorld;
                _positions[i].A = b.Sweep.Angle;
                _velocities[i].V = b.LinearVelocity;
                _velocities[i].W = b.AngularVelocity;
            }

            //Velcro: We reset the contact solver instead of creating a new one to reduce garbage
            _contactSolver.Reset(subStep, _contactCount, _contacts, _positions, _velocities, false);

            // Solve position constraints.
            for (var i = 0; i < subStep.PositionIterations; ++i)
            {
                bool contactsOkay = _contactSolver.SolveToIPositionConstraints(toiIndexA, toiIndexB);
                if (contactsOkay)
                    break;
            }

            // Leap of faith to new safe state.
            _bodies[toiIndexA].Sweep.CenterWorld0 = _positions[toiIndexA].C;
            _bodies[toiIndexA].Sweep.Angle0 = _positions[toiIndexA].A;
            _bodies[toiIndexB].Sweep.CenterWorld0 = _positions[toiIndexB].C;
            _bodies[toiIndexB].Sweep.Angle0 = _positions[toiIndexB].A;

            // No warm starting is needed for TOI events because warm
            // starting impulses were applied in the discrete solver.
            _contactSolver.InitializeVelocityConstraints();

            // Solve velocity constraints.
            for (var i = 0; i < subStep.VelocityIterations; ++i)
            {
                _contactSolver.SolveVelocityConstraints();
            }

            // Don't store the TOI contact forces for warm starting
            // because they can be quite large.
            float h = subStep.DeltaTime;

            // Integrate positions.
            for (var i = 0; i < _bodyCount; ++i)
            {
                Vector2 c = _positions[i].C;
                float a = _positions[i].A;
                Vector2 v = _velocities[i].V;
                float w = _velocities[i].W;

                // Check for large velocities
                Vector2 translation = h * v;
                if (Vector2.Dot(translation, translation) > _maxTranslation * _maxTranslation)
                {
                    float ratio = _maxTranslation / translation.Length();
                    v *= ratio;
                }

                float rotation = h * w;
                if (rotation * rotation > _maxRotation * _maxRotation)
                {
                    float ratio = _maxRotation / Math.Abs(rotation);
                    w *= ratio;
                }

                // Integrate
                c += h * v;
                a += h * w;

                _positions[i].C = c;
                _positions[i].A = a;
                _velocities[i].V = v;
                _velocities[i].W = w;

                // Sync bodies
                PhysicsBody body = _bodies[i];
                body.Sweep.CenterWorld = c;
                body.Sweep.Angle = a;
                body.LinearVelocity = v;
                body.AngularVelocity = w;
                body.SynchronizeSweep();
            }
        }

        public void Add(PhysicsBody body)
        {
            Debug.Assert(_bodyCount < _bodyCapacity);
            body.IslandIndex = _bodyCount;
            _bodies[_bodyCount++] = body;
        }

        public void Add(CollisionContact contact)
        {
            Debug.Assert(_contactCount < _contactCapacity);
            _contacts[_contactCount++] = contact;
        }
    }
}