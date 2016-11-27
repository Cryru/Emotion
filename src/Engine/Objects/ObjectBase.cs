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
    public class ObjectBase
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
        #region "Parenting"
        /// <summary>
        /// Children of this object.
        /// </summary>
        public List<ObjectBase> Children = new List<ObjectBase>();
        /// <summary>
        /// Whether children of the object should fill in the full size of the object.
        /// </summary>
        public bool ChildrenFill = true;
        /// <summary>
        /// If the children are filling the parent they will be offset by the padding.
        /// </summary>
        public Vector2 Padding = Vector2.Zero;
        /// <summary>
        /// Whether children should inherit the visibility of the object.
        /// </summary>
        public bool ChildrenInheritVisibiity = true;
        /// <summary>
        /// Whether to draw the children above the parent.
        /// </summary>
        public bool ChildrenOnTop = false;
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
            float Rotation = -1f, Vector2 DrawOrigin = new Vector2(), SpriteEffects DrawEffects = SpriteEffects.None, bool ignoreChildren = false)
        {
            //Process parenting, for when the children are on top.
            if(ignoreChildren == false && ChildrenOnTop == false) ProcessParenting();

            //Check if not visible, in which case we are skipping drawing.
            if (Visible == false)
            {
                //Process parenting for when the children are on top.
                if (ignoreChildren == false && ChildrenOnTop == true) ProcessParenting();
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

            //Process parenting.
            if (ignoreChildren == false && ChildrenOnTop == true) ProcessParenting();
        }
        /// <summary>
        /// Processes parenting.
        /// </summary>
        private void ProcessParenting()
        {
            //For each child object.
            foreach (var child in Children)
            {
                //Check if the child is null.
                if (child == null) continue;

                //Store the location of the child.
                Rectangle tempHolder = child.Bounds;

                //Check if filling, or not and assign the offset location and appropriate size.
                if(ChildrenFill == true)
                {
                    child.Bounds = new Rectangle((int)(X + child.X - Padding.X), (int)(Y + child.Y - Padding.Y), Width + (int) (Padding.X * 2), Height + (int)(Padding.Y * 2));
                }
                else
                {
                    child.Bounds = new Rectangle((int)(X + child.X), (int)(Y + child.Y), child.Width, child.Height);
                }
                
                //Check if inheriting visibility.
                if(ChildrenInheritVisibiity == true)
                {
                    child.Visible = Visible;
                }

                //Draw the child.
                child.Draw();

                //Restore the location of the child.
                child.Bounds = tempHolder;
            }
        }
        /// <summary>
        /// A draw function that can be overriden by children of this object.
        /// </summary>
        public virtual void Draw()
        {
            DrawObject();
        }

        #region "Functions"
        /// <summary>
        /// Center the object within the window.
        /// </summary>
        public void ObjectCenter()
        {
            ObjectCenterX();
            ObjectCenterY();
        }
        /// <summary>
        /// Center the object within the window on the X axis.
        /// </summary>
        public void ObjectCenterX()
        {
            X = Settings.game_width / 2 - Width / 2;
        }
        /// <summary>
        /// Center the object within the window on the Y axis.
        /// </summary>
        public void ObjectCenterY()
        {
            Y = Settings.game_height / 2 - Height / 2;
        }
        /// <summary>
        /// Center the object within another bigger object.
        /// </summary>
        /// <param name="obj">The object to center inside.</param>
        public void ObjectCenterOnObject(ObjectBase obj)
        {
            Center = obj.Center;
            Center = obj.Center;
        }
        /// <summary>
        /// Makes the object fit the whole screen.
        /// </summary>
        public void ObjectFullscreen()
        {
            Width = Settings.game_width;
            Height = Settings.game_height;
            X = 0;
            Y = 0;
        }
        /// <summary>
        /// Copy the location, size, and image of another object.
        /// </summary>
        /// <param name="obj">The object to copy size and location from.</param>
        /// <param name="copyImage">Whether to copy the image too.</param>
        public void ObjectCopy(ObjectBase obj, bool copyImage = true)
        {
            Location = obj.Location;
            Size = obj.Size;
            if(copyImage == true) Image = obj.Image;
        }
        /// <summary>
        /// Sets the size of the object to the size of its image.
        /// </summary>
        public void ObjectSizeFromImage()
        {
            Width = Image.Image.Width;
            Height = Image.Image.Height;
        }
        #endregion

    }
}
