using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;

namespace SoulEngine.Objects.Components
{
    public class Button : Component
    {
        #region "Declarations"
        #region "Styles"
        /// <summary>
        /// The color of the button.
        /// </summary>
        public Color NormalColor;
        public Color MouseoveredColor;
        public Color ClickedColor;
        /// <summary>
        /// The texture of the button.
        /// </summary>
        public Texture2D Normal;
        public Texture2D MouseOvered;
        public Texture2D Clicked;
        #endregion
        #region "Objects"
        /// <summary>
        /// The parent of the component but as a UIObject.
        /// </summary>
        private UIObject Parent
        {
            get
            {
                return (UIObject)attachedObject;
            }
        }
        #endregion
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when the button is clicked.
        /// </summary>
        public event EventHandler<EventArgs> OnClicked;
        #endregion

        public override void Initialize()
        {
            //Check if object we are attaching to is on the UI layer.
            if (Parent.Layer != Enums.ObjectLayer.UI) throw new Exception("Cannot attach UI component to an object not on the UI layer!");

            //Check if the parent object has correct components.
            if (!(Parent.HasComponent<ActiveTexture>() && Parent.HasComponent<MouseInput>()))
                throw new Exception("Parent object is incorrectly initialized.");

            //Default styles.
            Normal = AssetManager.BlankTexture;
            MouseOvered = AssetManager.BlankTexture;
            Clicked = AssetManager.BlankTexture;
            NormalColor = Color.White;
            MouseoveredColor = Color.Black;
            ClickedColor = Color.Gray;

            //Hook events.
            Parent.Component<MouseInput>().OnMouseEnter += Button_OnMouseEnter;
            Parent.Component<MouseInput>().OnMouseLeave += Button_OnMouseLeave;
            Parent.Component<MouseInput>().OnClicked += Button_OnClicked;
            Parent.Component<MouseInput>().OnLetGo += Button_OnLetGo;

            Button_OnMouseLeave(null, new MouseMoveEventArgs(Vector2.Zero, Vector2.Zero));
        }

        #region "Event Handlers"
        private void Button_OnMouseLeave(object sender, MouseMoveEventArgs e)
        {
            Parent.Component<ActiveTexture>().Texture = Normal;
            Parent.Component<ActiveTexture>().Tint = NormalColor;
        }
        private void Button_OnMouseEnter(object sender, MouseMoveEventArgs e)
        {
            Parent.Component<ActiveTexture>().Texture = MouseOvered;
            Parent.Component<ActiveTexture>().Tint = MouseoveredColor;
        }
        private void Button_OnClicked(object sender, MouseButtonEventArgs e)
        {
            Parent.Component<ActiveTexture>().Texture = Clicked;
            Parent.Component<ActiveTexture>().Tint = ClickedColor;
            OnClicked?.Invoke(Parent, EventArgs.Empty);
        }
        private void Button_OnLetGo(object sender, MouseButtonEventArgs e)
        {
            Button_OnMouseEnter(null, new MouseMoveEventArgs(Vector2.Zero, Vector2.Zero));
        }
        #endregion
    }
}
