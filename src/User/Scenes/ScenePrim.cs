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
            a.AddComponent(new SoulEngine.Objects.Components.ActiveText()
            { Text =
            "</></></><a>Hello sir! This is the first line.<a></><a>\nAnd</> this <a>is <a>t</>he second<a></> line!\nWhile</> this is the third :D!</><a>\n</><a></>"
            });

            Legacy.Core.Setup();
        }

        public override void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
