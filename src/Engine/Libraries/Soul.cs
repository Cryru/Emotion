using System;
using System.IO;

namespace Soul
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul - A library of frequently used functions.                           //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    // Version 1                                                                //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Frequently used functions for writing and reading files.
    /// </summary>
    public partial class IO
    {
        #region "Writing"
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
        /// [Untested] Writes data to a specified stream.
        /// </summary>
        /// <param name="fileStream">The stream to write to.</param>
        /// <param name="datatowrite">The data to write.</param>
        public static void WriteFile(Stream fileStream, string datatowrite)
        {
            try
            {
                StreamWriter writer = new StreamWriter(fileStream);
                //Write the contents.
                writer.Write(datatowrite);
                writer.Flush();
                //Close the writer to free the file
                writer.Close();
                writer.Dispose();
            }
            catch { }

        }
        #endregion
        #region "Reading"
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
        /// Reads a file and returns the contents as an array with each new line being an index in the array.
        /// </summary>
        /// <param name="filepath">The path of the file to read.</param>
        /// <returns>The contents of the file as a string array.</returns>
        public static string[] ReadFile(string filepath, bool returnArray)
        {
            return SplitNewLine(ReadFile(filepath));
        }
        /// <summary>
        /// Reads a stream and returns its contents as a string with "\r\n" characters separating the lines.
        /// </summary>
        /// <param name="fileStream">The stream to read from.</param>
        /// <returns>The contents of the file as a string.</returns>
        public static string ReadFile(Stream fileStream)
        {
            string content;
            //Try to read the file.
            try
            {
                StreamReader reader = new StreamReader(fileStream, System.Text.Encoding.UTF8);
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
        /// Reads a stream and returns the contents as an array with each new line being an index in the array.
        /// </summary>
        /// <param name="fileStream">The stream to read from.</param>
        /// <returns>The contents of the file as a string array.</returns>
        public static string[] ReadFile(Stream fileStream, bool returnArray)
        {
            return SplitNewLine(ReadFile(fileStream));
        }
        #endregion
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
    }
}

