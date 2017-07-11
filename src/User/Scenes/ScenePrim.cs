using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Events;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulEngine.Modules;

namespace SoulEngine.Scenes
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
            // Context.Core.LoadScene(new StressTest());
            Context.Core.Module<SceneManager>().LoadScene("test", new test1(), false);

            GameObject a = GameObject.GenericDrawObject;
            a.Bounds = new Rectangle(100, 100, 100, 100);
            AddObject("test object 1", a);

            GameObject clickTest = GameObject.GenericTextObject;
            clickTest.Component<ActiveText>().Text = "Test test test <click>CLICK ME</>wadasdsadasdasdasdas";
            clickTest.Bounds = new Rectangle(200, 200, 100, 100);
            AddObject("text link test", clickTest);

        }

        public override void Update()
        {
            if (InputModule.isKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                Context.Core.Module<SceneManager>().SwapScene("test");
            }
        }
    }
}
