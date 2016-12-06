using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Physics;

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
        public BodyType SimulationType
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
        public Vertices Vertices = new Vertices();
        /// <summary>
        /// Whether Physics are enabled.
        /// </summary>
        public bool PhysicsEnabled
        {
            get
            {
                return physics;
            }
        }

        #region "Events"
        /// <summary>
        /// Triggered when the object collides with another object.
        /// The first fixture is always the event raiser.
        /// </summary>
        public Internal.Event<PhysicsObject, PhysicsObject, Contact> onCollision = new Internal.Event<PhysicsObject, PhysicsObject, Contact>();
        /// <summary>
        /// Triggered when a collision has happened, and has ended.
        /// </summary>
        public Internal.Event<PhysicsObject, PhysicsObject> onCollisionEnd = new Internal.Event<PhysicsObject, PhysicsObject>();
        #endregion
        #region "Private"
        /// <summary>
        /// The object's physics body.
        /// </summary>
        Body body;
        /// <summary>
        /// The object's screen parent.
        /// </summary>
        private Screen screenhost;
        /// <summary>
        /// The private body type holder.
        /// </summary>
        BodyType type;
        /// <summary>
        /// The private accessor for the physics being enabled.
        /// </summary>
        private bool physics = false;
        #endregion
        #endregion

        /// <summary>
        /// Initializes a physics object.
        /// </summary>
        /// <param name="Screen">The screen that this object belongs to. Requires for physics simulation context.</param>
        /// <param name="Image">The texture object that represents the object.</param>
        public PhysicsObject(Screen Screen, Texture Image = null) : base(Image)
        {
            //Get the hosting screen.
            screenhost = Screen;
        }

        #region "Physics Fumctions"
        /// <summary>
        /// Enabled physics calculations for the object.
        /// This prevents the object from being resized or moved and will be handled by the physics engine.
        /// </summary>
        /// <param name="Template">The bounding template to use, if any. If set to none a shape will be generated from the Vertices field.</param>
        public void EnablePhysics(PhysicsTemplate Template = PhysicsTemplate.None, float Density = 1f, bool ConvertPixelUnits = true)
        {
            switch(Template)
            {
                case PhysicsTemplate.None:
                    if(ConvertPixelUnits == true)
                    {
                        //Convert vertices from pixel coordinates to physics coordinates.
                        for (int i = 0; i < Vertices.Count; i++)
                        {
                            Vector2 t;
                            t.X = Physics.Engine.PixelToPhysics(Vertices[i].X);
                            t.Y = Physics.Engine.PixelToPhysics(Vertices[i].Y);

                            Vertices[i] = t;
                        }
                    }
                    //Create a body from a the vertices.
                    body = BodyFactory.CreatePolygon(screenhost.PhysicsWorld, Vertices, Density, this);
                    //Set it's position.
                    body.Position = Physics.Engine.PixelToPhysics(Center);
                    break;
                case PhysicsTemplate.Rectangle:
                    //Create a body from a rectangle template.
                    body = BodyFactory.CreateRectangle(screenhost.PhysicsWorld, Physics.Engine.PixelToPhysics(Size.X), Physics.Engine.PixelToPhysics(Size.Y), Density, this);
                    //Set it's position.
                    body.Position = Physics.Engine.PixelToPhysics(Center);
                    break;
                case PhysicsTemplate.Circle:
                    //Create a body from a circle template.
                    body = BodyFactory.CreateCircle(screenhost.PhysicsWorld, Physics.Engine.PixelToPhysics(Size.X / 2), Density, this);
                    //Set it's position.
                    body.Position = Physics.Engine.PixelToPhysics(Center);
                    break;
            }

            //Assign the body type.
            body.BodyType = SimulationType;

            //Attach events.
            body.OnCollision += CollisionEvent;
            body.OnSeparation += CollisionEndEvent;

            //Set the flag to true.
            physics = true;
        }
        /// <summary>
        /// Removes the physics object from simulation. Other physics objects will not collide with it, to stop the object from moving but keep collision set the SimulationType to Static.
        /// </summary>
        public void DisablePhysics()
        {
            if(body != null && body.FixtureList != null) body.DestroyFixture(body.FixtureList[0]);
            screenhost.PhysicsWorld.RemoveBody(body);

            //Set the flag to false.
            physics = false;
        }
        #endregion
        #region "Event Handling"
        /// <summary>
        /// Triggers the internal event from the body's event.
        /// </summary>
        private void CollisionEndEvent(Fixture fixtureA, Fixture fixtureB)
        {
            onCollisionEnd.Trigger((PhysicsObject) fixtureA.Body.UserData, (PhysicsObject)fixtureB.Body.UserData);
        }
        /// <summary>
        /// Triggers the internal event from the body's event.
        /// </summary>
        private bool CollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            onCollision.Trigger((PhysicsObject)fixtureA.Body.UserData, (PhysicsObject)fixtureB.Body.UserData, contact);
            return true;
        }
        #endregion
        public override void Draw()
        {
            //Check if physics is enabled.
            if(physics == true)
            {
                //Get data from physics calculations.
                Center = Physics.Engine.PhysicsToPixel(body.Position);
                RotationRadians = body.Rotation;
            }

            //Draw the object.
            base.Draw();
        }
    }

    public enum PhysicsTemplate
    {
        None,
        Rectangle,
        Circle
    }
}
