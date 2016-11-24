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
    /// A base for UI based objects.
    /// </summary>
    public class UIObjectBase : ObjectBase
    {
        #region "Declarations"
        #region "Settings"
        /// <summary>
        /// Whether the object is toggleable.
        /// </summary>
        public bool Toggleable = false;
        /// <summary>
        /// Whether the object is enabled - can send events.
        /// </summary>
        public bool Enabled = true;
        #endregion
        #region "Images"
        /// <summary>
        /// The image to be displayed when nothing is happening to the object.
        /// </summary>
        public Texture ImageNone;
        /// <summary>
        /// The image to be displayed when the object is mouse overed.
        /// </summary>
        public Texture ImageMouseOver;
        /// <summary>
        /// The image to be displayed when the object is clicked.
        /// </summary>
        public Texture ImageMouseClick;
        #endregion
        #region "Events"
        /// <summary>
        /// When the mouse pointer enters the object.
        /// </summary>
        public Internal.Event<UIObjectBase> onMouseEnter = new Internal.Event<UIObjectBase>();
        /// <summary>
        /// When the mouse pointer exits the object.
        /// </summary>
        public Internal.Event<UIObjectBase> onMouseLeave = new Internal.Event<UIObjectBase>();
        /// <summary>
        /// When the mouse clicks on the object.
        /// </summary>
        public Internal.Event<UIObjectBase> onMouseDown = new Internal.Event<UIObjectBase>();
        /// <summary>
        /// When the mouse is let go on the object.
        /// </summary>
        public Internal.Event<UIObjectBase> onMouseUp = new Internal.Event<UIObjectBase>();
        /// <summary>
        /// When the mouse right clicks on the object.
        /// </summary>
        public Internal.Event<UIObjectBase> onMouseRightClickDown = new Internal.Event<UIObjectBase>();
        /// <summary>
        /// When the mouse's right click is let go on the object.
        /// </summary>
        public Internal.Event<UIObjectBase> onMouseRightClickUp = new Internal.Event<UIObjectBase>();
        #endregion
        #region "Other"
        /// <summary>
        /// UI object status
        /// </summary>
        public Status Status
        {
            get
            {
                return _Status;
            }
        }
        /// <summary>
        /// The private status property.
        /// </summary>
        private Status _Status = Status.None;
        #endregion
        #region "Internal"
        /// <summary>
        /// The gray out effect.
        /// </summary>
        private ObjectBase grayingOut = new ObjectBase(new Texture(Core.blankTexture.Image));
        #region "Triggers"
        private bool trigger_mouseover_done = false;
        private bool trigger_click_done = false;
        private bool trigger_up_done = false;
        private bool trigger_out_done = false;
        private bool toggleable_status_on = false;
        private bool trigger_mousewasdown = false;
        private bool trigger_mouserightclickwasup = false;
        private bool trigger_mouserightclickwasdown = false;
        #endregion
        #endregion
        #endregion

        /// <summary>
        /// The initializer.
        /// </summary>
        /// <param name="Image">The image to display at all times.</param>
        /// <param name="ImageMouseOver">The image to display when the object is being mouseovered.</param>
        /// <param name="ImageMouseClick">The image to display when the object is clicked.</param>
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

        /// <summary>
        /// Draws the object and updates its status.
        /// </summary>
        /// <param name="skipUpdate">Skips updating the status.</param>
        public void Draw(bool skipUpdate = false)
        {
            //Check if update is to be skipped.
            if(skipUpdate == false) Update();

            //Check not enabled in which case we want to draw a graying out effect.
            if(Enabled == false)
            {
                grayingOut.X = Bounds.X;
                grayingOut.Y = Bounds.Y;
                grayingOut.Width = Bounds.Width;
                grayingOut.Height = Bounds.Height;

                grayingOut.Color = Color.Gray;
                grayingOut.Opacity = 0.8f;

                grayingOut.Draw();
            }

            //Set the image based on the state.
            switch(_Status)
            {
                case Status.Clicked:
                case Status.Selected:
                    Image = ImageMouseClick;
                    if (ImageMouseClick.Image == Core.missingTexture.Image) Image = ImageNone;
                    break;
                case Status.Mouseovered:
                    Image = ImageMouseOver;
                    if (ImageMouseOver.Image == Core.missingTexture.Image) Image = ImageNone;
                    break;
                case Status.None:
                    Image = ImageNone;
                    break;
            }

            //Draw the object.
            DrawObject();
        }

        /// <summary>
        /// Updates the status of the object.
        /// </summary>
        public void Update()
        {
            if (Enabled == false) return;

            //Get the mouse location.
            Rectangle mouse = new Rectangle(Input.getMousePos().ToPoint(), new Point(1, 1));

            //Check if mouse is within the object.
            if (Bounds.Intersects(mouse))
            {
                //Set the status to mouseovered.
                if (Toggleable == false || _Status == Status.None) _Status = Status.Mouseovered;

                //Reset the exit trigger.
                trigger_out_done = false;

                //Check if the mouse over event has been triggered.
                if (trigger_mouseover_done == false)
                {
                    //If not, then invoke the event.
                    trigger_mouseover_done = true;
                    onMouseDown.Trigger(this);
                }

                //Check if the mouse button is pressed.
                if (Input.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                {
                    //Check if the object is toggleable.
                    if (Toggleable == false) _Status = Status.Clicked;

                    //Check if the mouse up event has been triggered.
                    if (trigger_click_done == false)
                    {
                        trigger_click_done = true;
                        onMouseDown.Trigger(this);
                    }

                    //Activate the trigger that the mouse was held down.
                    trigger_mousewasdown = true;
                }

                //Check if the buton is released, and was pressed last frame, AKA was just let go.
                if (Input.currentFrameMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && trigger_mousewasdown == true)
                {
                    trigger_mousewasdown = false;

                    //Check if the mouse up event has been triggered.
                    if (trigger_up_done == false)
                    {
                        trigger_up_done = true;
                        onMouseUp.Trigger(this);
                    }

                    if (Toggleable == true && toggleable_status_on == false)
                    {
                        _Status = Status.Selected;
                        toggleable_status_on = true;
                    }
                    else if (Toggleable == true && toggleable_status_on == true)
                    {
                        _Status = Status.None;
                        toggleable_status_on = false;
                    }
                }

                //Check if right click is down.
                if (Input.currentFrameMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && trigger_mouserightclickwasup == true)
                {
                    onMouseRightClickDown.Trigger(this);
                    trigger_mouserightclickwasup = false;
                    trigger_mouserightclickwasdown = true;
                }
                else if(Input.currentFrameMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                {
                    if(trigger_mouserightclickwasdown == true)
                    {
                        onMouseRightClickUp.Trigger(this);
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
                    onMouseLeave.Trigger(this);
                }

                //Reset triggers.
                trigger_up_done = false;
                trigger_click_done = false;
                trigger_mouseover_done = false;

                if (toggleable_status_on == false) _Status = Status.None;
            }

        }
    }

    /// <summary>
    /// The status of a UI object.
    /// </summary>
    public enum Status
    {
        None,
        Clicked,
        Mouseovered,
        Selected
    }
}
