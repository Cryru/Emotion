using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;
using SoulEngine.Modules;

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
                if (value == _value) return;
                _value = MathHelper.Clamp(value, 0, 100);
                OnValueChanged?.Invoke(attachedObject, new ScrollbarEventArgs(_value));
            }
        }
        private int _value;

        /// <summary>
        /// Whether the object is focused, used to apply various input events.
        /// </summary>
        public bool Focused = false;

        /// <summary>
        /// Whether the left click is held on the object.
        /// </summary>
        public bool Held = false;

        #region "Styles"
        /// <summary>
        /// The color of the scroll bar.
        /// </summary>
        public Color BarColor
        {
            get { return Bar.Component<ActiveTexture>().Tint; }
            set { Bar.Component<ActiveTexture>().Tint = value; }
        }
        /// <summary>
        /// The color of the selector.
        /// </summary>
        public Color SelectorColor;
        public Color FocusedColor;
        public Color HeldColor;
        /// <summary>
        /// The texture of the bar.
        /// </summary>
        public Texture2D BarTexture
        {
            get { return Bar.Component<ActiveTexture>().Texture; }
            set { Bar.Component<ActiveTexture>().Texture = value; }
        }
        /// <summary>
        /// The texture of the selector.
        /// </summary>
        public Texture2D SelectorTexture
        {
            get { return Selector.Component<ActiveTexture>().Texture; }
            set { Selector.Component<ActiveTexture>().Texture = value; }
        }
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
        private GameObject Bar
        {
            get
            {
                return Context.Core.Scene.GetObject(barID);
            }
        }
        private GameObject Selector
        {
            get
            {
                return Context.Core.Scene.GetObject(selID);
            }
        }
        private string barID;
        private string selID;
        #endregion
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when the scrollbar is scrolled.
        /// </summary>
        public event EventHandler<ScrollbarEventArgs> OnValueChanged;
        #endregion

        public Scrollbar(string Bar, string Selector)
        {
            barID = Bar;
            selID = Selector;
        }

        public override void Initialize()
        {
            //Check if object we are attaching to is on the UI layer.
            if (Parent.Layer != Enums.ObjectLayer.UI) throw new Exception("Cannot attach UI component to an object not on the UI layer!");

            //Check if children objects have correct components.
            if (!(Bar.HasComponent<ActiveTexture>() && Selector.HasComponent<ActiveTexture>() && Selector.HasComponent<MouseInput>()))
                throw new Exception("Children objects " + barID + " and " + selID + " are incorrectly initialized.");

            //Default styles.
            BarTexture = AssetManager.BlankTexture;
            SelectorTexture = AssetManager.BlankTexture;
            BarColor = Color.White;
            SelectorColor = Color.Black;
            FocusedColor = Color.Gray;
            HeldColor = Color.LightGray;

            //Hook events.
            Selector.Component<MouseInput>().OnClicked += FocusGain;
            Selector.Component<MouseInput>().OnClickOutside += FocusLost;
            Input.OnMouseButtonUp += LetGo;
            Input.OnMouseMove += ScrollbarMove;
            Input.OnKeyDown += ScrollbarButton;
        }

        public override void Update()
        {
            //Set bar size to parent size and position 0.
            Bar.Position = new Vector2(0, 0);
            Bar.Size = Parent.Size;
            Bar.Drawing = Parent.Drawing;
            Parent.AsChild(Bar);

            //Calculate selector properties.
            Selector.Width = (Parent.Width / 100) * 6;
            Selector.Height = (int)(Parent.Height + ((Parent.Height * 0.1) * 2));
            Selector.Center = Parent.Center;
            Selector.X = Parent.X + ((Bar.Width / 100) * _value) - Selector.Width / 2;
            Selector.Drawing = Parent.Drawing;

            //Apply dynamic styles.
            if (Held) Selector.Component<ActiveTexture>().Tint = HeldColor;
            else if (Focused) Selector.Component<ActiveTexture>().Tint = FocusedColor; else Selector.Component<ActiveTexture>().Tint = SelectorColor;
        }

        #region "Event Handlers"
        private void ScrollbarMove(object sender, Events.MouseMoveEventArgs e)
        {
            if (!Held) return;

            float PosWithin = e.To.X - Parent.X;

            float incr = Bar.Width / 100;

            float va = PosWithin / incr;

            Value = (int) va;
        }
        private void LetGo(object sender, Events.MouseButtonEventArgs e)
        {
            if (e.Button == Enums.MouseButton.Left) Held = false;
        }
        private void FocusGain(object sender, Events.MouseButtonEventArgs e)
        {
            Focused = true;

            if (e.Button == Enums.MouseButton.Left) Held = true;
        }
        private void FocusLost(object sender, EventArgs e)
        {
            Focused = false;
        }
        private void ScrollbarButton(object sender, KeyEventArgs e)
        {
            if (!Focused) return;

            switch(e.Key)
            {
                case Microsoft.Xna.Framework.Input.Keys.Left:
                case Microsoft.Xna.Framework.Input.Keys.A:
                    Value -= 5;
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Right:
                case Microsoft.Xna.Framework.Input.Keys.D:
                    Value += 5;
                    break;
            }
        }
        #endregion

        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Remove children objects.
                    Context.Core.Scene.RemoveObject(barID);
                    Context.Core.Scene.RemoveObject(selID);
                }

                //Free resources.
                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}
