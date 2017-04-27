using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components
{
    public class Scrollbar : Component
    {
        #region "Declarations"
        /// <summary>
        /// The value of the scrollbar.
        /// </summary>
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = MathHelper.Clamp(value, 0, 100);
            }
        }
        private int _value;

        /// <summary>
        /// The parent of the component but as a UIObject.
        /// </summary>
        private UIObject parent
        {
            get
            {
                return (UIObject)attachedObject;
            }
        }

        /// <summary>
        /// Whether the object is focused, used to apply various input events.
        /// </summary>
        private bool focused = false;

        #region "Styles"
        /// <summary>
        /// The color of the scroll bar.
        /// </summary>
        public Color BarColor
        {
            get => parent.Children["bar"].Component<ActiveTexture>().Tint;
            set => parent.Children["bar"].Component<ActiveTexture>().Tint = value;
        }
        /// <summary>
        /// The color of the selector.
        /// </summary>
        public Color SelectorColor
        {
            get => parent.Children["selector"].Component<ActiveTexture>().Tint;
            set => parent.Children["selector"].Component<ActiveTexture>().Tint = value;
        }
        /// <summary>
        /// The texture of the bar.
        /// </summary>
        public Texture2D BarTexture
        {
            get => parent.Children["bar"].Component<ActiveTexture>().Texture;
            set => parent.Children["bar"].Component<ActiveTexture>().Texture = value;
        }
        /// <summary>
        /// The texture of the selector.
        /// </summary>
        public Texture2D SelectorTexture
        {
            get => parent.Children["selector"].Component<ActiveTexture>().Texture;
            set => parent.Children["selector"].Component<ActiveTexture>().Texture = value;
        }
        #endregion
        #endregion

        public Scrollbar()
        {
            //Check if object we are attaching to is on the UI layer.
            if (attachedObject.Layer != Enums.ObjectLayer.UI) throw new Exception("Cannot attach UI component to an object not on the UI layer!");
        }

        public void Initialize()
        {
            //Generate objects.
            GameObject bar = GameObject.GenericDrawObject;
            bar.Layer = Enums.ObjectLayer.UI;
            bar.Position = new Vector2(0, 0);
            bar.Size = parent.Size;
            bar.Priority = 0;
            parent.Children.Add("bar", bar);

            GameObject selector = new GameObject();
            selector.Layer = Enums.ObjectLayer.UI;
            selector.Position = new Vector2(0, 0);
            selector.AddComponent(new ActiveTexture());
            selector.AddComponent(new MouseInput());
            selector.Priority = 1;
            parent.Children.Add("selector", selector);

            //Default styles.
            BarTexture = AssetManager.BlankTexture;
            SelectorTexture = AssetManager.BlankTexture;
            BarColor = Color.White;
            SelectorColor = Color.Black;

            //Hook events.
            selector.Component<MouseInput>().OnClicked += FocusGain;
        }

        private void FocusGain(object sender, EventArgs e)
        {
            focused = true;
        }

        public override void Update()
        {
            parent.Children["bar"].Size = parent.Size;

            parent.Children["selector"].Width = (attachedObject.Width / 100) * 3;
            parent.Children["selector"].Height = (int)(attachedObject.Height + (attachedObject.Height * 0.1) * 2);
            parent.Children["selector"].Center = parent.ActualPositionToChild(parent.Center);
            parent.Children["selector"].X = ((parent.Children["bar"].Width / 100) * _value);
        }

        #region "Events"


        #endregion
    }
}
