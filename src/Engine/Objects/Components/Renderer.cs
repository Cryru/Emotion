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
        #region "Declarations"
        //Main variables.
        #region "Primary"
        #endregion
        //Private variables.
        #region "Private"

        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// Component used to render a game object in 2D space. For best results add an ActiveTexture and a Transform component.
        /// </summary>
        public Renderer()
        {

        }
        #endregion

        #region "Functions"
        /// <summary>
        /// Draws the object based on parameters specified by its other components. Make sure to call ink.Begin() first.
        /// </summary>
        public override void Draw()
        {
            if(attachedObject.HasComponent<ActiveTexture>())
            {
                DrawComponent(attachedObject.Component<ActiveTexture>().Texture, 
                    attachedObject.Component<ActiveTexture>().Tint, 
                    attachedObject.Component<ActiveTexture>().Opacity);
            }

            if (attachedObject.HasComponent<ActiveText>())
            {
                Rectangle temp = attachedObject.Component<Transform>().Bounds;

                if (!attachedObject.Component<ActiveText>().LockWidth) temp.Width = (int) attachedObject.Component<ActiveText>().Width;
                if (!attachedObject.Component<ActiveText>().LockHeight) temp.Height = (int) attachedObject.Component<ActiveText>().Height;

                DrawComponent(attachedObject.Component<ActiveText>().Texture, 
                    attachedObject.Component<ActiveText>().Color, 1f, temp);
            }
        }
        #endregion

        #region "Internal Functions"
        private void DrawComponent(Texture2D Texture, Color Tint, float Opacity, Rectangle? Bounds = null)
        {
            //Check if empty texture, sometimes it happens.
            if (Texture == null) return;

            //Define drawing properties.
            Texture2D DrawImage = Texture;
            Color DrawTint = Tint;
            float DrawOpacity = Opacity;
            SpriteEffects DrawEffects = SpriteEffects.None;
            //In case there is no transform attached we will take the texture's size and render it in its fullest.
            Rectangle DrawBounds = new Rectangle(0, 0, Texture.Width, Texture.Height);
            float Rotation = 0f;

            //Check for components to overwrite default drawing properties.
            if (attachedObject.HasComponent<ActiveTexture>())
            {
                DrawEffects = attachedObject.Component<ActiveTexture>().MirrorEffects;
            }
            if (attachedObject.HasComponent<Transform>())
            {
                if(Bounds == null) DrawBounds = attachedObject.Component<Transform>().Bounds;
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
        #endregion

        #region "Component Interface"
        public override void Compose(){}
        public override void Update() {}
        #endregion
    }
}
