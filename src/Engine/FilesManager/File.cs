using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul;
using System.IO;

namespace SoulEngine.FilesManager
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    public class File
    {
        #region "Declarations"

        private Dictionary<string, object> Content;
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Path"></param>
        public File(string Name, string Path = "Files/")
        {
            try
            {
                if (System.IO.File.Exists(Path + Name))
                {

                }
            }
            catch (Exception)
            {
                CreateFile();
            }
        }

        private void CreateFile()
        {

        }

        public void Save()
        {

        }
        
    }
}
