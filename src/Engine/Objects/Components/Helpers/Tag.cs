using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    public abstract class Tag
    {
        #region "Variables"
        /// <summary>
        /// 
        /// </summary>
        public int Start
        {
            get
            {
                return start;
            }
        }
        private int start;
        /// <summary>
        /// 
        /// </summary>
        public int? End;
        /// <summary>
        /// 
        /// </summary>
        public bool Active;
        /// <summary>
        /// 
        /// </summary>
        public string Data
        {
            get
            {
                return data;
            }
        }
        private string data;
        #endregion

        #region "Functions"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract CharData onStart(CharData c);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract CharData onDuration(CharData c);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract CharData onEnd(CharData c);
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        public Tag(string Data, int Start, int? End)
        {
            start = Start;
            this.End = End;
            data = Data;
        }
    }
}
