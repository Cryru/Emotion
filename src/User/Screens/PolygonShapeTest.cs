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
    public class PolygonToolTest : Screen
    {
        #region "Declarations"

        #endregion

        Physics.Common.Vertices vert = new Physics.Common.Vertices();
        List<ObjectBase> points = new List<ObjectBase>();

        /// <summary>
        /// Is run when the screen is first loaded.
        /// It is recommended that you initialize your objects here.
        /// </summary>
        public override void LoadObjects()
        {
            vert.Add(new Physics.Vector2(0, 0));
            vert.Add(new Physics.Vector2(40, 0));
            vert.Add(new Physics.Vector2(65, -26));
            vert.Add(new Physics.Vector2(59, -55));
            vert.Add(new Physics.Vector2(27, -82));
            vert.Add(new Physics.Vector2(2, -74));
            vert.Add(new Physics.Vector2(-17, -38));
            vert.Add(new Physics.Vector2(-10, -13));

            for (int i = 0; i < vert.Count; i++)
            {
                ObjectBase temp = new ObjectBase(Core.blankTexture);
                temp.Center = new Vector2(500 - vert[i].X, 500 - vert[i].Y);
                temp.Size = new Vector2(3, 3);
                points.Add(temp);
            }
        }

        /// <summary>
        /// Is run every frame on the CPU.
        /// Game logic and other stuff go here.
        /// </summary>
        public override void Update()
        {
           
        }
        /// <summary>
        /// Is run every frame on the GPU.
        /// Your draw calls go here.
        /// </summary>
        public override void Draw()
        {

            Core.DrawOnScreen();
            for (int i = 0; i < points.Count; i++)
            {
                if(i > 0) Core.ink.DrawLine(Color.Black, points[i].Location, points[i - 1].Location, 2);
                points[i].Draw();
            }
            Core.ink.Draw(Core.blankTexture.Image, null, new Rectangle(100, 100, 100, 1), color: Color.White, rotation: Core.DegreesToRadians(0));
            Core.ink.End();
        }
    }
}
