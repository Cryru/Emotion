using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;
using SoulEngine.Events;
using SoulEngine.Objects.Components.Helpers;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A managed texture object.
    /// </summary>
    public class ActiveTexture : Component
    {
        #region "Declarations"
        /// <summary>
        /// The active texture's drawing texture.
        /// </summary>
        public Texture2D Texture
        {
            get
            {
                if (TextureMode == TextureMode.Animate && attachedObject.HasComponent<Animation>()) return attachedObject.Component<Animation>().Frames[attachedObject.Component<Animation>().FrameTotal];
                if (TextureMode == TextureMode.Stretch) return _xnaTexture;
                if (_texture == null) return AssetManager.MissingTexture;
                return _texture as Texture2D;
            }
            set
            {
                _xnaTexture = value;
            }
        }
        /// <summary>
        /// The part of the texture to draw.
        /// </summary>
        public Rectangle DrawArea;
        /// <summary>
        /// The way to texture should be rendered to fill its bounds.
        /// </summary>
        public TextureMode TextureMode = 0;
        /// <summary>
        /// Used to mirror textures horizontally or vertically. Used by the renderer component.
        /// </summary>
        public SpriteEffects MirrorEffects = SpriteEffects.None;
        /// <summary>
        /// Used to decide the texture opacity. Used by the renderer component.
        /// </summary>
        public float Opacity = 1f;
        /// <summary>
        /// Used to color the texture. Used by the renderer component.
        /// </summary>
        public Color Tint = Color.White;
        #region "Private"
        private RenderTarget2D _texture;
        private Texture2D _xnaTexture;
        #endregion
        #endregion

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextureMode"></param>
        public ActiveTexture(TextureMode TextureMode = 0)
        {
            this.TextureMode = TextureMode;
            Texture = AssetManager.MissingTexture;
            DrawArea = _xnaTexture.Bounds; //Render whole texture if area not specified.
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Texture"></param>
        /// <param name="TextureMode"></param>
        /// <param name="DrawArea"></param>
        public ActiveTexture(Texture2D Texture, TextureMode TextureMode = 0, Rectangle DrawArea = new Rectangle())
        {
            this.TextureMode = TextureMode;
            this.Texture = Texture;
            this.DrawArea = DrawArea;
        }
        #endregion

        //Main functions.
        #region "Functions"
        /// <summary>
        /// 
        /// </summary>
        public override void Compose()
        {
            if (TextureMode != TextureMode.Tile) return;

            //Check empty draw area.
            if (DrawArea == new Rectangle()) DrawArea = _xnaTexture.Bounds;

            //Get the size of the object, or if a Transform component is attached get the bounds from it.
            Rectangle Bounds = DrawArea;
            if (attachedObject != null && attachedObject.HasComponent<Transform>())
            {
                Bounds = attachedObject.Component<Transform>().Bounds;
            }

            //Start drawing on internal target.
            Context.ink.StartRenderTarget(ref _texture, Bounds.Width, Bounds.Height);

            //Draw the texture depending on how we are stretching.
            switch (TextureMode)
            {
                case TextureMode.Tile:
                    //Calculate the limit of the texture tile, we go higher as the rendertarget will not allow out of bounds drawing anyway.
                    int xLimit = (int) Math.Ceiling((double) Bounds.Width / _xnaTexture.Width);
                    int yLimit = (int) Math.Ceiling((double) Bounds.Height / _xnaTexture.Height);

                    for (int x = 0; x < xLimit; x++)
                    {
                        for (int y = 0; y < yLimit; y++)
                        {
                            Context.ink.Draw(_xnaTexture, new Rectangle(_xnaTexture.Width * x, _xnaTexture.Height * y,
                                _xnaTexture.Width, _xnaTexture.Height), DrawArea, Color.White);
                        }
                    }
                    break;
            }

            //Stop drawing.
            Context.ink.EndRenderTarget();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Draw()
        {
            //Check if empty texture, sometimes it happens.
            if (Texture == null) return;

            //Get some drawing properties.
            int X = attachedObject.GetProperty("X", 0);
            int Y = attachedObject.GetProperty("Y", 0);
            int Width = attachedObject.GetProperty("Width", Texture.Width);
            int Height = attachedObject.GetProperty("Height", Texture.Height);
            float Rotation = attachedObject.GetProperty("Rotation", 0f);

            Rectangle DrawBounds = new Rectangle(X, Y, Width, Height);

            //Correct bounds to center origin.
            DrawBounds = new Rectangle(new Point((DrawBounds.X + DrawBounds.Width / 2),
                (DrawBounds.Y + DrawBounds.Height / 2)),
                new Point(DrawBounds.Width, DrawBounds.Height));

            //Draw the object through XNA's SpriteBatch.
            Context.ink.Draw(Texture,
                DrawBounds,
                null,
                Tint * Opacity,
                Rotation,
                new Vector2((float)Texture.Width / 2, (float)Texture.Height / 2),
                MirrorEffects,
                1.0f);
        }
        #endregion

        #region "Component Interface"
        public override void Update() { }
        #endregion

        //Other
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
                    // TODO: dispose managed state (managed objects).
                }

                //Free resources.
                _texture = null;
                SoulEngine.Events.ESystem.Remove(this);
                attachedObject = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }
        #endregion
    }
}
