using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    public class PhysicsTest : Screen
    {
        #region "Declarations"

        #endregion

        List<PhysicsObject> Objects = new List<PhysicsObject>();
        ObjectBase test = new ObjectBase();
        PhysicsObject ground;
        PhysicsObject shape;
        /// <summary>
        /// Is run when the screen is first loaded.
        /// It is recommended that you initialize your objects here.
        /// </summary>
        public override void LoadObjects()
        {
            ground = new PhysicsObject(this, Core.blankTexture);
            ground.Location = new Vector2(100, 500);
            ground.Size = new Vector2(1000, 20);

            ground.SimulationType = Physics.Dynamics.BodyType.Static;
            ground.PhysicsEnable(PhysicsTemplate.Rectangle);

            ground.Tags.Add("ground");

            Gravity = new Physics.Vector2(0, 10);

            shape = new PhysicsObject(this, new Texture("shape"));
            shape.SimulationType = Physics.Dynamics.BodyType.Dynamic;
            shape.Size = new Vector2(100, 100);
            Physics.Common.Vertices vert = new Physics.Common.Vertices();
            vert.Add(new Physics.Vector2(-20, 38));
            vert.Add(new Physics.Vector2(16, 38));
            vert.Add(new Physics.Vector2(40, 14));
            vert.Add(new Physics.Vector2(36, -17));
            vert.Add(new Physics.Vector2(4, -38));
            vert.Add(new Physics.Vector2(-20, -30));
            vert.Add(new Physics.Vector2(-38, 6));

            shape.Vertices = vert;
            shape.Location = new Vector2(300, 100);
            shape.PhysicsEnable();
        }

        public void testcol(PhysicsObject a, PhysicsObject b, Physics.Dynamics.Contacts.Contact c)
        {
           if(b != null && b.Tags.Count > 0 && b.Tags[0] == "ground")
            {
                //a.PhysicsDisable();
            }
        }

        /// <summary>
        /// Is run every frame on the CPU.
        /// Game logic and other stuff go here.
        /// </summary>
        public override void Update()
        {
            if(ground.Body.ContactList != null) Core.debugText.Text = string.Join("\n", ground.Body.ContactList);
            if (Input.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                //Add new objects.
                Vector2 mouseincenter = Input.getMousePos();
                mouseincenter.X -= 8;
                mouseincenter.Y -= 8;
                PhysicsObject temp = new PhysicsObject(this, Core.blankTexture);
                temp.Location = mouseincenter;
                temp.Size = new Vector2(16, 16);
                temp.SimulationType = Physics.Dynamics.BodyType.Dynamic;
                temp.PhysicsEnable(PhysicsTemplate.Rectangle);
                temp.Body.ApplyForce(new Physics.Vector2(0, 2000));
                temp.onCollision.Add(testcol);
                Objects.Add(temp);
            }
            if(Input.KeyDownTrigger(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                for (int i = 0; i < Objects.Count; i++)
                {
                    Objects[i].Body.ApplyForce(new Physics.Vector2(0, -1500));
                }
            }
        }
        /// <summary>
        /// Is run every frame on the GPU.
        /// Your draw calls go here.
        /// </summary>
        public override void Draw()
        {

            Core.DrawOnScreen();
            for (int i = 0; i < Objects.Count; i++)
            {
                Objects[i].Draw();
            }
            ground.Draw();
            shape.Draw();
            Core.ink.Draw(Core.blankTexture.Image, null, new Rectangle(100, 100, 100, 1), color: Color.White, rotation: Core.DegreesToRadians(0));
            Core.ink.End();
        }
    }
}
