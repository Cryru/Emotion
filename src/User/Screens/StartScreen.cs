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
            test = new TextObject(Core.fontDebug, "Lorem Ipsum is s<color=255-0-0>i</>mply dummy text of the printing <color=255-0-0>a</>nd typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.", 250, 500);
            test.Background = true;
            test.TextStyle = TextObject.RenderMode.Center;
            test.backgroundColor = Color.Black;
            test.backgroundOpacity = 0.5f;
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
