// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Raya.Graphics.Primitives;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Physics;
using Soul.Physics.Common;
using Soul.Physics.Dynamics;
using Soul.Physics.Dynamics.Contacts;
using Soul.Physics.Factories;
using Vector2 = Raya.Graphics.Primitives.Vector2;

#endregion

namespace Soul.Engine.Objects
{
    public class PhysicsBody : Actor
    {
        #region Properties

        /// <summary>
        /// The object's physics body.
        /// </summary>
        public Body Body;

        /// <summary>
        /// The type of physics simulation to apply. By default it is static.
        /// </summary>
        public BodyType SimulationType
        {
            get { return _type; }
            set
            {
                _type = value;

                // Set the type to the body object.
                if (Body != null)
                    Body.BodyType = _type;
            }
        }

        private BodyType _type = BodyType.Static;

        /// <summary>
        /// The vertices which make us the physics collision shape.
        /// </summary>
        public Vector2[] PolygonVertices;

        /// <summary>
        /// The physics body's density.
        /// </summary>
        public float Density = 1;

        /// <summary>
        /// The body's shape template.
        /// </summary>
        private ShapeType _shape;

        /// <summary>
        /// The world this object belongs to.
        /// </summary>
        private World _world;

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the object collides with another object.
        /// The first fixture is always the event raiser.
        /// </summary>
        public event Action<PhysicsBody, PhysicsBody, Contact> OnCollision;

        /// <summary>
        /// Triggered when a collision has happened, and has ended.
        /// </summary>
        public event Action<PhysicsBody, PhysicsBody> OnCollisionEnd;

        #endregion

        /// <summary>
        /// Setups a physics body.
        /// </summary>
        /// <param name="scene">The scene this body will belong to.</param>
        /// <param name="shape">The collision shape to represent the body.</param>
        /// <param name="polygonVertices">The vertices that make up the polygon, if the shape is polygon.</param>
        public PhysicsBody(Scene scene, ShapeType shape, Vector2[] polygonVertices = null)
        {
            // Assign the physics world.
            PolygonVertices = polygonVertices;

            // Assign the shape.
            _shape = shape;

            // Assign the world and mark down that it has physics.
            scene.HasPhysics = true;
            _world = PhysicsModule.GetWorldForScene(scene);

            // Check if polygon shape and no vertices.
            if (shape == ShapeType.Polygon && polygonVertices == null) _shape = ShapeType.Rectangle;
        }


        public override void Initialize()
        {
            // Create a body.
            CreateBody();
        }

        public void Destroy()
        {
            // Unhook events.
            ((GameObject) Parent).onSizeChanged -= CreateBody;

            if (Body?.FixtureList != null) Body.DestroyFixture(Body.FixtureList[0]);
            _world.RemoveBody(Body);
        }

        public override void Update()
        {
            // Update the parent's position and rotation according to what's happening in the physics world.
            ((GameObject) Parent).Center = PhysicsModule.PhysicsToPixel(Body.Position.X, Body.Position.Y);
            ((GameObject) Parent).Rotation = Body.Rotation;
        }

        #region Internals

        /// <summary>
        /// Creates or recreates the physics body.
        /// </summary>
        private void CreateBody()
        {
            // Check if it already exists, in which case destroy it.
            if (Body != null)
            {
                if (Body?.FixtureList != null) Body.DestroyFixture(Body.FixtureList[0]);
                _world.RemoveBody(Body);
                
                // Report the destruction of a body.
                Debugger.DebugMessage(DebugMessageSource.PhysicsModule, "Destroyed a body at " + Body.Position);
            }

            // Create a body from the defined shape.
            switch (_shape)
            {
                case ShapeType.Polygon:

                    Vertices vertices = new Vertices(PolygonVertices.Length);

                    // Convert vertices from pixel coordinates to physics coordinates.
                    for (int i = 0; i < PolygonVertices.Length; i++)
                    {
                        vertices.Add(PhysicsModule.PixelToPhysics(PolygonVertices[i]));
                    }
                    // Create a polygon body from the vertices.
                    Body = BodyFactory.CreatePolygon(_world, vertices, Density,
                        this);
                    break;
                case ShapeType.Rectangle:
                    // Create a body from a rectangle template.
                    Body = BodyFactory.CreateRectangle(_world,
                        PhysicsModule.PixelToPhysics(((GameObject) Parent).Width),
                        PhysicsModule.PixelToPhysics(((GameObject) Parent).Height), Density, this);
                    break;
                case ShapeType.Circle:
                    // Create a body from a circle template.
                    Body = BodyFactory.CreateCircle(_world,
                        PhysicsModule.PixelToPhysics(Math.Max(((GameObject) Parent).Width / 2,
                            ((GameObject) Parent).Height / 2)),
                        Density, this);
                    break;
                default:
                    Error.Raise(3, "Unknown physics shape type: " + _shape);
                    break;
            }

            // Assign the body type.
            Body.BodyType = SimulationType;

            // Set the physics body's position according to the parent's center.
            Body.Position = PhysicsModule.PixelToPhysics((Vector2f) ((GameObject) Parent).Center);

            // Attach to physics engine events.
            Body.OnCollision += CollisionEvent;
            Body.OnSeparation += CollisionEndEvent;

            // Report the creation of the new body.
            Debugger.DebugMessage(DebugMessageSource.PhysicsModule, "Created new body at " + Body.Position + " and Real: " + ((GameObject)Parent).Center);
        }

        #endregion

        #region Event Handlers

        private bool CollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            // Convert the fixture's user data to physics bodies and invoke the SE event.
            OnCollision?.Invoke((PhysicsBody) fixtureA.Body.UserData, (PhysicsBody) fixtureB.Body.UserData, contact);

            return true;
        }

        private void CollisionEndEvent(Fixture fixtureA, Fixture fixtureB)
        {
            // Convert the fixture's user data to physics bodies and invoke the SE event.
            OnCollisionEnd?.Invoke((PhysicsBody) fixtureA.Body.UserData, (PhysicsBody) fixtureB.Body.UserData);
        }

        #endregion
    }
}