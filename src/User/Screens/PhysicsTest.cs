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
        /// <summary>
        /// Is run when the screen is first loaded.
        /// It is recommended that you initialize your objects here.
        /// </summary>
        public override void LoadObjects()
        {
            ground = new PhysicsObject(Core.blankTexture);
            ground.Location = new Vector2(100, 500);
            ground.Size = new Vector2(1000, 20);

            ground.Type = Physics.Dynamics.BodyType.Static;

        }

        /// <summary>
        /// Is run every frame on the CPU.
        /// Game logic and other stuff go here.
        /// </summary>
        public override void Update()
        {
            if (Input.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                //Add new objects.
                Vector2 mouseincenter = Input.getMousePos();
                mouseincenter.X -= 8;
                mouseincenter.Y -= 8;
                PhysicsObject temp = new PhysicsObject(Core.blankTexture, mouseincenter, new Vector2(16,16));
                Objects.Add(temp);
            }
            if(Input.KeyDownTrigger(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                for (int i = 0; i < Objects.Count; i++)
                {
                    Objects[i].PhysicsBody.ApplyForce(new Physics.Vector2(0, -1500));
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
            Core.ink.Draw(Core.blankTexture.Image, null, new Rectangle(100, 100, 100, 1), color: Color.White, rotation: Core.DegreesToRadians(0));
            Core.ink.End();
        }
    }
}
