using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // The base for UI objects.                                                 //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class UIObjectBase : ObjectBase
    {
        #region "Declarations"
        //Settings
        public bool Toggleable = false; //When on the object will be toggled between clicked and not.
        public bool Enabled = true; //Whether the object is enabled and can send/receive events.

        //Images
        public Texture ImageNone; //The image to be displayed when nothing is happening to the object.
        public Texture ImageMouseOver; //The image to be displayed when the object is mouse overed.
        public Texture ImageMouseClick; //The image to be displayed when the object is clicked.

        //Events
        public Action onMouseEnter; //When the mouse pointer enters the object.
        public Action onMouseLeave; //When the mouse pointer exits the object.
        public Action onMouseDown; //When the mouse clicks on the object.
        public Action onMouseUp; //When the mouse is let go on the object.
        public Action onMouseRightClickDown; //When the mouse right clicks on the object.
        public Action onMouseRightClickUp; //When the mouse's right click is let go on the object.

        //Status
        public enum Mode
        {
            None,
            Clicked,
            Mouseovered,
            Selected
        }
        public Mode Status
        {
            get
            {
                return _Status;
            }

        }

        //Parenting
        public ObjectBase Parent; //The parent of the object within which this object will be rendered.

        //Text
        public TextObject Text; //The text object that will be rendered within.

        //Internal
        private Mode _Status = Mode.None; //The status editable declaration.
        private Rectangle offsetBounds; //The offset of the object's location, used when under a parent.
        private ObjectBase grayingOut = new ObjectBase(new Texture(Core.blankTexture)); //The gray out effect.
        private bool trigger_mouseover_done = false;
        private bool trigger_click_done = false;
        private bool trigger_up_done = false;
        private bool trigger_out_done = false;
        private bool toggleable_status_on = false;
        private bool trigger_mousewasdown = false;
        private bool trigger_mouserightclickwasup = false;
        private bool trigger_mouserightclickwasdown = false;
        #endregion

        //The initializer.
        public UIObjectBase(Texture Image = null, Texture ImageMouseOver = null, Texture ImageMouseClick = null)
        {
            //Check if image is null.
            if(Image == null)
            {
                Image = new Texture();
            }
            if (ImageMouseOver == null)
            {
                ImageMouseOver = new Texture();
            }
            if (ImageMouseClick == null)
            {
                ImageMouseClick = new Texture();
            }

            //Assign the texture to the property.
            ImageNone = Image;
            this.ImageMouseOver = ImageMouseOver;
            this.ImageMouseClick = ImageMouseClick;
        }

        //Draws the object using the ObjectBase's drawing method.
        //The object's updating can be skipped. This could be done when optimizing and
        //the user wishes to separate the update from the drawing call.
        public void Draw(bool skipUpdate = false)
        {
            //Check if update is to be skipped.
            if(skipUpdate == false) Update();

            //Check not enabled in which case we want to draw a graying out effect.
            if(Enabled == true)
            {
                grayingOut.X = offsetBounds.X;
                grayingOut.Y = offsetBounds.Y;
                grayingOut.Width = offsetBounds.Width;
                grayingOut.Height = offsetBounds.Height;

                grayingOut.Color = Color.Gray;
                grayingOut.Opacity = 0.8f;

                grayingOut.Draw();
            }

            //Set the image based on the state.
            switch(_Status)
            {
                case Mode.Clicked:
                case Mode.Selected:
                    Image = ImageMouseClick;
                    if (ImageMouseClick.Image == Core.missingimg) Image = ImageNone;
                    break;
                case Mode.Mouseovered:
                    Image = ImageMouseOver;
                    if (ImageMouseOver.Image == Core.missingimg) Image = ImageNone;
                    break;
                case Mode.None:
                    Image = ImageNone;
                    break;
            }

            //Assign offset variable from actual
            offsetBounds = Bounds;

            //Check for parent
            if (Parent != null)
            {
                //Offset the object's location from the parent's location.
                offsetBounds = new Rectangle(Bounds.X + (int)Parent.X, Bounds.Y + (int)Parent.Y,
                    Bounds.Width, Bounds.Height);
                //Get the parent's visibility.
                Visible = Parent.Visible;
            }

            //Draw the object.
            DrawObject(DrawBounds: offsetBounds);
        }

        //Updates the status of the object and such.
        public void Update()
        {
            if (Enabled == false) return;

            //Get the mouse location.
            Rectangle mouse = new Rectangle(Core.WorldMousePos().ToPoint(), new Point(1, 1));

            bool touchMode = false;
#if ANDROID
            touchMode = true;
#endif

            //Check if mouse is within the object.
            if (offsetBounds.Intersects(mouse))
            {
                //Set the status to mouseovered.
                if (Toggleable == false || _Status == Mode.None) _Status = Mode.Mouseovered;

                //Reset the exit trigger.
                trigger_out_done = false;

                //Check if the mouse over event has been triggered.
                if (trigger_mouseover_done == false)
                {
                    //If not, then invoke the event.
                    trigger_mouseover_done = true;
                    onMouseEnter?.Invoke();
                }

                //Check if the mouse button is pressed.
                if (Core.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed || touchMode == true)
                {
                    //Check if the object is toggleable.
                    if (Toggleable == false) _Status = Mode.Clicked;

                    //Check if the mouse up event has been triggered.
                    if (trigger_click_done == false)
                    {
                        trigger_click_done = true;
                        onMouseDown?.Invoke();
                    }

                    //Activate the trigger that the mouse was held down.
                    trigger_mousewasdown = true;
                }

                //Check if the buton is released, and was pressed last frame, AKA was just let go.
                if (Core.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && trigger_mousewasdown == true)
                {
                    trigger_mousewasdown = false;

                    //Check if the mouse up event has been triggered.
                    if (trigger_up_done == false)
                    {
                        trigger_up_done = true;
                        onMouseUp?.Invoke();
                    }

                    if (Toggleable == true && toggleable_status_on == false)
                    {
                        _Status = Mode.Selected;
                        toggleable_status_on = true;
                    }
                    else if (Toggleable == true && toggleable_status_on == true)
                    {
                        _Status = Mode.None;
                        toggleable_status_on = false;
                    }
                }

                //Check if right click is down.
                if (Core.currentFrameMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && trigger_mouserightclickwasup == true)
                {
                    onMouseRightClickDown?.Invoke();
                    trigger_mouserightclickwasup = false;
                    trigger_mouserightclickwasdown = true;
                }
                else if(Core.currentFrameMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                {
                    if(trigger_mouserightclickwasdown == true)
                    {
                        onMouseRightClickUp?.Invoke();
                        trigger_mouserightclickwasdown = false;
                    }
                    trigger_mouserightclickwasup = true;
                }

            }
            else
            {
                //When the mouse is not within the object.

                //Check if the mouse was in and is not now.
                if(trigger_mouseover_done == true && trigger_out_done == false)
                {
                    trigger_out_done = true;
                    onMouseLeave?.Invoke();
                }

                //Reset triggers.
                trigger_up_done = false;
                trigger_click_done = false;
                trigger_mouseover_done = false;

                if (toggleable_status_on == false) _Status = Mode.None;
            }

        }
    }
}
