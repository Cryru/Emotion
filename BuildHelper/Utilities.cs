using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soul.Engine.BuildHelper
{
    public static class Utilities
    {
        /// <summary>
        /// Copies the contents of one directory to another.
        /// </summary>
        /// <param name="sourcePath">The path to copy from.</param>
        /// <param name="destinationPath">The path to copy to.</param>
        public static void CopyFolder(string sourcePath, string destinationPath)
        {
            // Recreate all folders from the source.
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
            }

            // Copy all files from the source.
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
            }
        }
    }
}
