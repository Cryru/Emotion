using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A basis for game objects.
    /// </summary>
    class ObjectBase
    {
        #region "Declarations"
        #region "Location, size, and rotation"
        /// <summary>
        /// The location of the object on the X axis.
        /// </summary>
        public float X = 0;
        /// <summary>
        /// The location of the object on the Y axis.
        /// </summary>
        public float Y = 0;
        /// <summary>
        /// The location of the object as a Vector2.
        /// </summary>
        public Vector2 Location 
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <summary>
        /// The width of the object.
        /// </summary>
        public int Width = 10;
        /// <summary>
        /// The height of the object.
        /// </summary>
        public int Height = 10;
        /// <summary>
        /// The size of the object as a Vector2.
        /// </summary>
        public Vector2 Size 
        {
            get
            {
                return new Vector2(Width, Height);
            }
            set
            {
                Width = (int) value.X;
                Height = (int) value.Y;
            }
        }
        /// <summary>
        /// The rectangle that represents the object.
        /// </summary>
        public Rectangle Bounds 
        {
            get
            {
                return new Rectangle(Location.ToPoint(), new Point(Width,Height));
            }
            set
            {
                X = value.Location.X;
                Y = value.Location.Y;
                Width = value.Size.X;
                Height = value.Size.Y;
            }
        }
        /// <summary>
        /// The rotation of the object in radians.
        /// </summary>
        public float RotationRadians = 0;
        /// <summary>
        /// The rotation of the object in degrees.
        /// </summary>
        public int RotationDegrees 
        //This value is stored as radians when entered as the drawing required for the rotation to be in radians.
        {
            get
            {
                return Core.RadiansToDegrees(RotationRadians);
            }
            set
            {
                RotationRadians = Core.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// The center of the object.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return new Vector2(Location.X + Width / 2, Location.Y + Height / 2);
            }
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }
        #endregion
        #region "Texture and drawing variables"
        /// <summary>
        /// The texture object the object should be drawn as.
        /// </summary>
            public Texture Image;
        /// <summary>
        /// The hue of the object.
        /// </summary>
            public Color Color = Color.White;
        /// <summary>
        /// The object's visibiliy.
        /// </summary>
            public bool Visible = true;
        /// <summary>
        /// The opacity of the object. (0-1)
        /// </summary>
            public float Opacity = 1f;
        /// <summary>
        /// SpriteEffects for horizontal and/or vertical flipping.
        /// </summary>
            public SpriteEffects MirrorEffects = SpriteEffects.None;
            #endregion
        #region "Other"
            /// <summary>
            /// //A list of strings you can use to store values inside the object.
            /// </summary>
            public List<string> Tags = new List<string>(); 
            #endregion
        #endregion

        /// <summary>
        /// Initializes an object.
        /// </summary>
        /// <param name="Image">The texture object that represents the object.</param>
        public ObjectBase(Texture Image = null)
        {
            //Check if image is null.
            if (Image == null)
            {
                Image = new Texture();
            }

            //Assign the texture to the property.
            this.Image = Image;
        }
        /// <summary>
        /// Draws the object according to it's specified variables, or according to the object's variables.
        /// By default these are the properties of the object.
        /// </summary>
        /// <param name="DrawImage">The image.</param>
        /// <param name="DrawBounds">The size and location.</param>
        /// <param name="DrawTint">The color.</param>
        /// <param name="DrawOpacity">The opacity.</param>
        /// <param name="Rotation">The rotation in radians.</param>
        /// <param name="DrawOrigin">The origin point for rotation, by default this is the center.</param>
        /// <param name="DrawEffects">Flipping and mirroring effects.</param>
        protected void DrawObject(Texture2D DrawImage = null, Rectangle DrawBounds = new Rectangle(), Color DrawTint = new Color(), float DrawOpacity = -1f, 
            float Rotation = -1f, Vector2 DrawOrigin = new Vector2(), SpriteEffects DrawEffects = SpriteEffects.None)
        {
            //Check if not visible, in which case we are skipping drawing.
            if(Visible == false)
            {
                return;
            }

            //Apply Properties to empty values.
            if (DrawImage == null)
            {
                if (Image == null) Image = Core.missingTexture;
                DrawImage = Image.Image;
            }
            if(DrawBounds == new Rectangle())
            {
                //Offset the drawing location by half the object so the object's origin point is it's center, and it rotates around it.
                DrawBounds = new Rectangle(new Point((int)(X + Width / 2), (int)(Y + Height / 2)), new Point((int)Width, (int)Height));
            }
            else
            {
                DrawBounds = new Rectangle(new Point((int)(DrawBounds.X + DrawBounds.Width / 2), (int)(DrawBounds.Y + DrawBounds.Height / 2)), new Point((int)DrawBounds.Width, (int)DrawBounds.Height));
            }
            if(DrawTint == new Color())
            {
                DrawTint = Color;
            }
            if(DrawOpacity == -1f)
            {
                DrawOpacity = Opacity;
            }
            if(Rotation == -1f)
            {
                Rotation = RotationRadians;
            }
            if(DrawOrigin == new Vector2())
            {
                DrawOrigin = new Vector2((float)DrawImage.Width / 2, (float)DrawImage.Height / 2);
            }
            if(DrawEffects == SpriteEffects.None)
            {
                DrawEffects = MirrorEffects;
            }

            //Draw the object. To understand how this works read the documentation on objects.
            Core.ink.Draw(DrawImage, DrawBounds, null, DrawTint * DrawOpacity, Rotation, DrawOrigin, DrawEffects, 1.0f);
        }
        /// <summary>
        /// A draw function that can be overriden by children of this object.
        /// </summary>
        public virtual void Draw()
        {
            DrawObject();
        }
    }
}
