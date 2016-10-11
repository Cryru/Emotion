using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // An active texture object for loading and storing                         //
    // 2DTextures in various ways including interfacing with                    //
    // the engine's Animation object.                                           //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Texture
    {
        //Initializers
        public Texture(Texture2D Image)
        {
            this.Image = Image;
        }
        public Texture(string ImageName)
        {
            this.ImageName = ImageName;
        }
        public Texture(Animation Animation)
        {
            this.Animation = Animation;
        }
        public Texture()
        {
            Image = Core.missingimg;
        }

        public Texture2D Image //The texture's image.
        {
            get
            {
                //Check if animation is linked.
                if (Animation != null)
                {
                    return Animation.GetFrameTexture();
                }

                //Check if the image is empty.
                if (_Image == null)
                {
                    //If it is assign the missing image image.
                    ImageName = "missingimg";
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
        public string ImageName //The texture's name, can also be used to assign a texture by name.
        {
            get
            {
                //Check if an animation is linked.
                if (Animation != null)
                {
                    return Animation.textureAtlas.ImageName + " on frame " + Animation.Frame;
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
                    _Image = Core.blankTexture; //Assign the blank texture.
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
                    //Lost a texture by the name.
                    _Image = Core.LoadTexture(_ImageName);
                }
            }
        }
        public bool Force = false; //Whether to force loading textures and not check if they are the same texture.
        public Animation Animation; //The animation object that can be linked with this one to automatically update textures from.

        //Private holders.
        private string _ImageName = ""; //The texture's name private property.
        private Texture2D _Image; //The texture's image private property.

    }
}
