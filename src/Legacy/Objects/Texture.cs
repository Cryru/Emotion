using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine.Legacy.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // This code is part of the SoulEngine backwards compatibility layer.       //
    // Original Repository: https://github.com/Cryru/SoulEngine-2016            //
    //////////////////////////////////////////////////////////////////////////////
    public class Texture
    {
        #region "Initializers"
        public Texture(Texture2D Image)
        {
            this.Image = Image;
        }
        public Texture(string ImageName)
        {
            this.ImageName = ImageName;
        }
        public Texture()
        {
            Image = Core.missingTexture.Image;
            ImageName = Core.missingTexture.ImageName;
        }
        #endregion
        #region "Public Accessors"
        public Texture2D Image
        {
            get
            {
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
        public string ImageName
        {
            get
            {
                if (_ImageName == "" || _ImageName == null)
                {
                    ImageName = "blank";
                }
                return _ImageName;
            }
            set
            {

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
                    _Image = AssetManager.Texture(_ImageName);
                }
            }
        }
        public bool Force = false;
        #endregion
        #region "Private Holders"
        private string _ImageName = "";
        private Texture2D _Image;
        #endregion
    }
}
