#region Using

using System;
using System.Text;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Game
{
    /// <summary>
    /// A save file.
    /// </summary>
    /// <typeparam name="T">The model for the save file.</typeparam>
    public class SaveFile<T> : XMLAsset<T>
    {
        /// <summary>
        /// This is the constructor used by the AssetLoader.
        /// </summary>
        public SaveFile()
        {
        }

        /// <summary>
        /// Create a new save file.
        /// </summary>
        /// <param name="filePath">The path to the save file.</param>
        public SaveFile(string filePath)
        {
            Name = filePath;

            // If the file doesn't exist - create it.
            if (Engine.AssetLoader.Exists(Name)) return;
            Content = Activator.CreateInstance<T>();
            Save();
        }

        /// <summary>
        /// Save the save file to the asset store.
        /// </summary>
        public void Save()
        {
            string data = FromObject(Content);
            if (!Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(data), Name)) Engine.Log.Warning($"Couldn't save file {Name}.", MessageSource.Other);
        }
    }
}