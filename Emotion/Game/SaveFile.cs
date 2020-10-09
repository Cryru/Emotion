#region Using

using System;
using System.Text;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;
using Emotion.Standard.XML;

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
        protected SaveFile(string filePath)
        {
            Name = filePath;
        }

        /// <summary>
        /// Save the save file to the asset store.
        /// </summary>
        public void Save()
        {
            string data = XMLFormat.To(Content);
            if (!Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(data), Name)) Engine.Log.Warning($"Couldn't save file {Name}.", MessageSource.Other);
        }

        /// <summary>
        /// Load a save file via the asset loader, or create a new one, if missing.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SaveFile<T> LoadSaveOrCreate(string name)
        {
            if (Engine.AssetLoader.Exists(name)) return Engine.AssetLoader.Get<SaveFile<T>>(name);
            // If the file doesn't exist - create it.
            var newFile = new SaveFile<T>(name) {Content = Activator.CreateInstance<T>()};
            newFile.Save();
            return newFile;
        }
    }
}