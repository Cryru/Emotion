#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Collision;
using Emotion.Game.QuadTree;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D
{
    public class PhysicsWorld
    {
#if DEBUG
        public static PhysicsWorld LastUpdatedWorld;
#endif

        /// <summary>
        /// The world is locked during parts of the simulation. While locked no
        /// bodies can be added, instead they will be queued to be added later.
        /// While locked some other things such as links cannot be added.
        /// </summary>
        public bool IsLocked;

        protected HashSet<PhysicsBody> _bodyAddList = new HashSet<PhysicsBody>();
        protected HashSet<PhysicsBody> _bodyRemoveList = new HashSet<PhysicsBody>();

        /// <summary>
        /// List of all bodies. Do not mutate directly.
        /// </summary>
        public List<PhysicsBody> Bodies = new List<PhysicsBody>();

        /// <summary>
        /// List of all moving colliders. Do not mutate directly.
        /// </summary>
        public List<PhysicsLink> MovingLinks = new List<PhysicsLink>();

        public HashSet<PhysicsLink> MovingLinkHash = new HashSet<PhysicsLink>();

        /// <summary>
        /// List of currently active broadphase collision contacts. do not mutate directly.
        /// </summary>
        public List<CollisionContact> ActiveContacts = new List<CollisionContact>();

        public QuadTree<PhysicsLink> BroadPhaseCollision = new QuadTree<PhysicsLink>(Rectangle.Empty);
        protected QuadTreeQuery<PhysicsLink> _overlappingQuery = new QuadTreeQuery<PhysicsLink> {Results = new List<PhysicsLink>()};
        protected ObjectPool<CollisionContact> _contacts = new ObjectPool<CollisionContact>();

        // todo
        public Vector2 Gravity;

        /// <summary>
        /// This will cause the world to check for newly made contacts in the next step as
        /// new actors/links and such could have appeared from anywhere.
        /// </summary>
        private bool _newContacts = true;

        private PhysicsTimeStepData _stepData = new PhysicsTimeStepData();
        private uint _nextBodyId;
        private PhysicsIsland _island = new PhysicsIsland();

        public PhysicsWorld(Vector2 gravity) // todo: Should gravity be a force that is added?
        {
            Gravity = gravity;
        }

        /// <summary>
        /// Advance the physics simulation by the specified amount of seconds.
        /// </summary>
        public void Step(float dtSeconds)
        {
            LastUpdatedWorld = this;

            // Add any bodies and colliders which were
            // added in the previous step while the world was locked.
            ProcessPending();

            // Broad-phase collision
            // Find any bodies that are within bounding box range of each other.
            if (_newContacts)
            {
                FindNewContacts();
                _newContacts = false;
            }

            IsLocked = true;

            // Narrow-phase collision
            // Process all ongoing contacts.
            // Find precise collision manifolds for those which are touching.
            ProcessContacts();

            // Collision Resolve
            // Integrate velocities, solve velocity constraints, and integrate positions.
            _stepData.DeltaTime = dtSeconds;
            _stepData.DeltaTimeRatio = _stepData.InvertedDeltaTime0 * dtSeconds;

            // Integrate velocities, solve velocity constraints, and integrate positions.
            if (_stepData.DeltaTime > 0.0f) SolveCollisions();

            // Handle TOI events.
            if (_stepData.DeltaTime > 0.0f) SolveTimeOfImpact();

            // Cleanup
            if (_stepData.DeltaTime > 0.0f)
                _stepData.InvertedDeltaTime0 = _stepData.InvertedDeltaTime;

            for (var i = 0; i < Bodies.Count; i++)
            {
                PhysicsBody body = Bodies[i];
                body.ResetActiveForces();
            }

            IsLocked = false;
        }

        #region Actors

        public void AddBody(PhysicsBody body)
        {
            if (IsLocked)
            {
                Debug.Assert(!_bodyAddList.Contains(body), "You are adding the same body more than once.");

                if (!_bodyAddList.Contains(body))
                    _bodyAddList.Add(body);
            }
            else
            {
                AddBodyInternal(body);
            }
        }

        public void RemoveBody(PhysicsBody body)
        {
            if (IsLocked)
            {
                Debug.Assert(!_bodyAddList.Contains(body), "You are removing the same body twice.");

                if (!_bodyRemoveList.Contains(body))
                    _bodyRemoveList.Add(body);
            }
            else
            {
                RemoveBodyInternal(body);
            }
        }

        private void AddBodyInternal(PhysicsBody body)
        {
            body.World = this;
            body.WorldId = _nextBodyId;
            _nextBodyId++;
            Bodies.Add(body);

            for (var i = 0; i < body.Links.Count; i++)
            {
                PhysicsLink link = body.Links[i];
                AddLink(link);
            }

            PossibleNewContacts();
        }

        private void RemoveBodyInternal(PhysicsBody body)
        {
            Debug.Assert(Bodies.Contains(body));
            body.Dispose();
            Bodies.Remove(body);
        }

        private void ProcessPending()
        {
            // Process additions.
            if (_bodyAddList.Count != 0)
            {
                foreach (PhysicsBody body in _bodyAddList)
                {
                    AddBodyInternal(body);
                }

                _bodyAddList.Clear();
            }

            // Process removals.
            if (_bodyRemoveList.Count != 0)
            {
                foreach (PhysicsBody body in _bodyRemoveList)
                {
                    RemoveBodyInternal(body);
                }

                _bodyRemoveList.Clear();
            }
        }

        public void AddLink(PhysicsLink link)
        {
            Debug.Assert(!BroadPhaseCollision.GetAllObjects().Contains(link));
            BroadPhaseCollision.Add(link);
            MovingLinks.Add(link);
        }

        public void RemoveLink(PhysicsLink link)
        {
            BroadPhaseCollision.Remove(link);
            if (MovingLinks.Contains(link)) MovingLinks.Remove(link);
        }

        #endregion

        #region Broad-Phase Collision

        public void PossibleNewContacts()
        {
            _newContacts = true;
        }

        protected void FindNewContacts()
        {
            for (var i = 0; i < MovingLinks.Count; i++)
            {
                PhysicsLink moving = MovingLinks[i];
                Rectangle bound = moving.GetBoundsForQuadTree();
                _overlappingQuery.SearchArea = bound;
                _overlappingQuery.Results.Clear();
                BroadPhaseCollision.ExecuteQuery(_overlappingQuery);

                for (var j = 0; j < _overlappingQuery.Results.Count; j++)
                {
                    PhysicsLink other = _overlappingQuery.Results[j];

                    // Cant overlap with self.
                    if (other == moving) continue;

                    // Avoid duplicates.
                    int otherMovedIdx = MovingLinks.IndexOf(other);
                    if (otherMovedIdx != -1 && otherMovedIdx > i) continue;

                    // Check if intersecting.
                    if (!other.GetBoundsForQuadTree().Intersects(ref bound))
                        continue;

                    // Shapes within one body don't collide with each other.
                    PhysicsBody bOne = moving.Body;
                    PhysicsBody bTwo = other.Body;

                    if (!bOne.CanCollideWith(bTwo) || !bTwo.CanCollideWith(bOne)) continue;

                    // Check if contact already exists.
                    var contactExists = false;
                    List<CollisionContact> bodyContacts = bOne.ActiveContacts;
                    for (var c = 0; c < bodyContacts.Count; c++)
                    {
                        CollisionContact existingContact = bodyContacts[c];
                        PhysicsBody existingContactWith = bOne.ActiveContactsOtherBody[c];

                        if (existingContactWith == bTwo)
                        {
                            PhysicsLink linkA = existingContact.LinkA;
                            PhysicsLink linkB = existingContact.LinkB;

                            if (linkA == moving && linkB == other || linkA == other && linkB == moving)
                            {
                                contactExists = true;
                                break;
                            }
                        }
                    }

                    if (contactExists) continue;

                    // Add contact to world.
                    CollisionContact contact = EstablishContact(moving, other);
                    ActiveContacts.Add(contact);

                    // Add contact to bodies.
                    bOne.AddContact(contact, bTwo);
                    bTwo.AddContact(contact, bOne);
                }
            }

            MovingLinks.Clear();
            MovingLinkHash.Clear();
        }

        protected CollisionContact EstablishContact(PhysicsLink linkA, PhysicsLink linkB)
        {
            CollisionContact contact = _contacts.Get();
            contact.Set(linkA, linkB);
            return contact;
        }

        public void RemoveContact(CollisionContact contact)
        {
            ActiveContacts.Remove(contact);

            PhysicsBody bodyA = contact.LinkA.Body;
            bodyA.RemoveContact(contact);
            PhysicsBody bodyB = contact.LinkB.Body;
            bodyB.RemoveContact(contact);

            _contacts.Return(contact);
        }

        protected void ProcessContacts()
        {
            for (int i = ActiveContacts.Count - 1; i >= 0; i--)
            {
                CollisionContact contact = ActiveContacts[i];
                PhysicsLink linkA = contact.LinkA;
                PhysicsLink linkB = contact.LinkB;
                PhysicsBody bodyA = linkA.Body;
                PhysicsBody bodyB = linkB.Body;

                // Don't process disabled body contacts.
                if (!bodyA.Flags.HasFlag(BodyFlags.Enabled) || !bodyB.Flags.HasFlag(BodyFlags.Enabled)) continue;

                // Re-filter, if needed.
                if (contact.Flags.HasFlag(ContactFlags.FilterNeeded))
                    // todo: call body should collide to recheck.
                    // todo: this can destroy contacts.

                    contact.Flags &= ~ContactFlags.FilterNeeded;

                // At least one of the bodies must be awake, and it shouldn't be a static one.
                bool activeA = bodyA.Awake && bodyA.BodyType != BodyType.Static;
                bool activeB = bodyB.Awake && bodyB.BodyType != BodyType.Static;
                if (!activeA && !activeB) continue;

                // Check if still intersecting.
                bool stillIntersecting = linkA.GetBoundsForQuadTree().Intersects(linkB.GetBoundsForQuadTree());
                if (!stillIntersecting)
                {
                    RemoveContact(contact);
                    continue;
                }

                contact.Update();
            }
        }

        #endregion

        protected Stack<PhysicsBody> _solveCollisionsStack = new Stack<PhysicsBody>();
        protected HashSet<PhysicsBody> _islandMarkedBodies = new HashSet<PhysicsBody>();
        protected HashSet<CollisionContact> _islandMarkedContacts = new HashSet<CollisionContact>();

        private void SolveCollisions()
        {
            // Size the island for the worst case.
            _island.Reset(Bodies.Count, ActiveContacts.Count);

            // Reset island markers.
            _solveCollisionsStack.Clear();

            // Clear all the island flags.
            for (var i = 0; i < Bodies.Count; i++)
            {
                PhysicsBody b = Bodies[i];
                b.Flags &= ~BodyFlags.Island;
            }

            for (var i = 0; i < ActiveContacts.Count; i++)
            {
                CollisionContact contact = ActiveContacts[i];
                contact.Flags &= ~ContactFlags.Island;
            }

            // Build and simulate all awake islands.
            for (int index = Bodies.Count - 1; index >= 0; index--)
            {
                PhysicsBody seed = Bodies[index];
                if (seed.Flags.HasFlag(BodyFlags.Island))
                    continue;

                if (!seed.Awake || !seed.Flags.HasFlag(BodyFlags.Enabled))
                    continue;

                // The seed must be dynamic or kinematic.
                if (seed.BodyType == BodyType.Static)
                    continue;

                // Reset island and stack.
                _island.Clear();
                _solveCollisionsStack.Push(seed);

                seed.Flags |= BodyFlags.Island;

                // Perform a depth first search (DFS) on the constraint graph.
                while (_solveCollisionsStack.Count > 0)
                {
                    // Grab the next body off the stack and add it to the island.
                    PhysicsBody b = _solveCollisionsStack.Pop();
                    Debug.Assert(b.Flags.HasFlag(BodyFlags.Enabled));
                    _island.Add(b);

                    // To keep islands as small as possible, we don't
                    // propagate islands across static bodies.
                    if (b.BodyType == BodyType.Static)
                        continue;

                    // Make sure the body is awake (without resetting sleep timer).
                    b.Flags |= BodyFlags.Awake;

                    // Search all contacts connected to this body.
                    for (var ce = 0; ce < b.ActiveContacts.Count; ce++)
                    {
                        CollisionContact contact = b.ActiveContacts[ce];

                        // Has this contact already been added to an island?
                        if (contact.Flags.HasFlag(ContactFlags.Island))
                            continue;

                        // Is this contact touching?
                        if (!contact.Flags.HasFlag(ContactFlags.Enabled) || !contact.Flags.HasFlag(ContactFlags.Touching))
                            continue;

                        // todo: skip triggers/sensors

                        _island.Add(contact);
                        contact.Flags |= ContactFlags.Island;

                        PhysicsBody other = b.ActiveContactsOtherBody[ce];

                        // Was the other body already added to this island?
                        if (other.Flags.HasFlag(BodyFlags.Island))
                            continue;

                        other.Flags |= BodyFlags.Island;
                        _solveCollisionsStack.Push(other);
                    }
                }

                _island.Solve(_stepData, Gravity);

                // Post solve cleanup.
                // Allow static bodies to participate in other islands.
                for (var i = 0; i < _island._bodyCount; ++i)
                {
                    PhysicsBody b = _island._bodies[i];
                    if (b.BodyType == BodyType.Static) b.Flags &= ~BodyFlags.Island;
                }
            }

            // Update state for broad phase.
            for (var i = 0; i < Bodies.Count; i++)
            {
                PhysicsBody body = Bodies[i];

                if (body.BodyType == BodyType.Static) continue;

                // If body wasn't in an island, it did not move.
                if (!body.Flags.HasFlag(BodyFlags.Island)) continue;

                for (var j = 0; j < body.Links.Count; j++)
                {
                    PhysicsLink link = body.Links[j];
                    link.UpdateBounds();
                }
            }

            // Look for new contacts.
            FindNewContacts();
        }

        private void SolveTimeOfImpact()
        {
            _island.Reset(2 * PhysicsConfig.MaxTimeOfImpactContacts, PhysicsConfig.MaxTimeOfImpactContacts);

            for (var i = 0; i < Bodies.Count; i++)
            {
                PhysicsBody body = Bodies[i];
                body.Flags &= ~BodyFlags.Island;
                body.Sweep.Alpha0 = 0f;
            }

            for (var i = 0; i < ActiveContacts.Count; i++)
            {
                CollisionContact contact = ActiveContacts[i];
                contact.Flags &= ~(ContactFlags.TimeOfImpact | ContactFlags.Island);
                contact.TimeOfImpactCount = 0;
                contact.TimeOfImpact = 1.0f;
            }

            // Find TOI events and solve them.
            while (true)
            {
                // Find the first TOI.
                CollisionContact minContact = null;
                var minAlpha = 1.0f;

                for (var i = 0; i < ActiveContacts.Count; i++)
                {
                    CollisionContact c = ActiveContacts[i];

                    // Is this contact disabled?
                    if (!c.Flags.HasFlag(ContactFlags.Enabled))
                        continue;

                    // Prevent excessive sub-stepping.
                    if (c.TimeOfImpactCount > PhysicsConfig.MaxSubSteps)
                        continue;

                    float alpha;
                    if (c.Flags.HasFlag(ContactFlags.TimeOfImpact))
                    {
                        // This contact has a valid cached TOI.
                        alpha = c.TimeOfImpact;
                    }
                    else
                    {
                        PhysicsLink fA = c.LinkA;
                        PhysicsLink fB = c.LinkB;

                        // todo: skip triggers/sensors

                        PhysicsBody bA = fA.Body;
                        PhysicsBody bB = fB.Body;

                        BodyType typeA = bA.BodyType;
                        BodyType typeB = bB.BodyType;
                        Debug.Assert(typeA == BodyType.Dynamic || typeB == BodyType.Dynamic);

                        bool activeA = bA.Awake && typeA != BodyType.Static;
                        bool activeB = bB.Awake && typeB != BodyType.Static;

                        // Is at least one body active (awake and dynamic or kinematic)?
                        if (!activeA && !activeB)
                            continue;

                        Debug.Assert(bA.CanCollideWith(bB) && bB.CanCollideWith(bA));
                        //  (fA.IgnoreCcdWith & fB.CollisionCategories) == 0
                        bool collideA = bA.Flags.HasFlag(BodyFlags.Bullet) || typeA != BodyType.Dynamic;
                        bool collideB = bB.Flags.HasFlag(BodyFlags.Bullet) || typeB != BodyType.Dynamic;

                        // Are these two non-bullet dynamic bodies?
                        if (!collideA && !collideB)
                            continue;

                        // Compute the TOI for this contact.
                        // Advance the sweep that is behind to be in the same time interval.
                        float alpha0 = bA.Sweep.Alpha0;
                        if (bA.Sweep.Alpha0 < bB.Sweep.Alpha0)
                        {
                            alpha0 = bB.Sweep.Alpha0;
                            bA.Sweep.Advance(alpha0);
                        }
                        else if (bB.Sweep.Alpha0 < bA.Sweep.Alpha0)
                        {
                            alpha0 = bA.Sweep.Alpha0;
                            bB.Sweep.Advance(alpha0);
                        }

                        Debug.Assert(alpha0 < 1.0f);

                        // Compute the time of impact in interval [0, minTOI]
                        var input = new TOIInput();
                        input.ProxyA = new DistanceProxy(fA.Shape);
                        input.ProxyB = new DistanceProxy(fB.Shape);
                        input.SweepA = bA.Sweep;
                        input.SweepB = bB.Sweep;
                        input.TMax = 1.0f;

                        TimeOfImpact.CalculateTimeOfImpact(ref input, out TOIOutput output);

                        // Beta is the fraction of the remaining portion of the .
                        float beta = output.T;
                        if (output.State == TOIOutputState.Touching)
                            alpha = Math.Min(alpha0 + (1.0f - alpha0) * beta, 1.0f);
                        else
                            alpha = 1.0f;

                        c.TimeOfImpact = alpha;
                        c.Flags &= ~ContactFlags.TimeOfImpact;
                    }

                    if (alpha < minAlpha)
                    {
                        // This is the minimum TOI found so far.
                        minContact = c;
                        minAlpha = alpha;
                    }
                }

                // No more TOI events. Done!
                if (minContact == null || 1.0f - 10.0f * Maths.EPSILON < minAlpha) break;

                // Advance the bodies to the TOI.
                PhysicsLink fA1 = minContact.LinkA;
                PhysicsLink fB1 = minContact.LinkB;
                PhysicsBody bA0 = fA1.Body;
                PhysicsBody bB0 = fB1.Body;

                Sweep backup1 = bA0.Sweep;
                Sweep backup2 = bB0.Sweep;

                bA0.AdvanceSweep(minAlpha);
                bB0.AdvanceSweep(minAlpha);

                // The TOI contact likely has some new contact points.
                minContact.Update();
                minContact.Flags &= ~ContactFlags.TimeOfImpact;
                minContact.TimeOfImpactCount++;

                // Is the contact still touching?
                Debug.Assert(minContact.Flags.HasFlag(ContactFlags.Enabled));
                if (!minContact.Flags.HasFlag(ContactFlags.Enabled) || !minContact.Flags.HasFlag(ContactFlags.Touching))
                {
                    // Restore the sweeps.
                    minContact.Flags &= ~ContactFlags.Enabled;
                    bA0.Sweep = backup1;
                    bB0.Sweep = backup2;
                    bA0.SynchronizeSweep();
                    bB0.SynchronizeSweep();
                    continue;
                }

                bA0.Awake = true;
                bB0.Awake = true;

                // Build the island
                _island.Clear();
                _island.Add(bA0);
                _island.Add(bB0);
                _island.Add(minContact);

                bA0.Flags |= BodyFlags.Island;
                bB0.Flags |= BodyFlags.Island;
                minContact.Flags &= ~ContactFlags.Island;

                // Get contacts on bodyA and bodyB.
                for (var i = 0; i < 2; ++i)
                {
                    PhysicsBody body = i == 0 ? bA0 : bB0;
                    if (body.BodyType != BodyType.Dynamic) continue;

                    for (var j = 0; j < body.ActiveContacts.Count; j++)
                    {
                        CollisionContact contact = body.ActiveContacts[j];

                        if (_island._bodyCount == _island._bodyCapacity)
                            break;

                        if (_island._contactCount == _island._contactCapacity)
                            break;

                        // Has this contact already been added to the island?
                        if (contact.Flags.HasFlag(ContactFlags.Island))
                            continue;

                        // Only add static, kinematic, or bullet bodies.
                        PhysicsBody otherBody = body.ActiveContactsOtherBody[j];
                        PhysicsBody other = otherBody;
                        if (other.BodyType == BodyType.Dynamic && !body.Flags.HasFlag(BodyFlags.Bullet) && !other.Flags.HasFlag(BodyFlags.Bullet))
                            continue;

                        // todo: skip sensors/triggers

                        // Tentatively advance the body to the TOI.
                        Sweep backup = other.Sweep;
                        if (!other.Flags.HasFlag(BodyFlags.Island))
                            other.AdvanceSweep(minAlpha);

                        // Update the contact points
                        contact.Update();

                        // Was the contact disabled by the user?
                        if (!contact.Flags.HasFlag(ContactFlags.Enabled))
                        {
                            other.Sweep = backup;
                            other.SynchronizeSweep();
                            continue;
                        }

                        // Are there contact points?
                        if (!contact.Flags.HasFlag(ContactFlags.Touching))
                        {
                            other.Sweep = backup;
                            other.SynchronizeSweep();
                            continue;
                        }

                        // Add the contact to the island
                        minContact.Flags |= ContactFlags.Island;
                        _island.Add(contact);

                        // Has the other body already been added to the island?
                        if (other.Flags.HasFlag(BodyFlags.Island))
                            continue;

                        // Add the other body to the island.
                        other.Flags |= BodyFlags.Island;

                        if (other.BodyType != BodyType.Static)
                            other.Awake = true;

                        _island.Add(other);
                    }
                }

                var subStep = new PhysicsTimeStepData
                {
                    DeltaTime = (1.0f - minAlpha) * _stepData.DeltaTime,
                    DeltaTimeRatio = 1.0f,
                    PositionIterations = 20,
                    VelocityIterations = 8
                };
                _island.SolveTimeOfImpact(subStep, bA0.IslandIndex, bB0.IslandIndex);

                // Reset island flags and synchronize broad-phase proxies.
                for (var i = 0; i < _island._bodyCount; ++i)
                {
                    PhysicsBody body = _island._bodies[i];
                    body.Flags &= ~BodyFlags.Island;

                    if (body.BodyType != BodyType.Dynamic)
                        continue;

                    body.UpdateLinkBounds();

                    // Invalidate all contact TOIs on this displaced body.
                    for (var j = 0; j < body.ActiveContacts.Count; j++)
                    {
                        CollisionContact contact = body.ActiveContacts[j];
                        contact.Flags &= ~(ContactFlags.TimeOfImpact | ContactFlags.Island);
                    }
                }

                // Commit fixture proxy movements to the broad-phase so that new contacts are created.
                // Also, some contacts can be destroyed.
                FindNewContacts();
            }
        }
    }
}