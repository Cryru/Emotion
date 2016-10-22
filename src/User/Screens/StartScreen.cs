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
    public class StartScreen : Objects.Screen
    {
        #region "Declarations"
        /// <summary>
        /// The screens priority. The higher it is the earlier it will be executed.
        /// </summary>
        internal int Priority = 0;
        #endregion

        TextObject test;
        /// <summary>
        /// Is run when the screen is first loaded.
        /// It is recommended that you initialize your objects here.
        /// </summary>
        public override void LoadObjects()
        {
            test = new TextObject(Core.fontDebug, "A list of some of my <color=255-0-0>f</>avorite free programs that you definitely have to check out.", 200, 500);
            test.Background = true;
            test.backgroundColor = Color.Black;
            Core.CenterObject(test);
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
            Core.DrawScreen();
            test.Draw();
            Core.ink.End();
        }
    }
}
