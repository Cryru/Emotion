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
        public Body PhysicsBody
        {
            get
            {
                return body;
            }
        }


        Body body;
        ObjectBase outline;
        public BodyType Type
        {
            get
            {
                return body.BodyType;
            }
            set
            {
                body.BodyType = value;
            }
        }
        #endregion

        /// <summary>
        /// Initializes an object.
        /// </summary>
        /// <param name="Image">The texture object that represents the object.</param>
        public PhysicsObject(Texture Image, Vector2 Location, Vector2 Size) : base(Image)
        {
            Width = (int) Size.X;
            Height = (int)Size.Y;

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

            body = Physics.Factories.BodyFactory.CreateRectangle(Physics.Engine.world, Physics.Engine.PixelToPhysics(Size.X), Physics.Engine.PixelToPhysics(Size.Y), 1f);

            body.Position = Physics.Engine.PixelToPhysics(Center.ToPhys());
            body.BodyType = BodyType.Dynamic;




            //Core.Updates.Add(PhysicsUpdate);

            outline = new ObjectBase(Core.blankTexture);

        }

        private void PhysicsUpdate()
        {
            
        }


        public override void Draw()
        {
            Center = Physics.Engine.PhysicsToPixel(body.Position).ToNorm();
            RotationRadians = body.Rotation;

            outline.ObjectCopy(this);
            outline.Color = Color.Black;
            outline.RotationRadians = RotationRadians;
            outline.Size = new Vector2(Width + 5, Height + 5);
            outline.Center = Center;
            outline.Draw();

            

            base.Draw();
        }
    }
}
