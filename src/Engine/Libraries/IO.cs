using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
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
    /// Provides easy access to writing and reading files.
    /// </summary>
    class IO
    {
        /// <summary>
        /// Writes the string to a file. If the file doesn't exist it is created.
        /// </summary>
        /// <param name="filepath">The path of the file, with the root being the executable.</param>
        /// <param name="datatowrite">The data to write in the file.</param>
        public static void WriteFile(string filepath, string datatowrite)
        {
            try
            {
                StreamWriter writer = File.CreateText(filepath);
                //Write the contents.
                writer.Write(datatowrite);
                //Close the writer to free the file
                writer.Close();
                writer.Dispose();
            }
            catch { }

        }
        /// <summary>
        /// Writes the string array to a file with each index being a new line.
        /// </summary>
        /// <param name="filepath">The path of the file, with the root being the executable.</param>
        /// <param name="datatowrite">The string array to write to the file.</param>
        public static void WriteFile(string filepath, string[] datatowrite)
        {
            WriteFile(filepath, string.Join("\r\n", datatowrite));
        }
        /// <summary>
        /// Reads a file and returns the contents of the file as a string with "\r\n" characters separating the lines.
        /// </summary>
        /// <param name="filepath">The path of the file to read.</param>
        /// <returns>The contents of the file as a string.</returns>
        public static string ReadFile(string filepath)
        {
            string content;
            //Try to read the file.
            try
            {
                StreamReader reader = File.OpenText(filepath);
                //Transfer the contents onto the earlier defined variable.
                content = reader.ReadToEnd();
                //Close the reader to free the file.
                reader.Close();
                reader.Dispose();
                //Return the contents.
                return content;
            }
            catch { return null; }
        }
        /// <summary>
        /// Reads a file and returns the contents of the file as an array with each new line being an index in the array.
        /// </summary>
        /// <param name="filepath">The path of the file to read.</param>
        /// <returns>The contents of the file as a string array.</returns>
        public static string[] ReadFile(string filepath, bool returnArray)
        {
            string[] content;
            //Try to read the file.
            try
            {
                StreamReader reader = File.OpenText(filepath);
                //Transfer the contents onto the earlier defined variable.
                content = SplitNewLine(reader.ReadToEnd());
                //Close the reader to free the file.
                reader.Close();
                reader.Dispose();
                //Return the contents.
                return content;
            }
            catch { return null; }
        }
        /// <summary>
        /// Splits a string by the Windows new line ending.
        /// This might not be needed but for now it's been useful as Windows line endings are two characters.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <returns>The string split into an array by the Windows new line ending.</returns>
        public static string[] SplitNewLine(string s)
        {
            return s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
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
