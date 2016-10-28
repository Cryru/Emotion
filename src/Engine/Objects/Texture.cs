using Microsoft.Xna.Framework.Graphics;

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
    /// A texture object for storing and loading textures in an event free way and 
    /// interfacing with the engine's Animation object.  
    /// </summary>
    public class Texture
    {
        #region "Initializers"
        /// <summary>
        /// Create a new Texture Object by specifying a texture.
        /// </summary>
        public Texture(Texture2D Image)
        {
            this.Image = Image;
        }
        /// <summary>
        /// Create a new Texture Object by specifying a texture name.
        /// </summary>
        public Texture(string ImageName)
        {
            this.ImageName = ImageName;
        }
        /// <summary>
        /// Create a new Texture Object by specifying an animation object.
        /// </summary>
        public Texture(Animation Animation)
        {
            this.Animation = Animation;
        }
        /// <summary>
        /// Create a new Texture Object by not specifying anything.
        /// This will load the missingTexture from Core.
        /// </summary>
        public Texture()
        {
            Image = Core.missingTexture.Image;
        }
        #endregion
        #region "Public Accessors"
        /// <summary>
        /// The image texture. If an invalid input is entered the missingTexture from Core will be loaded instead.
        /// If linked with an animation object, the current frame will be returned.
        /// </summary>
        public Texture2D Image
        {
            get
            {
                //Check if animation is linked.
                if (Animation != null)
                {
                    return Animation.FrameTexture;
                }

                //Check if the image is empty.
                if (_Image == null)
                {
                    //If it is assign the missing image image.
                    ImageName = "missing";
                }
                //Return the private property image.
                return _Image;
            }
            set
            {
                //Check if animation is linked.
                if (Animation != null)
                {
                    return;
                }

                //Check if trying to load the same image.
                if (value == _Image && Force == false || value == null)
                {
                    return;
                }
                //Set the texture to be the value.
                _Image = value;
                //Assign the name of the image.
                _ImageName = value.Name;
                //Check if the name is empty, in which case assign a placeholder.
                if (value.Name == "" || value.Name == null)
                {
                    _ImageName = "unnamed";
                }
            }
        }
        /// <summary>
        /// The image texture's name. If set, the object will load a texture from the Content Pipeline using
        /// the name as the texture's path. In case of invalid input the missingTexture will be loaded.
        /// </summary>
        public string ImageName
        {
            get
            {
                //Check if an animation is linked.
                if (Animation != null)
                {
                    return Animation.SheetName + " on frame " + Animation.Frame;
                }

                if (_ImageName == "" || _ImageName == null)
                {
                    ImageName = "blank";
                }
                return _ImageName;
            }
            set
            {
                //Check if an animation is linked.
                if (Animation != null)
                {
                    return;
                }

                //Check if the value is "blank" or empty.
                if (value == "blank" || value == "" || value == null)
                {
                    _ImageName = value; //Assign the name from variable.
                    _Image = Core.blankTexture.Image; //Assign the blank texture.
                }
                else
                {
                    //Check if trying to load the same image.
                    if (value == _ImageName && Force == false)
                    {
                        return;
                    }
                    //Assign the image's name.
                    _ImageName = value;

                    //Load a texture by the name.
                    _Image = Content.Load.Texture(_ImageName);
                }
            }
        }
        /// <summary>
        /// If true the object will be forced to load the inputted texture or texture name even 
        /// if the same texture, or a texture with the same name is inputted.
        /// </summary>
        public bool Force = false;
        /// <summary>
        /// An animation object that will be linked to this object.
        /// When linked the current texture will not be returned, but the animation's current frame will be returned.
        /// Also attempting to set the texture will not work.
        /// To resume normal operation of the object the animation must be unset.
        /// </summary>
        public Animation Animation;
        #endregion
        #region "Private Holders"
        /// <summary>
        /// The texture's name.
        /// </summary>
        private string _ImageName = "";
        /// <summary>
        /// The texture.
        /// </summary>
        private Texture2D _Image;
        #endregion
    }
}
