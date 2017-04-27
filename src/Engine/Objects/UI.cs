using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects.Components;
using SoulEngine.Objects.Components.UI;

namespace SoulEngine.Objects
{
    public static class UI
    {
        public static UIObject Scrollbar(GameObject Object)
        {
            //Setup a scrollbar game object.
            Object.Layer = Enums.ObjectLayer.UI;
            Object.AddComponent(new MouseInput());
            Object.AddComponent(new Scrollbar());

            return Object;
        }
    }
}
