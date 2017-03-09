using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects.Components;
using SoulEngine.Objects;

namespace SoulEngine.Legacy.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // This code is part of the SoulEngine backwards compatibility layer.       //
    // Original Repository: https://github.com/Cryru/SoulEngine-2016            //
    //////////////////////////////////////////////////////////////////////////////
    public class ObjectBase
    {
        private GameObject actualObject;

        #region "Declarations"
        public float X
        {
            get
            {
                return actualObject.Component<Transform>().X;
            }
            set
            {
                actualObject.Component<Transform>().X = value;
            }
        }
        public float Y
        {
            get
            {
                return actualObject.Component<Transform>().Y;
            }
            set
            {
                actualObject.Component<Transform>().Y = value;
            }
        }
        public Vector2 Location
        {
            get
            {
                return actualObject.Component<Transform>().Position;
            }
            set
            {
                actualObject.Component<Transform>().Position = value;
            }
        }
        public int Width
        {
            get
            {
                return (int) actualObject.Component<Transform>().Width;
            }
            set
            {
                actualObject.Component<Transform>().Width = value;
            }
        }
        public int Height
        {
            get
            {
                return (int)actualObject.Component<Transform>().Height;
            }
            set
            {
                actualObject.Component<Transform>().Height = value;
            }
        }
        public Vector2 Size
        {
            get
            {
                return actualObject.Component<Transform>().Size;
            }
            set
            {
                actualObject.Component<Transform>().Size = value;
            }
        }
        public Rectangle Bounds
        {
            get
            {
                return actualObject.Component<Transform>().Bounds;
            }
            set
            {
                actualObject.Component<Transform>().Bounds = value;
            }
        }

        public float RotationRadians
        {
            get
            {
                return actualObject.Component<Transform>().Rotation;
            }
            set
            {
                actualObject.Component<Transform>().Rotation = value;
            }
        }
        public int RotationDegrees
        {
            get
            {
                return actualObject.Component<Transform>().RotationDegree;
            }
            set
            {
                actualObject.Component<Transform>().RotationDegree = value;
            }
        }

        public Vector2 Center
        {
            get
            {
                return actualObject.Component<Transform>().Center;
            }
            set
            {
                actualObject.Component<Transform>().Center = value;
            }
        }

        public Texture Image
        {
            get
            {
                return new Texture(actualObject.Component<ActiveTexture>().Texture);
            }
            set
            {
                actualObject.Component<ActiveTexture>().Texture = value.Image;
            }
        }
        public Color Color
        {
            get
            {
                return actualObject.Component<ActiveTexture>().Tint;
            }
            set
            {
                actualObject.Component<ActiveTexture>().Tint = value;
            }
        }
        public bool Visible
        {
            get
            {
                return actualObject.Component<ActiveTexture>().Opacity > 0;
            }
            set
            {
                actualObject.Component<ActiveTexture>().Opacity = value ? 1 : 0;
            }
        }
        public float Opacity
        {
            get
            {
                return actualObject.Component<ActiveTexture>().Opacity;
            }
    set
            {
                actualObject.Component<ActiveTexture>().Opacity = value;
            }
        }
        public SpriteEffects MirrorEffects
        {
            get
            {
                return actualObject.Component<ActiveTexture>().MirrorEffects;
            }
            set
            {
                actualObject.Component<ActiveTexture>().MirrorEffects = value;
            }
        }

        public ObjectBase Parent;
        public List<ObjectBase> Children = new List<ObjectBase>();
        public bool ChildrenFill = true;
        public Vector2 Padding = Vector2.Zero;
        public bool ChildrenInheritVisibiity = true;
        public bool ChildrenOnTop = false;

        #region "Effects"
        #region "Move"
        /// <summary>
        /// The location the object started from.
        /// </summary>
        protected Vector2 Effect_Move_StartLocation;
        /// <summary>
        /// The location the object is moving towards.
        /// </summary>
        protected Vector2 Effect_Move_TargetLocation;
        /// <summary>
        /// The total time the moving should take.
        /// </summary>
        protected float Effect_Move_Duration;
        /// <summary>
        /// The elapsed time.
        /// </summary>
        protected float Effect_Move_TimePassed;
        /// <summary>
        /// Whether the object is moving.
        /// </summary>
        protected bool Effect_Move_Running;
        #endregion
        /// <summary>
        /// Triggered when a move starts.
        /// </summary>
        public Event<ObjectBase> onMoveStart = new Event<ObjectBase>();
        /// <summary>
        /// Triggered when a move is complete.
        /// </summary>
        public Event<ObjectBase> onMoveComplete = new Event<ObjectBase>();
        #endregion
        #region "Other"
        /// <summary>
        /// //A list of strings you can use to store values inside the object.
        /// </summary>
        public Dictionary<string, string> Tags = new Dictionary<string, string>();
        #endregion
        #endregion


        public ObjectBase(Texture Image = null)
        {
            actualObject = GameObject.GenericDrawObject;

            //Check if image is null.
            if (Image == null)
            {
                actualObject.Component<ActiveTexture>().Texture = Image.Image;
            }
        }

        protected void DrawObject(Texture2D DrawImage = null, Rectangle DrawBounds = new Rectangle(), Color DrawTint = new Color(), float DrawOpacity = -1f,
            float Rotation = -1f, Vector2 DrawOrigin = new Vector2(), SpriteEffects DrawEffects = SpriteEffects.None, bool ignoreChildren = false)
        {
            //Process object effects.
            ProcessEffects();

            //Process parenting, for when the children are on top.
            if (ignoreChildren == false && ChildrenOnTop == false) ProcessParenting();

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
            if (DrawBounds == new Rectangle())
            {
                //Offset the drawing location by half the object so the object's origin point is it's center, and it rotates around it.
                DrawBounds = new Rectangle(new Point((int)(X + Width / 2), (int)(Y + Height / 2)), new Point((int)Width, (int)Height));
            }
            else
            {
                DrawBounds = new Rectangle(new Point((int)(DrawBounds.X + DrawBounds.Width / 2), (int)(DrawBounds.Y + DrawBounds.Height / 2)), new Point((int)DrawBounds.Width, (int)DrawBounds.Height));
            }
            if (DrawTint == new Color())
            {
                DrawTint = Color;
            }
            if (DrawOpacity == -1f)
            {
                DrawOpacity = Opacity;
            }
            if (Rotation == -1f)
            {
                Rotation = RotationRadians;
            }
            if (DrawOrigin == new Vector2())
            {
                DrawOrigin = new Vector2((float)DrawImage.Width / 2, (float)DrawImage.Height / 2);
            }
            if (DrawEffects == SpriteEffects.None)
            {
                DrawEffects = MirrorEffects;
            }

            //Draw the object. To understand how this works read the documentation on objects.
            Core.ink.Draw(DrawImage, DrawBounds, null, DrawTint * DrawOpacity, Rotation, DrawOrigin, DrawEffects, 1.0f);

            //Process parenting.
            if (ignoreChildren == false && ChildrenOnTop == true) ProcessParenting();
        }

        private void ProcessParenting()
        {
            //For each child object.
            foreach (var child in Children)
            {
                //Check if the child is null.
                if (child == null) continue;

                //Check if the child doesn't have the parent added as being this object.
                if (child.Parent != this) child.Parent = this;

                //Check if the original location of the children has been recorded.
                if (!child.Tags.ContainsKey("Parenting_OriginalLocationX") || !child.Tags.ContainsKey("Parenting_OriginalLocationY"))
                {
                    child.Tags["Parenting_OriginalLocationX"] = child.X.ToString();
                    child.Tags["Parenting_OriginalLocationY"] = child.X.ToString();
                }
                //Check if the parenting offset is set properly.
                if (!child.Tags.ContainsKey("Parenting_Offset") || child.Tags["Parenting_Offset"] != Width + "/" + Height + "/" + X + "/" + Y)
                {
                    child.Location = new Vector2(X - Padding.X + float.Parse(child.Tags["Parenting_OriginalLocationX"]), Y - Padding.Y + float.Parse(child.Tags["Parenting_OriginalLocationY"]));
                    child.Tags["Parenting_Offset"] = (Width + "/" + Height + "/" + X + "/" + Y);
                }

                //Check if filling, or not and assign the offset location and appropriate size.
                if (ChildrenFill == true)
                {
                    child.Size = new Vector2(Width + Padding.X * 2, Height + Padding.Y * 2);
                }

                //Check if inheriting visibility.
                if (ChildrenInheritVisibiity == true)
                {
                    child.Visible = Visible;
                    child.Opacity = Opacity;
                }

                //Draw the child.
                child.Draw();
            }
        }
        public virtual void Draw()
        {
            DrawObject();
        }

        #region "Functions"
        public void ObjectCenter()
        {
            ObjectCenterX();
            ObjectCenterY();
        }
        public void ObjectCenterX()
        {
            X = Settings.game_width / 2 - Width / 2;
        }
        public void ObjectCenterY()
        {
            Y = Settings.game_height / 2 - Height / 2;
        }
        public void ObjectCenterOnObject(ObjectBase obj)
        {
            Center = obj.Center;
            Center = obj.Center;
        }
        public void ObjectFullscreen()
        {
            Width = Settings.game_width;
            Height = Settings.game_height;
            X = 0;
            Y = 0;
        }
        public void ObjectCopy(ObjectBase obj, bool copyImage = true)
        {
            Location = obj.Location;
            Size = obj.Size;
            if (copyImage == true) Image = obj.Image;
        }
        public void ObjectSizeFromImage()
        {
            Width = Image.Image.Width;
            Height = Image.Image.Height;
        }
        #endregion
        #region "Effects"
        public void MoveTo(int Duration, Vector2 TargetLocation, bool OverwritePrevious = false)
        {
            //Check if moving already.
            if (Effect_Move_Running && OverwritePrevious == false) return;

            //Assign properties.
            Effect_Move_StartLocation = Location;
            Effect_Move_TargetLocation = TargetLocation;
            Effect_Move_TimePassed = 0;
            Effect_Move_Running = true;
            Effect_Move_Duration = Duration;

            //Trigger movement starting event.
            onMoveStart.Trigger(this);


        }
        private void ProcessEffects()
        {
            //Check if a movement effect is in use.
            if (Effect_Move_Running)
            {
                //Calculate the increments and add them.
                //Targetlocation minus current location divided by the total time minus the time that has passed. All of this multiplied by the milliseconds that passed since the last frame.
                //X += ((Effect_Move_TargetLocation.X - X) / (Effect_Move_Duration - Effect_Move_TimePassed)) * Core.frametime;
                //Y += ((Effect_Move_TargetLocation.Y - Y) / (Effect_Move_Duration - Effect_Move_TimePassed)) * Core.frametime;

                X = MathHelper.Lerp(Effect_Move_StartLocation.X, Effect_Move_TargetLocation.X, (Effect_Move_TimePassed / Effect_Move_Duration));
                Y = MathHelper.Lerp(Effect_Move_StartLocation.Y, Effect_Move_TargetLocation.Y, (Effect_Move_TimePassed / Effect_Move_Duration));
                //Vector2.Lerp(Location, Effect_Move_TargetLocation, )

                //Add the time passed.
                Effect_Move_TimePassed += Core.frametime;

                //Check if the movement time has passed.
                if (Effect_Move_TimePassed >= Effect_Move_Duration)
                {
                    //Set the location to the target location, this is to prevent fractions from messing up the positioning.
                    Location = Effect_Move_TargetLocation;
                    //Run the completion event.
                    Effect_Move_Running = false;
                    onMoveComplete.Trigger(this);
                }
            }

        }
        #endregion

    }
}
