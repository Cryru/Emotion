using System;
using System.IO;

namespace Soul
{
	//////////////////////////////////////////////////////////////////////////////
	// Soul - A library of frequently used functions.                           //
	// Copyright © 2016 Vlad Abadzhiev                                          //
    // Extension for SoulEngine                                                 //
	//////////////////////////////////////////////////////////////////////////////

	public partial class IO
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

