#region Using

using System;
using System.IO;
using System.Xml.Serialization;
using Emotion.Utility;

#endregion

namespace Adfectus.Game
{
    /// <summary>
    /// A save file.
    /// </summary>
    /// <typeparam name="T">The model for the save file.</typeparam>
    public class SaveFile<T>
    {
        /// <summary>
        /// The content of the file.
        /// </summary>
        public T Content { get; set; }

        private XmlSerializer _serializer;
        private string _path;

        /// <summary>
        /// Create a new save file.
        /// </summary>
        /// <param name="filePath">The path to the save file.</param>
        public SaveFile(string filePath)
        {
            _serializer = new XmlSerializer(typeof(T));
            _path = Helpers.CrossPlatformPath(filePath);

            // Check if the file exists. If it doesn't - create it.
            if (!File.Exists(_path))
            {
                Content = Activator.CreateInstance<T>();
                Save();
                return;
            }

            string content = File.ReadAllText(_path);

            // Read the file.
            using (TextReader writer = new StringReader(content))
            {
                Content = (T) _serializer.Deserialize(writer);
            }
        }

        /// <summary>
        /// Save the save file to the disk.
        /// </summary>
        public void Save()
        {
            // If an old save file exists, back it up.
            if (File.Exists(_path)) File.Copy(_path, _path + ".backup", true);

            using (StreamWriter stream = new StreamWriter(_path))
            {
                _serializer.Serialize(stream, Content);
            }
        }
    }
}