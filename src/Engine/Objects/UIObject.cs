using Microsoft.Xna.Framework;
using SoulEngine.Objects.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;

namespace SoulEngine.Objects
{
    public class UIObject : GameObject
    {
        #region "Declarations"
        /// <summary>
        /// Override the layer property so the object is always on the UI layer.
        /// </summary>
        public override ObjectLayer Layer
        {
            get
            {
                return ObjectLayer.UI;
            }
        }
        #endregion

        #region "Helper Functions"
        public void AsChild(GameObject Child)
        {
            Child.X += X;
            Child.Y += Y;
        }
        #endregion

        #region "Templates"
        /// <summary>
        /// A UI horizontal scroll bar. The value of it is under the Scrollbar component.
        /// </summary>
        public static UIObject Scrollbar
        {
            get
            {
                UIObject Object = new UIObject();

                GameObject bar = GenericDrawObject;
                bar.Layer = ObjectLayer.UI;
                bar.Priority = 0;
                string nameBar = GenerateChildObjectName("scrollbar-bar");
                Context.Core.Scene.AddObject(nameBar, bar);

                GameObject selector = GenericDrawObject;
                selector.Layer = ObjectLayer.UI;
                selector.AddComponent(new MouseInput());
                selector.Priority = 1;
                string nameSelect = GenerateChildObjectName("scrollbar-selector");
                Context.Core.Scene.AddObject(nameSelect, selector);

                Object.AddComponent(new Scrollbar(nameBar, nameSelect));

                return Object;
            }
        }
        /// <summary>
        /// A button.
        /// </summary>
        public static UIObject Button
        {
            get
            {
                UIObject Object = new UIObject();
                Object.AddComponent(new MouseInput());
                Object.AddComponent(new ActiveTexture(TextureMode.Stretch));
                Object.AddComponent(new Button());

                return Object;
            }
        }

        /// <summary>
        /// A box.
        /// </summary>
        public static UIObject Box
        {
            get
            {
                UIObject Object = new UIObject();
                Object.AddComponent(new Box());

                return Object;
            }
        }
        #endregion

        #region "Helpers"
        /// <summary>
        /// Generates a unique name for a child object.
        /// </summary>
        /// <param name="Type">The type of object.</param>
        private static string GenerateChildObjectName(string Type)
        {
            return Type + "_" + Functions.generateRandomNumber(1, 255) + "N" + Context.Core.Scene.ObjectCount +
                "@" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond;
        }
        #endregion
    }
}
