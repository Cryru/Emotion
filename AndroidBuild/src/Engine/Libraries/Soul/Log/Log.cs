using System;
using System.Collections.Generic;
using SoulEngine;
using System.IO;

namespace Soul
{

    //////////////////////////////////////////////////////////////////////////////
    // Log - A Soul Project                                                     //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // A logging system.                                                        //
    //////////////////////////////////////////////////////////////////////////////
    class Log
    {
        #region "Properties"
        //Module Properties
        public static string fr_name = "Log";
        public static string fr_ver = "0.1";
        public static string fr_linkedApp = Core.Name;

        //Logging Variables
        public static List<string> log = new List<string>(); //The current log stored in memory.
        public static DateTime currentLogStart; //The time the current logging started.
        public static int maxLog = 2000; //The maximum lines of log before it's written to a file.

        //Other
        public static bool enabled = true; //Whether the module is enabled.
        #endregion

        //Dumps the current log.
        public static void DumpLog(bool overwriteMax = false)
        {
            //Check if logging is enabled.
            if (enabled == false || log.Count <= 5)
            {
                return;
            }

            //Check if overwritting the maxLog variable and dumping.
            if(overwriteMax == false)
            {
                //Check if we are not overflowing.
                if (log.Count < maxLog)
                {
                    return;
                }
            }

            //Create a folder if missing.
            SoulLib_Ext.CreateMissing("Logs");
            //Write the file.
            string file = String.Format("Logs//Log-{0:hh mm ss}-To-{1:hh mm ss}", currentLogStart, DateTime.Now) + ".txt";
            SoulLib_Ext.IO_WriteFile_Array(file, log.ToArray());
            //Start a new log.
            NewLog();
        }
        //Starts a new log.
        public static void NewLog()
        {
            //Check if logging is enabled.
            if (enabled == false)
            {
                return;
            }

            log.Clear();
            log.Add("--------------------");
            log.Add(fr_name + " " + fr_ver + ": Application(" + fr_linkedApp + ")" ); //The Log version and application stamp.
            log.Add("This log started at: " + DateTime.Now); //The time the log started.
            log.Add("--------------------");
            log.Add("");

            currentLogStart = DateTime.Now;
        }
        //Adds the data to the log.
        public static void Add(string data)
        {
            //Check if logging is enabled.
            if (enabled == false)
            {
                return;
            }

            //Dump the log if overflowing.
            DumpLog();
            //Add the specified line to the log with a timestamp.
            log.Add(String.Format("[{0:hh:mm:ss:fff}] ", DateTime.Now) + data);
        }
    }
}
public class SoulLib_Ext
{
    #region "Create"
    //---------------------------------------------------------------------------------------------------------------------------
    //Creates the defined folder if it's missing.
    public static void CreateMissing(string folder)
    {
        if (!(Directory.Exists(folder)))
        {
            Directory.CreateDirectory(folder);
        }
    }
    #region "IO"
    //---------------------------------------------------------------------------------------------------------------------------
    //Writes the specified string to the specified file, if it doesn't exist it will be created.
    public static void IO_WriteFile(string filepath, string datatowrite)
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
    //---------------------------------------------------------------------------------------------------------------------------
    //Writes the specified string array to the specified file, if it doesn't exist it will be created.
    public static void IO_WriteFile_Array(string filepath, string[] datatowrite)
    {
        IO_WriteFile(filepath, ArrayToString(datatowrite));
    }
    //---------------------------------------------------------------------------------------------------------------------------
    //Reads the specified file and returns a string holding the contents.
    public static string IO_ReadFile(string filepath)
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
    //---------------------------------------------------------------------------------------------------------------------------
    //Reads the specified file and returns a string holding the contents.
    public static string[] IO_ReadFile_Array(string filepath)
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
    #endregion
    #region "Other"
    //---------------------------------------------------------------------------------------------------------------------------
    //Split the string at new lines.
    public static string[] SplitNewLine(string s)
    {
        return s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }
    #endregion
    #region "String Arrays and Strings"
    //---------------------------------------------------------------------------------------------------------------------------
    //String Array > Single String
    public static string ArrayToString(string[] arr, string LineSeparator = "/NEWLINE/")
    {
        //Check if the array is empty, not long enough or with an invalid delete index.
        if (arr == null || arr.Length == 0)
        {
            return "Empty Array";
        }
        //Convert line separator tags.
        if (LineSeparator == "/NEWLINE/") LineSeparator = Environment.NewLine;
        //Declare a variable to hold the line.
        string singleline = arr[0];

        //Check if the array is longer than one.
        if (!(arr.Length == 1))
        {
            //Look through all lines.
            for (int i = 1; i <= arr.Length - 1; i++)
            {
                singleline += LineSeparator + arr[i];
            }
        }
        //Return line.
        return singleline;
    }
    //Single String > String Array
    public static string[] StringToArray(string s, char LineSeparator)
    {
        string[] arr;

        //Convert line separator tags.
        arr = s.Split(LineSeparator);
        //Remove empty array entries.

        //Return the converter string.
        return arr;
    }
    #endregion
    #endregion
}
