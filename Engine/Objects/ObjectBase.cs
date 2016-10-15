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
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // The base for game objects.                                               //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class ObjectBase
    {
        #region "Declarations"
        //Location, size, and rotation.
        public float X = 0; //The X axis location of the object.
        public float Y = 0; //The Y axis location of the object.
        public Vector2 Location //The location of the object as a Vector2.
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
        public int Width = 10; //The width of the object.
        public int Height = 10; //The height of the object.
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
        } //The rectangle that represents the object.
        public float RotationRadians = 0; //The rotation of the object in radians.
        public int RotationDegrees //The rotation to of the object in degrees. 
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
        } //The center point of the object.

        //Texture and drawing variables.
        public Texture Image; //The texture of the object.
        public Color Color = Color.White; //The color hue of the image.
        public bool Visible = true; //Whether the object should be displayed.
        public float Opacity = 1f; //The opacity of the object. This value can range from 0 to 1.
        public SpriteEffects MirrorEffects = SpriteEffects.None; //Mirroring the texture.

        //Other
        public List<string> Tags = new List<string>(); //A list of strings you can use to store values inside the object.
        #endregion

        //The initializer.
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
        //Draws the object according to it's specified variables, or according to the object's variables.
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
        //The draw function that can be overriden by children of this object.
        public virtual void Draw()
        {
            DrawObject();
        }
    }
}
