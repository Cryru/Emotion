// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.IO;
using System.Xml;

#endregion

namespace Soul.Engine.BuildHelper
{
    internal class Program
    {
        /// <summary>
        /// The path to the SoulEngine project.
        /// </summary>
        public static string ProjectPath;
        /// <summary>
        /// The path where the project will be built.
        /// </summary>
        public static string OutputPath;


        private static void Main(string[] args)
        {
            Console.WriteLine("================================");
            Console.WriteLine("SoulEngine Build Helper Version " + Meta.Version);
            Console.WriteLine("================================");

            if (args.Length < 2 || args.Length > 2)
            {
                Console.WriteLine(
                    "Please supply the right command arguments to the build helper. Argument one should be the project folder, and argument two should be the output folder.");
                return;
            }

            Console.WriteLine("Looking for the SoulEngine project at \"" + args[0] + "\"...");

            // Look for the C# project file.
            if (!File.Exists(Path.Combine(args[0], "SoulEngine.csproj")))
            {
                Console.WriteLine("Project not found!");
                return;
            }

            // Check if the output folder exists.
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("The output directory - \"" + args[1] + "\" doesn't exist!");
                return;
            }

            Console.WriteLine("Project found, starting installation at \"" + args[1] + "\"...");

            // Set globals.
            ProjectPath = args[0];
            OutputPath = args[1];

            // Install Raya.
            InstallRaya();

            // Ready!
            Console.WriteLine("Installation Complete");
        }

        /// <summary>
        /// Installs the Raya library.
        /// Copies the Raya.dll.config to the output folder.
        /// Copies the lib folder to the output folder.
        /// </summary>
        private static void InstallRaya()
        {
            Console.WriteLine("Looking for Raya...");

            string rayaPath = Path.Combine(ProjectPath, "Libraries", "Raya");
            
            if (!Directory.Exists(rayaPath))
            {
                Console.WriteLine("Raya libraries are missing from the project folder!");
                return;
            }

            Console.WriteLine("Installing Raya config...");
            string dllConfigPath = Path.Combine(rayaPath, "Raya.dll.config");
            string dllOutputPath = Path.Combine(OutputPath, "Raya.dll.config");

            // Move the dll.config
            if (!File.Exists(dllConfigPath))
            {
                Console.WriteLine("The Raya DLL config is missing!");
                return;
            }
            else
            {
                // Copy the file.
                File.Copy(dllConfigPath, dllOutputPath, true);
            }

            Console.WriteLine("Installing Raya external libraries...");
            string externalLibs = Path.Combine(rayaPath, "lib");
            string externalLibsOutput = Path.Combine(OutputPath, "lib");

            // Move the dll.config
            if (!Directory.Exists(externalLibs))
            {
                Console.WriteLine("The Raya external libraries are missing!");
                return;
            }
            else
            {
                // Copy the folder.
                Utilities.CopyFolder(externalLibs, externalLibsOutput);
            }
        }
    }
}