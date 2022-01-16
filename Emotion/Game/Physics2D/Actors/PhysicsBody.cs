#region Using

using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Collision;
using Emotion.Game.Physics2D.Shape;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Actors
{
    /// <summary>
    /// A body being simulated.
    /// </summary>
    public class PhysicsBody
    {
        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very low CPU cost.
        /// </summary>
        public bool Awake
        {
            get => (Flags & BodyFlags.Awake) == BodyFlags.Awake;
            set
            {
                if (BodyType == BodyType.Static)
                    return;

                if (value)
                {
                    Flags |= BodyFlags.Awake;
                }
                else
                {
                    Flags &= ~BodyFlags.Awake;
                    ResetDynamics();
                }

                SleepTime = 0.0f;
            }
        }

        /// <summary>
        /// Timer to keep track of when to go to sleep.
        /// </summary>
        public float SleepTime { get; set; }

        /// <summary>
        /// The body type.
        /// </summary>
        public BodyType BodyType
        {
            get => _type;
            set
            {
                if (World != null)
                {
                    Debug.Assert(!World.IsLocked);
                    if (World.IsLocked)
                        return;
                }

                if (_type == value)
                    return;

                _type = value;

                RecalculateProperties();

                if (BodyType == BodyType.Static)
                {
                    _linearVelocity = Vector2.Zero;
                    _angularVelocity = 0.0f;
                    Sweep.Angle0 = Sweep.Angle;
                    Sweep.CenterWorld0 = Sweep.CenterWorld;
                    Flags &= ~BodyFlags.Awake;
                    UpdateLinkBounds();
                }

                Awake = true;

                ActiveVelocity = Vector2.Zero;
                ActiveTorque = 0.0f;

                // Delete the attached contacts.
                for (int i = ActiveContacts.Count - 1; i >= 0; i--)
                {
                    CollisionContact contact = ActiveContacts[i];
                    World.RemoveContact(contact);
                }

                ActiveContacts.Clear();
                ActiveContactsOtherBody.Clear();

                // Touch the proxies so that new contacts will be created (when appropriate)
                if (World == null) return;
                for (var i = 0; i < Links.Count; i++)
                {
                    PhysicsLink fixture = Links[i];
                    fixture.Moved();
                }
            }
        }

        protected BodyType _type;

        /// <summary>
        /// Collection of boolean flags that describe the body's state.
        /// </summary>
        public BodyFlags Flags = BodyFlags.Enabled | BodyFlags.Awake;

        #region Flags

        /// <summary>Gets or sets a value indicating whether this body should be included in the CCD solver.</summary>
        /// <value><c>true</c> if this instance is included in CCD; otherwise, <c>false</c>.</value>
        public bool IsBullet
        {
            get => (Flags & BodyFlags.Bullet) == BodyFlags.Bullet;
            set
            {
                if (value)
                    Flags |= BodyFlags.Bullet;
                else
                    Flags &= ~BodyFlags.Bullet;
            }
        }

        /// <summary>
        /// The active state of the body. An inactive body is not simulated and cannot be collided with or woken up.
        /// </summary>
        public bool Enabled
        {
            get => (Flags & BodyFlags.Enabled) == BodyFlags.Enabled;

            set
            {
                Debug.Assert(!World.IsLocked);

                if (value == Enabled)
                    return;

                if (value)
                {
                    Flags |= BodyFlags.Enabled;

                    // Create all proxies.
                    for (var i = 0; i < Links.Count; i++)
                    {
                        World.AddLink(Links[i]);
                    }

                    // Contacts are created the next time step.
                    World.PossibleNewContacts();
                }
                else
                {
                    Flags &= ~BodyFlags.Enabled;

                    // Destroy all proxies.
                    for (var i = 0; i < Links.Count; i++)
                    {
                        World.RemoveLink(Links[i]);
                    }

                    // Destroy the attached contacts.
                    for (int i = ActiveContacts.Count - 1; i >= 0; i--)
                    {
                        CollisionContact contact = ActiveContacts[i];
                        World.RemoveContact(contact);
                    }

                    ActiveContacts.Clear();
                    ActiveContactsOtherBody.Clear();
                }
            }
        }

        /// <summary>
        /// Bodies with fixed rotation will not rotate regardless of collisions.
        /// </summary>
        public bool FixedRotation
        {
            get => (Flags & BodyFlags.FixedRotation) == BodyFlags.FixedRotation;
            set
            {
                if (value == FixedRotation)
                    return;

                if (value)
                    Flags |= BodyFlags.FixedRotation;
                else
                    Flags &= ~BodyFlags.FixedRotation;

                _angularVelocity = 0f;
                RecalculateProperties();
            }
        }

        #endregion

        #region Dynamics

        /// <summary>
        /// The linear velocity of the center of mass.
        /// </summary>
        public Vector2 LinearVelocity
        {
            get => _linearVelocity;
            set
            {
                if (BodyType == BodyType.Static)
                    return;

                if (Vector2.Dot(value, value) > 0.0f)
                    Awake = true;

                _linearVelocity = value;
            }
        }

        protected Vector2 _linearVelocity;

        /// <summary>
        /// Angular velocity - Radians/second.
        /// </summary>
        public float AngularVelocity
        {
            get => _angularVelocity;
            set
            {
                if (BodyType == BodyType.Static)
                    return;

                if (value * value > 0.0f)
                    Awake = true;

                _angularVelocity = value;
            }
        }

        protected float _angularVelocity;

        /// <summary>
        /// Loss of linear velocity over time.
        /// </summary>
        public float LinearDamping { get; set; }

        /// <summary>
        /// Loss of angular velocity over time.
        /// </summary>
        public float AngularDamping { get; set; }

        #endregion

        /// <summary>
        /// Mass, in kilograms (kg).
        /// </summary>
        public float Mass
        {
            get => _mass;
            set
            {
                Debug.Assert(!float.IsNaN(value));

                if (BodyType != BodyType.Dynamic)
                    return;

                if (value <= 0.0f)
                    value = 1.0f;
                _mass = value;

                InvMass = 1.0f / _mass;
            }
        }

        protected float _mass;

        /// <summary>
        /// The rotational inertia of the body about the local origin. usually in kg-m^2.
        /// </summary>
        public float Inertia
        {
            get => _inertia + _mass * Vector2.Dot(Sweep.LocalCenter, Sweep.LocalCenter);
            set
            {
                Debug.Assert(!float.IsNaN(value));

                if (BodyType != BodyType.Dynamic)
                    return;

                if (value > 0.0f && !FixedRotation)
                {
                    _inertia = value - _mass * Vector2.Dot(Sweep.LocalCenter, Sweep.LocalCenter);
                    Debug.Assert(_inertia > 0.0f);
                    InvInertia = 1.0f / _inertia;
                }
            }
        }

        protected float _inertia;

        public float InvInertia { get; protected set; }
        public float InvMass { get; protected set; }
        public Vector2 ActiveVelocity { get; protected set; }
        public float ActiveTorque { get; protected set; }

        // todo: remove
        public int IslandIndex;

        public bool IsIsland
        {
            get => Flags.HasFlag(BodyFlags.Island);
        }

        /// <summary>
        /// The world this body is in.
        /// </summary>
        public PhysicsWorld World;

        /// <summary>
        /// The unique id of this body within the world.
        /// </summary>
        public uint WorldId;

        /// <summary>
        /// The swept motion by the continuous collision detection.
        /// </summary>
        public Sweep Sweep;

        /// <summary>
        /// The body's origin transform.
        /// </summary>
        public PhysicsTransform BodyTransform = new PhysicsTransform();

        /// <summary>
        /// List of collision contacts this body is part of.
        /// </summary>
        public List<CollisionContact> ActiveContacts = new List<CollisionContact>();

        /// <summary>
        /// List of bodies each contact is with.
        /// Used to differentiate which body this is, within the contact.
        /// </summary>
        public List<PhysicsBody> ActiveContactsOtherBody = new List<PhysicsBody>();

        /// <summary>
        /// List of colliders this body owns.
        /// </summary>
        public List<PhysicsLink> Links = new List<PhysicsLink>(1);

        /// <summary>Get the world body origin position.</summary>
        /// <returns>Return the world position of the body's origin.</returns>
        public Vector2 Position
        {
            get => BodyTransform.Position;
            set => SetPositionAndRotation(value, Sweep.Angle);
        }

        /// <summary>Get the angle in radians.</summary>
        /// <returns>Return the current world rotation angle in radians.</returns>
        public float Rotation
        {
            get => Sweep.Angle;
            set => SetPositionAndRotation(BodyTransform.Position, value);
        }

        /// <summary>
        /// The world position of the center of mass.
        /// </summary>
        public Vector2 WorldCenter
        {
            get => Sweep.CenterWorld;
        }

        /// <summary>
        /// The local position of the center of mass.
        /// </summary>
        public Vector2 LocalCenter
        {
            get => Sweep.LocalCenter;
            set
            {
                if (BodyType != BodyType.Dynamic)
                    return;

                // Move center of mass.
                Vector2 oldCenter = Sweep.CenterWorld;
                Sweep.LocalCenter = value;
                Sweep.CenterWorld0 = Sweep.CenterWorld = BodyTransform.TransformVector(Sweep.LocalCenter);

                // Update center of mass velocity.
                Vector2 a = Sweep.CenterWorld - oldCenter;
                _linearVelocity += new Vector2(-_angularVelocity * a.Y, _angularVelocity * a.X);
            }
        }

        protected PhysicsBody()
        {
        }


        /// <summary>
        /// Resets the dynamics of this body.
        /// </summary>
        public void ResetDynamics()
        {
            ActiveTorque = 0;
            _angularVelocity = 0;
            ActiveVelocity = Vector2.Zero;
            _linearVelocity = Vector2.Zero;
        }

        public void ResetActiveForces()
        {
            ActiveVelocity = Vector2.Zero;
            ActiveTorque = 0;
        }

        #region Contacts

        public void AddContact(CollisionContact contact, PhysicsBody otherBody)
        {
            ActiveContacts.Add(contact);
            ActiveContactsOtherBody.Add(otherBody);
        }

        public void RemoveContact(CollisionContact contact)
        {
            int idx = ActiveContacts.IndexOf(contact);
            ActiveContacts.RemoveAt(idx);
            ActiveContactsOtherBody.RemoveAt(idx);
        }

        #endregion

        #region Links

        public PhysicsLink AddLink(ShapeBase shape)
        {
            if (World != null)
            {
                Debug.Assert(!World.IsLocked);
                if (World.IsLocked) return null;
            }

            var link = new PhysicsLink(this, shape);
            Links.Add(link);
            if (Flags.HasFlag(BodyFlags.Enabled) && World != null) World.AddLink(link);
            if (shape.Density > 0.0f) RecalculateProperties();

            World?.PossibleNewContacts();
            return link;
        }

        /// <summary>
        /// Destroy a collider. This will automatically readjust the mass of the body.
        /// </summary>
        /// <param name="link">The fixture to be removed.</param>
        public void RemoveLink(PhysicsLink link)
        {
            Debug.Assert(!World.IsLocked);
            if (World.IsLocked)
                return;

            if (link == null)
                return;

            Debug.Assert(link.Body == this);
            Debug.Assert(Links.Contains(link));

            // Destroy any contacts associated.
            for (int i = ActiveContacts.Count - 1; i >= 0; i--)
            {
                CollisionContact c = ActiveContacts[i];

                PhysicsLink linkA = c.LinkA;
                PhysicsLink linkB = c.LinkB;

                // This destroys the contact and removes it from
                // this body's contact list.
                if (link == linkA || link == linkB)
                    World.RemoveContact(c);
            }

            if (Flags.HasFlag(BodyFlags.Enabled))
            {
                PhysicsWorld broadPhase = World;
                broadPhase.RemoveLink(link);
            }

            Links.Remove(link);
            link.Dispose();

            RecalculateProperties();
        }

        /// <summary>
        /// Update the bounds of all colliders with the body's position and data.
        /// </summary>
        public void UpdateLinkBounds()
        {
            for (var i = 0; i < Links.Count; i++)
            {
                Links[i].UpdateBounds();
            }
        }

        #endregion

        /// <summary>
        /// Returns whether this body can collide with another.
        /// </summary>
        public bool CanCollideWith(PhysicsBody other)
        {
            // Can't collide with self.
            if (other == this)
                return false;

            // At least one body should be dynamic.
            if (BodyType != BodyType.Dynamic && other.BodyType != BodyType.Dynamic)
                return false;

            return true;
        }

        /// <summary>
        /// Advance to the new safe time. This doesn't sync the broad-phase.
        /// </summary>
        public void AdvanceSweep(float alpha)
        {
            Sweep.Advance(alpha);
            Sweep.CenterWorld = Sweep.CenterWorld0;
            Sweep.Angle = Sweep.Angle0;
            BodyTransform.Rotation.SetAngle(Sweep.Angle);
            BodyTransform.Position = Sweep.CenterWorld - BodyTransform.RotateVector(Sweep.LocalCenter);
        }

        /// <summary>
        /// Synchronizes the transform from the sweep.
        /// </summary>
        public void SynchronizeSweep()
        {
            Sweep.SetTransform(BodyTransform, 1.0f);
        }

        /// <summary>
        /// This resets the mass properties to the sum of the mass properties of the fixtures. This normally does not need
        /// to be called unless you called SetMassData to override the mass and you later want to reset the mass.
        /// </summary>
        protected void RecalculateProperties()
        {
            // Compute mass data from shapes. Each shape has its own density.
            _mass = 0.0f;
            InvMass = 0.0f;
            _inertia = 0.0f;
            InvInertia = 0.0f;
            Sweep.LocalCenter = Vector2.Zero;

            // Kinematic bodies have zero mass.
            if (BodyType == BodyType.Kinematic)
            {
                Sweep.CenterWorld0 = BodyTransform.Position;
                Sweep.CenterWorld = BodyTransform.Position;
                Sweep.Angle0 = Sweep.Angle;
                return;
            }

            Debug.Assert(BodyType == BodyType.Dynamic || BodyType == BodyType.Static);

            // Accumulate mass over all fixtures.
            Vector2 localCenter = Vector2.Zero;
            foreach (PhysicsLink f in Links)
            {
                if (f.Shape.Density == 0.0f)
                    continue;

                MassData massData = f.Shape.MassData;
                _mass += massData.Mass;
                localCenter += massData.Mass * massData.Centroid;
                _inertia += massData.Inertia;
            }

            // From Velcro: Static bodies have mass so joints can be attached to them.
            if (BodyType == BodyType.Static)
            {
                Sweep.CenterWorld0 = Sweep.CenterWorld = BodyTransform.Position;
                return;
            }

            // Compute center of mass.
            if (_mass > 0.0f)
            {
                InvMass = 1.0f / _mass;
                localCenter *= InvMass;
            }

            if (_inertia > 0.0f && (Flags & BodyFlags.FixedRotation) == 0)
            {
                // Center the inertia about the center of mass.
                _inertia -= _mass * Vector2.Dot(localCenter, localCenter);

                Debug.Assert(_inertia > 0.0f);
                InvInertia = 1.0f / _inertia;
            }
            else
            {
                _inertia = 0.0f;
                InvInertia = 0.0f;
            }

            // Move center of mass.
            Vector2 oldCenter = Sweep.CenterWorld;
            Sweep.LocalCenter = localCenter;
            Sweep.CenterWorld0 = Sweep.CenterWorld = BodyTransform.TransformVector(Sweep.LocalCenter);

            // Update center of mass velocity.
            Vector2 a = Sweep.CenterWorld - oldCenter;
            _linearVelocity += new Vector2(-_angularVelocity * a.Y, _angularVelocity * a.X);
        }

        /// <summary>
        /// Set the position of the body's origin and rotation. This breaks any contacts and wakes the other bodies.
        /// Manipulating a body's transform may cause non-physical behavior.
        /// </summary>
        /// <param name="position">The world position of the body's local origin.</param>
        /// <param name="rotation">The world rotation in radians.</param>
        public void SetPositionAndRotation(Vector2 position, float rotation)
        {
            if (World != null)
            {
                Debug.Assert(!World.IsLocked);
                if (World.IsLocked)
                    return;
            }

            BodyTransform.Rotation.SetAngle(rotation);
            BodyTransform.Position = position;

            Sweep.CenterWorld = BodyTransform.TransformVector(Sweep.LocalCenter);
            Sweep.Angle = rotation;

            Sweep.CenterWorld0 = Sweep.CenterWorld;
            Sweep.Angle0 = rotation;

            for (var i = 0; i < Links.Count; i++)
            {
                Links[i].UpdateBounds();
            }

            // Check for new contacts the next step
            World?.PossibleNewContacts();
        }

        /// <summary>
        /// Applies a force at the center of mass
        /// </summary>
        public void ApplyForce(Vector2 force)
        {
            ApplyForce(force, BodyTransform.Position);
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not applied at the center of mass, it will generate a torque
        /// and affect the angular velocity. This wakes up the body.
        /// </summary>
        /// <param name="force">The world force vector, usually in Newtons (N).</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyForce(Vector2 force, Vector2 point)
        {
            Debug.Assert(!float.IsNaN(force.X));
            Debug.Assert(!float.IsNaN(force.Y));
            Debug.Assert(!float.IsNaN(point.X));
            Debug.Assert(!float.IsNaN(point.Y));

            if (BodyType != BodyType.Dynamic)
                return;

            if (!Awake)
                Awake = true;

            ActiveVelocity += force;
            ActiveTorque += Maths.Cross2D(point - Sweep.CenterWorld, force);
        }

        /// <summary>
        /// Apply a torque (rotational force). Usually in N-m
        /// This affects the angular velocity without affecting the linear velocity of the center of mass.
        /// </summary>
        public void ApplyTorque(float torque)
        {
            Debug.Assert(!float.IsNaN(torque));

            if (BodyType != BodyType.Dynamic)
                return;

            if (!Awake)
                Awake = true;

            ActiveTorque += torque;
        }

        /// <summary>Apply an impulse at a point. This immediately modifies the velocity. This wakes up the body.</summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        public void ApplyLinearImpulse(Vector2 impulse)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            if (!Awake)
                Awake = true;

            _linearVelocity += InvMass * impulse;
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity. It also modifies the angular velocity if
        /// the point of application is not at the center of mass. This wakes up the body.
        /// </summary>
        /// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
        /// <param name="point">The world position of the point of application.</param>
        public void ApplyLinearImpulse(Vector2 impulse, Vector2 point)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            if (!Awake)
                Awake = true;

            _linearVelocity += InvMass * impulse;
            _angularVelocity += InvInertia * Maths.Cross2D(point - Sweep.CenterWorld, impulse);
        }

        /// <summary>Apply an angular impulse.</summary>
        /// <param name="impulse">The angular impulse in units of kg*m*m/s.</param>
        public void ApplyAngularImpulse(float impulse)
        {
            if (BodyType != BodyType.Dynamic)
                return;

            if (!Awake)
                Awake = true;

            _angularVelocity += InvInertia * impulse;
        }

        /// <summary>Get the world coordinates of a point given the local coordinates.</summary>
        /// <param name="localPoint">A point on the body measured relative the body's origin.</param>
        /// <returns>The same point expressed in world coordinates.</returns>
        public Vector2 GetWorldPoint(Vector2 localPoint)
        {
            return BodyTransform.TransformVector(localPoint);
        }

        /// <summary>
        /// Get the world coordinates of a vector given the local coordinates. Note that the vector only takes the
        /// rotation into account, not the position.
        /// </summary>
        /// <param name="localVector">A vector fixed in the body.</param>
        /// <returns>The same vector expressed in world coordinates.</returns>
        public Vector2 GetWorldVector(Vector2 localVector)
        {
            return BodyTransform.RotateVector(localVector);
        }

        /// <summary>
        /// Gets a local point relative to the body's origin given a world point. Note that the vector only takes the
        /// rotation into account, not the position.
        /// </summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The corresponding local point relative to the body's origin.</returns>
        public Vector2 GetLocalPoint(Vector2 worldPoint)
        {
            return BodyTransform.TransformTransposeVector(worldPoint);
        }

        /// <summary>
        /// Gets a local vector given a world vector. Note that the vector only takes the rotation into account, not the
        /// position.
        /// </summary>
        /// <param name="worldVector">A vector in world coordinates.</param>
        /// <returns>The corresponding local vector.</returns>
        public Vector2 GetLocalVector(Vector2 worldVector)
        {
            return BodyTransform.RotateTransposeVector(worldVector);
        }

        /// <summary>Get the world linear velocity of a world point attached to this body.</summary>
        /// <param name="worldPoint">A point in world coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint)
        {
            return _linearVelocity + Maths.Cross2D(_angularVelocity, worldPoint - Sweep.CenterWorld);
        }

        /// <summary>Get the world velocity of a local point.</summary>
        /// <param name="localPoint">A point in local coordinates.</param>
        /// <returns>The world velocity of a point.</returns>
        public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
        }

        public void Dispose()
        {
            // Delete the attached contacts.
            for (int i = ActiveContacts.Count - 1; i >= 0; i--)
            {
                CollisionContact contact = ActiveContacts[i];
                World.RemoveContact(contact);
            }

            Debug.Assert(ActiveContacts.Count == 0);
            ActiveContacts.Clear();
            ActiveContactsOtherBody.Clear();

            // Delete the attached fixtures. This destroys broad-phase proxies.
            for (var i = 0; i < Links.Count; i++)
            {
                PhysicsLink fixture = Links[i];
                World.RemoveLink(fixture);
                fixture.Dispose();
            }

            Links = null;
            World = null;
        }

        #region Prefabs

        public static PhysicsBody CreateRectangle(Vector2 center, Vector2 size, BodyType type, float rotation = 0, float density = 1)
        {
            var body = new PhysicsBody
            {
                Position = center,
                BodyType = type
            };
            body.BodyTransform.Rotation.SetAngle(rotation);

            // Create a rectangle polygon centered at 0,0 (which in local space would be the body's position)
            Polygon rectanglePolygon = Polygon.FromRectangle(new Rectangle(0, 0, size) {Center = Vector2.Zero});

            // Create shape definition.
            var polygonShape = new PolygonShape(rectanglePolygon, density);
            body.AddLink(polygonShape);

            return body;
        }

        #endregion
    }
}