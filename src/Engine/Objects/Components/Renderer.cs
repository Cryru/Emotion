using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Hosts and handles textures and drawing.
    /// </summary>
    public class Renderer : Component
    {
        #region "Variables"
        //Main variables.
        #region "Primary"
        #endregion
        //Private variables.
        #region "Private"

        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// Component used to render a game object in 2D space. Requires an ActiveTexture and a Transform component.
        /// </summary>
        public Renderer()
        {

        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// Draws the object based on parameters specified by its other components. Make sure to call ink.Begin() first.
        /// </summary>
        public override void Draw()
        {
            //Define drawing properties.
            Texture2D DrawImage = AssetManager.MissingTexture;
            Color DrawTint = Color.White;
            float DrawOpacity = 1f;
            SpriteEffects DrawEffects = SpriteEffects.None;
            Rectangle DrawBounds = new Rectangle(0, 0, 50, 50);
            float Rotation = 0f;

            //Check for components to overwrite default drawing properties.
            if (attachedObject.HasComponent<ActiveTexture>())
            {
                DrawImage = attachedObject.Component<ActiveTexture>().Texture;
                DrawTint = attachedObject.Component<ActiveTexture>().Tint;
                DrawOpacity = attachedObject.Component<ActiveTexture>().Opacity;
                DrawEffects = attachedObject.Component<ActiveTexture>().MirrorEffects;
            }
            if (attachedObject.HasComponent<Transform>())
            {
                DrawBounds = attachedObject.Component<Transform>().Bounds;
                Rotation = attachedObject.Component<Transform>().Rotation;
            }

            //Correct bounds to center origin.
            DrawBounds = new Rectangle(new Point((DrawBounds.X + DrawBounds.Width / 2), 
                (DrawBounds.Y + DrawBounds.Height / 2)), 
                new Point(DrawBounds.Width, DrawBounds.Height));

            //Draw the object through XNA's SpriteBatch.
            Context.ink.Draw(DrawImage,
                DrawBounds,
                null,
                DrawTint * DrawOpacity,
                Rotation,
                new Vector2((float)DrawImage.Width / 2, (float)DrawImage.Height / 2),
                DrawEffects,
                1.0f);
        }

        public override void Update()
        {

        }
        #endregion
        //Private functions.
        #region "Internal Functions"

        #endregion
    }
}
