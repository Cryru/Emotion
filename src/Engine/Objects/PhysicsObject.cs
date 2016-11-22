using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Physics.Dynamics;
using SoulEngine.Physics.Collision.Shapes;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A basis for objects affected by physics.
    /// IN TESTING
    /// </summary>
    public class PhysicsObject : ObjectBase
    {
        #region "Declarations"
        /// <summary>
        /// Public accessor for the object's physics body.
        /// </summary>
        public Body Body
        {
            get
            {
                return body;
            }
        }
        /// <summary>
        /// The type of physics that should be applied to the object.
        /// By default this is static.
        /// </summary>
        public BodyType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                if(body != null)
                {
                    body.BodyType = type;
                }
            }
        }
        /// <summary>
        /// The collection of vertices that form the shape. These need to be in counterclockwise order.
        /// </summary>
        public Physics.Common.Vertices Shape = new Physics.Common.Vertices();

        #region "Events"
        /// <summary>
        /// Triggered when the object collides with another object.
        /// The first fixture is always the event raiser.
        /// </summary>
        Internal.Event<Fixture, Fixture, Physics.Dynamics.Contacts.Contact> onCollision = new Internal.Event<Fixture, Fixture, Physics.Dynamics.Contacts.Contact>();
        /// <summary>
        /// Triggered when a collision has happened, and has ended.
        /// </summary>
        Internal.Event<Fixture, Fixture> onCollisionEnd = new Internal.Event<Fixture, Fixture>();
        #endregion
        #region "Private"
        /// <summary>
        /// The object's physics body.
        /// </summary>
        Body body;
        /// <summary>
        /// The object's screen parent.
        /// </summary>
        private Screen parent;
        /// <summary>
        /// The private body type holder.
        /// </summary>
        BodyType type;
        #endregion
        #endregion

        /// <summary>
        /// Initializes an object.
        /// </summary>
        /// <param name="Screen">The screen that this object belongs to.</param>
        /// <param name="Image">The texture object that represents the object.</param>
        public PhysicsObject(Screen Screen, Texture Image = null) : base(Image)
        {
            parent = Screen;
            //Define body
            //Create body
            //  Type - Dynamic, static, kinematic
            //Create shape
            //Create fixture
            //Attach shape to body with fixture
            //Add body to world
            //Attach updater to Core

            //body = new Body(Physics.Engine.world, StartLocation);
            //body.BodyType = BodyType.Dynamic;

            //List<Physics.Vector2> edges = new List<Physics.Vector2>();

            ////Generate edges
            //edges.Add(new Physics.Vector2(0, 0));
            //edges.Add(new Physics.Vector2(width / 2, 0));
            //edges.Add(new Physics.Vector2(width / 2, width / 2));
            //edges.Add(new Physics.Vector2(0, width / 2));

            this.Location = Location;
            Location = Center;

            //shape = new PolygonShape(new Physics.Common.Vertices(edges.ToArray()), 1);
        }



        #region "Physics"
        /// <summary>
        /// Enabled physics calculations for the object.
        /// This prevents the object from being resized or moved and will be handled by the physics engine.
        /// </summary>
        /// <param name="Box">Whether the object should use the polygonal shape defined in the "Shape" field or just a box.</param>
        public void PhysicsEnable(bool Box = false)
        {
            if (Box == false)
            {
                //Create a body.
                body = new Body(parent.PhysicsWorld, Physics.Engine.PixelToPhysics(Center.ToPhys()), RotationRadians, Tags);
                //Create a shape.
                
                //Link with a fixture.
            }
            else
            {
                //Create the body from the rectangle preset.
                body = Physics.Factories.BodyFactory.CreateRectangle(parent.PhysicsWorld, Physics.Engine.PixelToPhysics(Size.X), Physics.Engine.PixelToPhysics(Size.Y), 1f, Tags);
                body.Position = Physics.Engine.PixelToPhysics(Center.ToPhys());
                
            }

            //Assign the body type.
            body.BodyType = Type;

            //Attach events.
            body.OnCollision += CollisionEvent;
            body.OnSeparation += CollisionEndEvent;
        }
        public void PhysicsDisable()
        {

        }
        #endregion

        #region "Event Handling"
        /// <summary>
        /// Triggers the internal event from the body's event.
        /// </summary>
        private void CollisionEndEvent(Fixture fixtureA, Fixture fixtureB)
        {
            onCollisionEnd.Trigger(fixtureA, fixtureB);
        }
        /// <summary>
        /// Triggers the internal event from the body's event.
        /// </summary>
        private bool CollisionEvent(Fixture fixtureA, Fixture fixtureB, Physics.Dynamics.Contacts.Contact contact)
        {
            onCollision.Trigger(fixtureA, fixtureB, contact);
            return true;
        }
        #endregion
        public override void Draw()
        {
            if(body != null)
            {
                //Get data from physics calculations.
                Center = Physics.Engine.PhysicsToPixel(body.Position).ToNorm();
                RotationRadians = body.Rotation;
            }

            //Draw the object.
            base.Draw();
        }
    }
}
