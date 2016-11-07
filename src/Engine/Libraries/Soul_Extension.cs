using System.IO;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An extension of the SoulLib's IO functions.
    /// </summary>
    public class IO : Soul.IO
    {
        /// <summary>
        /// Returns whether the content file exists.
        /// </summary>
        /// <param name="name">The name and path of the content file.</param>
        /// <returns></returns>
        public static bool GetContentExist(string name)
        {
            //Assign the path of the file.
            string contentpath = "Content\\SCon\\" + name.Replace("/", "\\") + ".xnb";
            //Check if the file exists.
            if (File.Exists(contentpath))
            {
                return true;
            }
            return false;
        }
    }
}

