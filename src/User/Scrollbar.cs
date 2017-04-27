using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.UI
{
    public class Scrollbar : DrawComponent
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
        public Color BarColor = Color.White;
        public Color SelectorColor = Color.Black;
        public Texture2D BarTexture;
        public Texture2D SelectorTexture;
        #endregion

        public Scrollbar()
        {
            BarTexture = AssetManager.BlankTexture;
            SelectorTexture = AssetManager.BlankTexture;
        }

        public override void Update()
        {

        }

        public override void Draw()
        {
            //Draw bar.
            Tint = BarColor;
            Draw(BarTexture);

            //Draw selector.
            int selectorWidth = (attachedObject.Width / 100) * 3;
            int selectorHeight = (int)(attachedObject.Height + (attachedObject.Height * 0.1) * 2);
            int selectorX = attachedObject.X + ((selectorWidth / 3) * _value);
            int selectorY = (int) attachedObject.Center.Y - selectorHeight / 2;

            Tint = SelectorColor;
            Draw(selectorWidth, selectorHeight, selectorX, selectorY, SelectorTexture);
        }
    }
}
