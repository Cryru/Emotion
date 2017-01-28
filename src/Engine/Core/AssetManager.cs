using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Manages asset integrity.
    /// </summary>
    static class AssetManager
    {

        public static Texture2D MissingTexture;

        /// <summary>
        /// Reads the assets meta file, and applies checks for validity.
        /// If true is returned then files are as they should be, otherwise false is returned.
        /// </summary>
        public static bool AssertAssets()
        {
            /*
             * The meta.soul file is built like a standard .ini file.
             * Each new file's data starts with a [File] header followed by properties.
             * The "Path" and "Hash" properties are used to check for file tampering and validity.
             */
            
            //Check if the file exists.
            if (!(System.IO.File.Exists("Content\\meta.soul"))) return false;

            //Read the file.
            string[] file = Soul.IO.ReadFile("Content\\meta.soul", true);

            //Check if the file is empty.
            if (file.Length <= 1) return true;

            //Check if the file is encrypted, and if it is, decrypt it using the internal security key.
            bool encrypted = bool.Parse(file[0].Substring("Encryption=".Length));
            if (encrypted == true) Soul.Encryption.Decrypt(string.Join("\r\n", file), Settings.SecurityKey);

            //The last file we checked.
            string lastPath = "";

            //Iterate through each line.
            for (int i = 0; i < file.Length; i++)
            {
                if (file[i] == "[File]") lastPath = ""; //Check if starting data for a new file.
                else if (file[i].Substring(0, "Path=".Length) == "Path=") lastPath = file[i].Substring("Path=".Length); //Assign path for the current file.
                else if (file[i].Substring(0, "Hash=".Length) == "Hash=") if (Soul.Encryption.GetMD5("Content\\" + lastPath) != file[i].Substring("Path=".Length)) return false; //Check hash.
            }

            //If everything went smoothly, return true.
            return true;
        }

    }
}
