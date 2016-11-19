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
        /// <summary>
        /// Is run when the screen is first loaded.
        /// It is recommended that you initialize your objects here.
        /// </summary>
        public override void LoadObjects()
        {
           
        }

        /// <summary>
        /// Is run every frame on the CPU.
        /// Game logic and other stuff go here.
        /// </summary>
        public override void Update()
        {
            Vector2 test = new Vector2();
            

            if(Input.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                //Add new objects.
                PhysicsObject temp = new PhysicsObject(Core.blankTexture);
                temp.Size = new Vector2(100, 100);
                temp.Center = Input.getMousePos();
                Objects.Add(temp);
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
            Core.ink.Draw(Core.blankTexture.Image, null, new Rectangle(100, 100, 100, 1), color: Color.White, rotation: Core.DegreesToRadians(0));
            Core.ink.End();
        }
    }
}
