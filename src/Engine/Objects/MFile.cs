using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul;
using System.IO;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A "managed" file. Data is recorded as JSON and is accessed through the "Content" dictionary.
    /// </summary>
    public class MFile
    {
        #region "Declarations"
        /// <summary>
        /// Keys and values stored in the file.
        /// </summary>
        public Dictionary<string, object> Content;
        /// <summary>
        /// The path of the file, for saving.
        /// </summary>
        private string Path;
        #endregion

        /// <summary>
        /// A SoulEngine managed file.
        /// </summary>
        /// <param name="Path">The path to the file relative to the exe.</param>
        /// <param name="DefaultFile">The default template, in case the file cannot be read.</param>
        public MFile(string Path, Dictionary<string, object> DefaultFile = null)
        {
            this.Path = Path;

            //Try to read the file and if we fail then create one using the defaults specified.
            try
            {
                if (System.IO.File.Exists(Path))
                {
                    string fileContent = IO.ReadFile(Path);
                    //Try to decrypt if encrypted.
                    fileContent = Encryption.TryDecrypt(fileContent, Settings.SecurityKey);

                    Content = JSON.fromJSON<Dictionary<string, object>>(fileContent);
                }
            }
            catch (Exception)
            {
                CreateFile(DefaultFile);
            }
        }

        /// <summary>
        /// Creates a new file using the specified default template.
        /// </summary>
        /// <param name="DefaultFile">The default template.</param>
        private void CreateFile(Dictionary<string, object> DefaultFile = null)
        {
            Content = DefaultFile;
            Save();
        }

        /// <summary>
        /// Saves the file to its path.
        /// </summary>
        /// <param name="Encrypt">Whether to encrypt the file using the default key.</param>
        public void Save(bool Encrypt = true)
        {
            //Check if file data is empty.
            if (Content == null) return;

            string content = JSON.toJSON(Content);
            if (Encrypt) content = Soul.Encryption.Encrypt(content, Settings.SecurityKey);
            IO.WriteFile(Path, content);
        }
        
    }
}
