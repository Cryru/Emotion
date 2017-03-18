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
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The first scene to load.
    /// </summary>
    public class ScenePrim : Scene
    {
        #region "Declarations"

        #endregion
        public override void Start()
        {
            // throw new NotImplementedException();
            Console.WriteLine("SCENE PRIM LOADED");

            GameObject a = new GameObject();
            a.AddComponent(new Objects.Components.ActiveTexture());
            a.AddComponent(new SoulEngine.Objects.Components.ActiveText()
            {
                Text =
            "</></></><bracket>Hello sir! This is the first line.<bracket></><bracket>\nAnd</> this <bracket>is <bracket>t</>he second<bracket></> line!\nWhile</> this is the third :D!</><bracket>\n</><bracket></>"
            });
            //a.AddComponent(new Objects.Components.Transform() { X = 0, Y = 0, Width = 200, Height = 200 });

            a.AddComponent(new Objects.Components.Renderer());
            AddObject("", a);
            
            //Legacy.Core.Setup();
        }

        public override void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
