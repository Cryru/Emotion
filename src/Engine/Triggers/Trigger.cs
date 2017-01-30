using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Triggers
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Custom implementation of an event system for SE.
    /// </summary>
    public class Trigger
    {
        #region "Declarations"
        public string Type;
        public object Sender;
        public object Data;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public Trigger(string type, object sender, object data = null)
        {
            Type = type;
            Sender = sender;
            Data = data;
        }
    }
}